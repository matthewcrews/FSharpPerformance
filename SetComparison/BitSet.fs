namespace SetComparison

open System
open System.Collections
open System.Collections.Generic

module private BitSetHelpers =

    let inline computeBucketAndMask (itemKey: int<'Item>) itemCount =

        if (uint itemKey) >= (uint itemCount) then
            raise (IndexOutOfRangeException (nameof itemKey))

        let location  = int itemKey
        let bucket    = location >>> 6
        let offset    = location &&& 0b111111
        let mask      = 1UL <<< offset
        bucket, mask

type BitSet<[<Measure>] 'Measure> (capacity: int) =
    // Calculate the number of bytes required by dividing by 8 with rounding up
    let uint64Required = (capacity + 63) >>> 6
    // Create the Byte array for storing the bits
    let buckets : uint64[] = Array.zeroCreate uint64Required
    
    member _.Capacity = capacity
    // These need to be public to support inlining
    member _._buckets = buckets
    
    member _.Values = ReadOnlySpan buckets

    member inline b.Count =
        let mutable total = 0
        
        for bucket in b._buckets do
            total <- total + System.Numerics.BitOperations.PopCount bucket
        
        total
    
    member b.Item
        with inline get (itemKey: int<'Measure>) =
            let bucketId, mask = BitSetHelpers.computeBucketAndMask itemKey b.Capacity
            let buckets = b._buckets
            let bucket = buckets[bucketId]
            (bucket &&& mask) <> 0UL
    
    member b.Clear () =
        for i = 0 to b._buckets.Length - 1 do
            b._buckets[i] <- 0UL
    
    member b.Contains (itemKey: int<'Measure>) =
        let bucketId, mask = BitSetHelpers.computeBucketAndMask itemKey b.Capacity
        let buckets = b._buckets
        let bucket = buckets[bucketId]
        (bucket &&& mask) <> 0UL
    
    member inline b.Add (itemKey: int<'Measure>) =
        let bucketId, mask = BitSetHelpers.computeBucketAndMask itemKey b.Capacity
        let buckets = b._buckets
        let bucket = buckets[bucketId]
        buckets[bucketId] <- bucket ||| mask

    member inline b.Remove (itemKey: int<'Measure>) =
        let bucketId, mask = BitSetHelpers.computeBucketAndMask itemKey b.Capacity
        let buckets = b._buckets
        let bucket = buckets[bucketId]
        buckets[bucketId] <- bucket &&& ~~~mask
        
    static member create (count: int<'Measure>) =
        BitSet<'Measure> (int count)