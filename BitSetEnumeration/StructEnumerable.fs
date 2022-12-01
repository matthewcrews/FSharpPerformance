module BitSetEnumeration.StructEnumerable


open System
open System.Collections.Generic

module private Helpers =

    let inline computeBucketAndMask (itemKey: int<'Item>) =
        let location = int itemKey
        let bucket = location >>> 6
        let offset = location &&& 0x3F
        let mask = 1UL <<< offset
        bucket, mask


[<Struct>]
type BitSetEnumerator<[<Measure>] 'Measure> =
    val mutable private BucketIdx: int
    val mutable private CurBucket: uint64
    val mutable private CurItem: int<'Measure>
    val private Buckets: uint64[]

    new(buckets: uint64[]) =
        {
            BucketIdx = 0
            CurBucket = 0UL
            CurItem = LanguagePrimitives.Int32WithMeasure<'Measure> -1
            Buckets = buckets
        }

    member b.Reset() =
        b.BucketIdx <- 0
        b.CurBucket <- 0UL
        b.CurItem <- LanguagePrimitives.Int32WithMeasure<'Measure> -1

    member b.Current =
        if b.CurItem < 0<_> then
            raise (InvalidOperationException "Enumeration has not started. Call MoveNext.")
        else
            b.CurItem

    member b.MoveNext() =
        // Check if we have actually started iteration
        if b.CurItem < 0<_> then
            b.CurBucket <- b.Buckets[b.BucketIdx]

        // There are still items in the Current bucket we should return
        if b.CurBucket <> 0UL then
            let r = System.Numerics.BitOperations.TrailingZeroCount b.CurBucket
            b.CurItem <- LanguagePrimitives.Int32WithMeasure<'Measure>((b.BucketIdx <<< 6) + r)
            b.CurBucket <- b.CurBucket ^^^ (1UL <<< r)
            true

        // We need to move to the next bucket of items
        else
            b.BucketIdx <- b.BucketIdx + 1

            if b.BucketIdx < b.Buckets.Length then
                b.CurBucket <- b.Buckets[b.BucketIdx]
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
            new BitSetEnumerator<'Measure>(buckets) :> System.Collections.IEnumerator

    interface IEnumerable<int<'Measure>> with
        member s.GetEnumerator() = new BitSetEnumerator<'Measure>(buckets)
