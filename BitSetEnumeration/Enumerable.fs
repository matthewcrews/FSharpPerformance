module BitSetEnumeration.Enumerable


open System
open System.Collections.Generic

module private Helpers =

    let inline computeBucketAndMask (itemKey: int<'Item>) =
        let location = int itemKey
        let bucket = location >>> 6
        let offset = location &&& 0x3F
        let mask = 1UL <<< offset
        bucket, mask


type BitSetEnumerator<[<Measure>] 'Measure>(buckets: uint64[]) =
    let mutable bucketIdx = 0
    let mutable curBucket = 0UL
    let mutable curItem = LanguagePrimitives.Int32WithMeasure<'Measure> -1

    member _.Reset() =
        bucketIdx <- 0
        curBucket <- 0UL
        curItem <- LanguagePrimitives.Int32WithMeasure<'Measure> -1

    member _.Current =
        if curItem < 0<_> then
            raise (InvalidOperationException "Enumeration has not started. Call MoveNext.")
        else
            curItem

    member b.MoveNext() =
        // Check if we have actually started iteration
        if curItem < 0<_> then
            curBucket <- buckets[bucketIdx]

        // There are still items in the Current bucket we should return
        if curBucket <> 0UL then
            let r = System.Numerics.BitOperations.TrailingZeroCount curBucket
            curItem <- LanguagePrimitives.Int32WithMeasure<'Measure>((bucketIdx <<< 6) + r)
            curBucket <- curBucket ^^^ (1UL <<< r)
            true

        // We need to move to the next bucket of items
        else
            bucketIdx <- bucketIdx + 1

            if bucketIdx < buckets.Length then
                curBucket <- buckets[bucketIdx]
                b.MoveNext()
            else
                false

    interface IEnumerator<int<'Measure>> with
        member b.Current = b.Current :> Object
        member b.Current = b.Current
        member b.MoveNext() = b.MoveNext()
        member b.Reset() = b.Reset()
        member b.Dispose() = ()



type BitSet<[<Measure>] 'Measure>(buckets: uint64[]) =

    new(capacity: int) =
        let bucketsRequired = (capacity + 63) >>> 6
        let buckets: uint64[] = Array.zeroCreate bucketsRequired
        BitSet<_> buckets


    /// WARNING: Public for inlining
    member _._buckets = buckets
    member _.Capacity = buckets.Length * 64
    member _.Values: ReadOnlySpan<uint64> = ReadOnlySpan buckets

    member inline b.Count =
        let mutable total = 0

        for bucket in b._buckets do
            total <- total + System.Numerics.BitOperations.PopCount bucket

        total

    member inline b.Item
        with get (itemKey: int<'Measure>) =
            let bucketId, mask = Helpers.computeBucketAndMask itemKey
            let buckets = b._buckets
            let bucket = buckets[bucketId]
            (bucket &&& mask) <> 0UL

    member b.Clear() =
        for i = 0 to b._buckets.Length - 1 do
            b._buckets[ i ] <- 0UL

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

    interface System.Collections.IEnumerable with
        member b.GetEnumerator() =
            (new BitSetEnumerator<'Measure>(buckets)) :> System.Collections.IEnumerator

    interface IEnumerable<int<'Measure>> with
        member s.GetEnumerator() = new BitSetEnumerator<'Measure>(buckets)
