namespace SetComparison

open System

module private InteractionTrackerHelpers =

    let inline computeBucketAndMask bytesPerItem itemCount (itemKey1: int<'Measure>) (itemKey2: int<'Measure>) =
        
        if (uint itemKey2) >= (uint itemCount) then
            raise (IndexOutOfRangeException (nameof itemKey2))

        let location  = int itemKey2
        // The Byte we are interested will be in the range of bytes for itemKey1
        // so we need to add the offset for itemKey1
        let bucket    = (location >>> 6) + ((int itemKey1) * bytesPerItem)
        let offset    = location &&& 0b111111
        let mask      = 1UL <<< offset
        bucket, mask


type InteractionTracker<[<Measure>] 'Measure> (capacity: int) =
    // How many Bytes will we need per Item for tracking the
    // interaction of Item * Item
    let uint64PerItem = (capacity + 63) >>> 6
    // Calculate the number of bytes required by dividing by 8 with rounding up
    let uint64Required = uint64PerItem * capacity
    // Create the Byte array for storing the bits
    let buckets : uint64[] = Array.zeroCreate uint64Required
    
    member _.BytesPerItem = uint64PerItem
    member _.Capacity = capacity
    member _._buckets = buckets
    
    member b.Item
        with inline get (itemKey1: int<'Measure>, itemKey2: int<'Measure>) =
            let bucketId, mask = InteractionTrackerHelpers.computeBucketAndMask b.BytesPerItem b.Capacity itemKey1 itemKey2
            let buckets = b._buckets
            let bucket = buckets[bucketId]
            (bucket &&& mask) <> 0UL
    
    member b.Clear () =
        for i = 0 to b._buckets.Length - 1 do
            b._buckets[i] <- 0UL
    
    member b.Contains (itemKey1: int<'Measure>, itemKey2: int<'Measure>) =
        let bucketId, mask = InteractionTrackerHelpers.computeBucketAndMask b.BytesPerItem b.Capacity itemKey1 itemKey2
        let buckets = b._buckets
        let bucket = buckets[bucketId]
        (bucket &&& mask) <> 0UL
    
    member inline b.Add (itemKey1: int<'Measure>, itemKey2: int<'Measure>) =
        let bucketId, mask = InteractionTrackerHelpers.computeBucketAndMask b.BytesPerItem b.Capacity itemKey1 itemKey2
        let buckets = b._buckets
        let bucket = buckets[bucketId]
        buckets[bucketId] <- bucket ||| mask

    member inline b.Remove (itemKey1: int<'Measure>, itemKey2: int<'Measure>) =
        let bucketId, mask = InteractionTrackerHelpers.computeBucketAndMask b.BytesPerItem b.Capacity itemKey1 itemKey2
        let buckets = b._buckets
        let bucket = buckets[bucketId]
        buckets[bucketId] <- bucket &&& ~~~mask
        
    member this.CheckForMatch (itemKey: int<'Measure>, bitSet: BitSet<'Measure>) =
        if this.Capacity <> bitSet.Capacity then
            invalidArg (nameof bitSet) "Cannot check for match with BitSet of different capacity"
        
        let buckets = this._buckets
        let bytesPerItem = this.BytesPerItem
        let itemBytesIndex = (int itemKey) * bytesPerItem
        let thisValues = ReadOnlySpan (buckets, itemBytesIndex, bytesPerItem)
        let thatValues = bitSet.Values
        
        let mutable i = 0
        let mutable result = true
        while i < thisValues.Length && result do
            result <- (thisValues[i] = thatValues[i])    
            i <- i + 1
        
        result
        
    static member create (capacity: int<'Measure>) =
        InteractionTracker<'Measure> (int capacity)