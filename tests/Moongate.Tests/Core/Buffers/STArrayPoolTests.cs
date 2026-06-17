using Moongate.Core.Buffers;

namespace Moongate.Tests.Core.Buffers;

public class STArrayPoolTests
{
    [Fact]
    public void Rent_FromMultipleThreads_DoesNotCorruptState()
    {
        // Regression: previous implementation shared bucket arrays across threads
        // and would deadlock or corrupt under concurrent access. With ThreadLocal
        // state, each thread sees its own buckets.
        const int threadCount = 8;
        const int iterations = 1000;
        var pool = STArrayPool<byte>.Shared;
        var errors = 0;

        var threads = new Thread[threadCount];

        for (var t = 0; t < threadCount; t++)
        {
            threads[t] = new Thread(() =>
                {
                    try
                    {
                        for (var i = 0; i < iterations; i++)
                        {
                            var buffer = pool.Rent(256);
                            Assert.True(buffer.Length >= 256);
                            pool.Return(buffer);
                        }
                    }
                    catch
                    {
                        Interlocked.Increment(ref errors);
                    }
                }
            );
        }

        foreach (var thread in threads)
        {
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        Assert.Equal(0, errors);
    }

    [Fact]
    public void Rent_NegativeLength_Throws()
    {
        var pool = STArrayPool<byte>.Shared;
        Assert.Throws<ArgumentOutOfRangeException>(() => pool.Rent(-1));
    }

    [Fact]
    public void Rent_ReturnsArrayOfAtLeastRequestedLength()
    {
        var pool = STArrayPool<int>.Shared;
        var array = pool.Rent(100);

        try
        {
            Assert.NotNull(array);
            Assert.True(array.Length >= 100);
        }
        finally
        {
            pool.Return(array);
        }
    }

    [Fact]
    public void Rent_ZeroLength_ReturnsEmptyArray()
    {
        var pool = STArrayPool<byte>.Shared;
        var array = pool.Rent(0);

        Assert.Empty(array);
    }

    [Fact]
    public void RentReturn_RoundTrip_ReusesBuffer()
    {
        var pool = STArrayPool<long>.Shared;
        var first = pool.Rent(64);
        pool.Return(first);

        var second = pool.Rent(64);

        try
        {
            Assert.Same(first, second);
        }
        finally
        {
            pool.Return(second);
        }
    }

    [Fact]
    public void Return_ArrayNotFromPool_Throws()
    {
        var pool = STArrayPool<int>.Shared;
        var foreign = new int[100]; // length doesn't match a pool bucket size

        Assert.Throws<ArgumentException>(() => pool.Return(foreign));
    }

    [Fact]
    public void Return_NullArray_DoesNotThrow()
    {
        var pool = STArrayPool<int>.Shared;
        pool.Return(null);
    }
}
