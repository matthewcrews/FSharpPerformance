module BitSetEnumeration.Iter

open System

module private Helpers =

    let inline computeBucketAndMask (itemKey: int<'Item>) =
        let location = int itemKey
        let bucket = location >>> 6
        let offset = location &&& 0x3F
        let mask = 1UL <<< offset
        bucket, mask


[<Struct>]
type BitSet<[<Measure>] 'Measure>(buckets: uint64[]) =

    new(capacity: int) =
        let bucketsRequired = (capacity + 63) >>> 6
        let buckets: uint64[] = Array.zeroCreate bucketsRequired
        BitSet<_> buckets

    /// WARNING: Public for inlining
    member _._buckets = buckets
    member _.Capacity = buckets.Length * 64

    member b.Count =
        let mutable total = 0

        for bucket in b._buckets do
            total <- total + System.Numerics.BitOperations.PopCount bucket

        total

    member b.Item
        with get (itemKey: int<'Measure>) =
            let bucketId, mask = Helpers.computeBucketAndMask itemKey
            let buckets = b._buckets
            let bucket = buckets[bucketId]
            (bucket &&& mask) <> 0UL

    member b.Contains(itemKey: int<'Measure>) =
        let bucketId, mask = Helpers.computeBucketAndMask itemKey
        let buckets = b._buckets
        let bucket = buckets[bucketId]
        (bucket &&& mask) <> 0UL

    member b.Add(itemKey: int<'Measure>) =
        let bucketId, mask = Helpers.computeBucketAndMask itemKey
        let bucket = buckets[bucketId]
        buckets[bucketId] <- bucket ||| mask

    member b.Remove(itemKey: int<'Measure>) =
        let bucketId, mask = Helpers.computeBucketAndMask itemKey
        let buckets = b._buckets
        let bucket = buckets[bucketId]
        buckets[bucketId] <- bucket &&& ~~~mask

    member b.Clear() =
        for i = 0 to b._buckets.Length - 1 do
            b._buckets[ i ] <- 0UL


module BitSet =

    let inline iter ([<InlineIfLambda>] f: int<'Measure> -> unit) (b: BitSet<'Measure>) =
        let mutable i = 0

        // Source of algorithm: https://lemire.me/blog/2018/02/21/iterating-over-set-bits-quickly/
        while i < b._buckets.Length do
            let mutable bitSet = b._buckets[i]

            while bitSet <> 0UL do
                let r = System.Numerics.BitOperations.TrailingZeroCount bitSet
                let itemId = (i <<< 6) + r |> LanguagePrimitives.Int32WithMeasure<'Measure>

                (f itemId)

                bitSet <- bitSet ^^^ (1UL <<< r)

            i <- i + 1
