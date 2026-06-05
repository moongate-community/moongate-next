// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using Moongate.Core.Types;

namespace Moongate.Core.Buffers;

/// <summary>
/// Thread-safe ArrayPool adaptation. Each calling thread keeps its own
/// bucket cache via <see cref="ThreadLocal{T}" />, avoiding cross-thread
/// contention while letting <see cref="Trim" /> reach every thread's state
/// during Gen2 GC callbacks.
/// </summary>
[SuppressMessage(
    "Reliability",
    "CA1001:Types that own disposable fields should be disposable",
    Justification = "STArrayPool is a process-lifetime singleton; the ThreadLocal handle is intentionally never disposed."
)]
public sealed class STArrayPool<T> : ArrayPool<T>
{
#if DEBUG_ARRAYPOOL
    private static readonly ConditionalWeakTable<T[], STArrayPoolRentReturnStatus> _rentedArrays = new();
#endif

    private const int StackArraySize = 32;
    private const int BucketCount = 27; // SelectBucketIndex(1024 * 1024 * 1024 + 1)

    public static STArrayPool<T> Shared { get; } = new();

    private readonly ThreadLocal<STArrayPoolThreadState<T>> _state =
        new(static () => new(BucketCount), true);

    private STArrayPool()
    {
        Gen2GcCallback.Register(static o => ((STArrayPool<T>)o).Trim(), this);
    }

    public override T[] Rent(int minimumLength)
    {
        T[]? buffer;

        var bucketIndex = SelectBucketIndex(minimumLength);
        var state = _state.Value!;
        var cacheBuckets = state.CacheBuckets;

        if ((uint)bucketIndex < (uint)cacheBuckets.Length)
        {
            buffer = cacheBuckets[bucketIndex].Array;

            if (buffer is not null)
            {
                cacheBuckets[bucketIndex].Array = null;
            #if DEBUG_ARRAYPOOL
                _rentedArrays.AddOrUpdate(
                    buffer,
                    new STArrayPoolRentReturnStatus { IsRented = true }
                );
            #endif
                return buffer;
            }
        }

        var buckets = state.Buckets;

        if ((uint)bucketIndex < (uint)buckets.Length)
        {
            var b = buckets[bucketIndex];

            if (b is not null)
            {
                buffer = b.TryPop();

                if (buffer is not null)
                {
                #if DEBUG_ARRAYPOOL
                    _rentedArrays.AddOrUpdate(
                        buffer,
                        new STArrayPoolRentReturnStatus { IsRented = true }
                    );
                #endif
                    return buffer;
                }
            }

            minimumLength = GetMaxSizeForBucket(bucketIndex);
        }

        if (minimumLength == 0)
        {
            // We aren't renting.
            return [];
        }

        if (minimumLength < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumLength));
        }

        var array = GC.AllocateUninitializedArray<T>(minimumLength);

    #if DEBUG_ARRAYPOOL
        _rentedArrays.AddOrUpdate(
            array,
            new STArrayPoolRentReturnStatus { IsRented = true, StackTrace = Environment.StackTrace }
        );
    #endif

        return array;
    }

    public override void Return(T[]? array, bool clearArray = false)
    {
        if (array is null)
        {
            return;
        }

        var bucketIndex = SelectBucketIndex(array.Length);
        var state = _state.Value!;
        var cacheBuckets = state.CacheBuckets;

        if ((uint)bucketIndex < (uint)cacheBuckets.Length)
        {
            if (clearArray)
            {
                Array.Clear(array);
            }

        #if DEBUG_ARRAYPOOL
            if (array.Length != GetMaxSizeForBucket(bucketIndex) || !_rentedArrays.TryGetValue(array, out var status))
            {
                throw new ArgumentException("Buffer is not from the pool", nameof(array));
            }

            if (!status!.IsRented)
            {
                throw new InvalidOperationException($"Array has already been returned.\nOriginal StackTrace:{status.StackTrace}\n");
            }

            // Mark it as returned
            status.IsRented = false;
            status.StackTrace = Environment.StackTrace;
        #else
            if (array.Length != GetMaxSizeForBucket(bucketIndex))
            {
                throw new ArgumentException("Buffer is not from the pool", nameof(array));
            }
        #endif

            ref var bucketArray = ref cacheBuckets[bucketIndex];
            var prev = bucketArray.Array;
            bucketArray = new(array);

            if (prev is not null)
            {
                var bucket = state.Buckets[bucketIndex] ?? CreateBucketStack(state, bucketIndex);
                bucket.TryPush(prev);
            }
        }
    }

    public bool Trim()
    {
        var ticks = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
        var pressure = GetMemoryPressure();

        foreach (var state in _state.Values)
        {
            if (state is null)
            {
                continue;
            }

            TrimState(state, ticks, pressure);
        }

        return true;
    }

    private static void TrimState(STArrayPoolThreadState<T> state, long ticks, STArrayPoolMemoryPressureType pressure)
    {
        var buckets = state.Buckets;

        for (var i = 0; i < buckets.Length; i++)
        {
            buckets[i]?.Trim(ticks, pressure, GetMaxSizeForBucket(i));
        }

        var cacheBuckets = state.CacheBuckets;

        // Under high pressure, release all cached buckets
        if (pressure == STArrayPoolMemoryPressureType.High)
        {
            Array.Clear(cacheBuckets);

            return;
        }

        uint threshold = pressure switch
        {
            STArrayPoolMemoryPressureType.Medium => 10000,
            _                                    => 30000
        };

        for (var i = 0; i < cacheBuckets.Length; i++)
        {
            ref var b = ref cacheBuckets[i];

            if (b.Array is null)
            {
                continue;
            }

            var lastSeen = b.Ticks;

            if (lastSeen == 0)
            {
                b.Ticks = ticks;
            }
            else if (ticks - lastSeen >= threshold)
            {
                b.Array = null;
                b.Ticks = 0;
            }
        }
    }

    private static STArrayPoolStack<T> CreateBucketStack(STArrayPoolThreadState<T> state, int bucketIndex)
        => state.Buckets[bucketIndex] = new(StackArraySize);

    // Buffers are bucketed so that a request between 2^(n-1) + 1 and 2^n is given a buffer of 2^n
    // Bucket index is log2(bufferSize - 1) with the exception that buffers between 1 and 16 bytes
    // are combined, and the index is slid down by 3 to compensate.
    // Zero is a valid bufferSize, and it is assigned the highest bucket index so that zero-length
    // buffers are not retained by the pool. The pool will return the Array.Empty singleton for these.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int SelectBucketIndex(int bufferSize)
        => BitOperations.Log2(((uint)bufferSize - 1) | 15) - 3;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetMaxSizeForBucket(int binIndex)
    {
        var maxSize = 16 << binIndex;
        Debug.Assert(maxSize >= 0);

        return maxSize;
    }

    internal static STArrayPoolMemoryPressureType GetMemoryPressure()
    {
        var memoryInfo = GC.GetGCMemoryInfo();

        if (memoryInfo.MemoryLoadBytes >= memoryInfo.HighMemoryLoadThresholdBytes * 0.90)
        {
            return STArrayPoolMemoryPressureType.High;
        }

        if (memoryInfo.MemoryLoadBytes >= memoryInfo.HighMemoryLoadThresholdBytes * 0.70)
        {
            return STArrayPoolMemoryPressureType.Medium;
        }

        return STArrayPoolMemoryPressureType.Low;
    }
}
