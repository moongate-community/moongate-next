namespace Moongate.Core.Buffers;

internal sealed class STArrayPoolThreadState<T>
{
    public readonly STArrayPoolBucket<T>[] CacheBuckets;
    public readonly STArrayPoolStack<T>?[] Buckets;

    public STArrayPoolThreadState(int bucketCount)
    {
        CacheBuckets = new STArrayPoolBucket<T>[bucketCount];
        Buckets = new STArrayPoolStack<T>?[bucketCount];
    }
}
