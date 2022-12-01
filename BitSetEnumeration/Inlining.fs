module rec BitSetEnumeration.Inlining

open System
open Microsoft.FSharp.Core

module private Helpers =

    let inline computeBucketAndMask (itemKey: int<'Item>) =
        let location = int itemKey
        let bucket = location >>> 6
        let offset = location &&& 0x3F
        let mask = 1UL <<< offset
        bucket, mask


[<Struct>]
type BitSetEnumerator<[<Measure>] 'Measure> =
    val mutable BucketIdx: int
    val mutable CurBucket: uint64
    val mutable CurItem: int<'Measure>
    val Buckets: uint64[]

    new(buckets: uint64[]) =
        {
            BucketIdx = 0
            CurBucket = 0UL
            CurItem = LanguagePrimitives.Int32WithMeasure<'Measure> -1
            Buckets = buckets
        }

    member inline b.Current =
        if b.CurItem < 0<_> then
            raise (InvalidOperationException "Enumeration has not started. Call MoveNext.")
        else
            b.CurItem

    member inline b.MoveNext() =
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
            let mutable result = false
            
            while b.BucketIdx < b.Buckets.Length && (not result) do
                b.CurBucket <- b.Buckets[b.BucketIdx]
                
                // There are still items in the Current bucket we should return
                if b.CurBucket <> 0UL then
                    let r = System.Numerics.BitOperations.TrailingZeroCount b.CurBucket
                    b.CurItem <- LanguagePrimitives.Int32WithMeasure<'Measure>((b.BucketIdx <<< 6) + r)
                    b.CurBucket <- b.CurBucket ^^^ (1UL <<< r)
                    result <- true
                
                if not result then
                    b.BucketIdx <- b.BucketIdx + 1
            
            result


[<Struct>]
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

    member b.GetEnumerator() = BitSetEnumerator<'Measure>(buckets)
