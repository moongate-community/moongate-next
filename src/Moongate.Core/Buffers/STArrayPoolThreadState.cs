namespace Moongate.Core.Buffers;

internal sealed class STArrayPoolThreadState<T>
{
    public readonly STArrayPoolStack<T>?[] Buckets;
    public readonly STArrayPoolBucket<T>[] CacheBuckets;

    public STArrayPoolThreadState(int bucketCount)
    {
        CacheBuckets = new STArrayPoolBucket<T>[bucketCount];
        Buckets = new STArrayPoolStack<T>?[bucketCount];
    }
}
