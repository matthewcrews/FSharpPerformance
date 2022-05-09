open System
open System.Collections
open System.Collections.Generic
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running

open System.Runtime.CompilerServices

[<Measure>] type JobId
[<Measure>] type MachineId
[<Measure>] type OperationId
[<Measure>] type Assignment = JobId * MachineId * OperationId

module Assignment =

    let create (jobId: int<JobId>) (machineId: int<MachineId>) (operationId: int<OperationId>) =
        ((int64 jobId) <<< 32) ||| ((int64 machineId) <<< 16) ||| (int64 operationId)
        |> LanguagePrimitives.Int64WithMeasure<Assignment>

    let decompose (assignment: int64<Assignment>) =
        let jobId = int (int64 assignment >>> 32) |> LanguagePrimitives.Int32WithMeasure<JobId>
        let machineId = (int assignment) >>> 16 |> LanguagePrimitives.Int32WithMeasure<MachineId>
        let operationId = (int assignment) &&& 0x0000FFFF |> LanguagePrimitives.Int32WithMeasure<OperationId>
        struct (jobId, machineId, operationId)


type BitSetTracker (jobCount, machineCount, operationCount: int) =
    let uint64sRequired = ((jobCount * machineCount * operationCount) + 63) >>> 6
    let buckets : uint64 array = Array.zeroCreate uint64sRequired

    member internal _.JobCount = jobCount
    member internal _.MachineCount = machineCount
    member internal _.OperationCount = operationCount
    member internal _.Values = buckets

    member _.Item

        with [<MethodImpl(MethodImplOptions.AggressiveInlining)>] get (jobId: int<JobId>, machineId: int<MachineId>, operationId: int<OperationId>) =
            if (int jobId) >= jobCount then
                raise (IndexOutOfRangeException (nameof jobId))
            if (int machineId) >= machineCount then
                raise (IndexOutOfRangeException (nameof machineId))
            if (int operationId) >= operationCount then
                raise (IndexOutOfRangeException (nameof operationId))

            // The location of the bit we are interested in
            let location = int jobId
            let location = location*machineCount    + int machineId
            let location = location*operationCount  + int operationId
            // The int64 we will need to lookup
            let bucket = location >>> 6
            // The bit in the int64 we want to return
            let offset = location &&& 0x3F
            // Mask to check with
            let mask = 1UL <<< offset
            // Return whether the bit at the offset is set to 1 or not
            buckets[bucket] &&& mask <> 0UL

        and [<MethodImpl(MethodImplOptions.AggressiveInlining)>] set (jobId: int<JobId>, machineId: int<MachineId>, operationId: int<OperationId>) value =
            if (int jobId) >= jobCount then
                raise (IndexOutOfRangeException (nameof jobId))
            if (int machineId) >= machineCount then
                raise (IndexOutOfRangeException (nameof machineId))
            if (int operationId) >= operationCount then
                raise (IndexOutOfRangeException (nameof operationId))

            let location = int jobId
            let location = location*machineCount    + int machineId
            let location = location*operationCount  + int operationId
            // The int64 we will need to lookup
            let bucket = location >>> 6
            // The bit in the int64 we want to return
            let offset = location &&& 0x3F
            // Get the int representation of the value
            let value = if value then 1UL else 0UL
            // Set the bit in the bucket to the desired value
            buckets[bucket] <- (buckets[bucket] &&& ~~~(1UL <<< offset)) ||| (value <<< offset)


    member _.SetMany (indices: struct (int<JobId> * int<MachineId> * int<OperationId>) array) value =

        for jobId, machineId, operationId in indices do
            if (int jobId) >= jobCount then
                raise (IndexOutOfRangeException (nameof jobId))
            if (int machineId) >= machineCount then
                raise (IndexOutOfRangeException (nameof machineId))
            if (int operationId) >= operationCount then
                raise (IndexOutOfRangeException (nameof operationId))

            // The location of the bit we are interested in
            let location = int jobId
            let location = location*machineCount    + int machineId
            let location = location*operationCount  + int operationId
            // The int64 we will need to lookup
            let bucket = location >>> 6
            // The bit in the int64 we want to return
            let offset = location &&& 0x3F
            // Get the int representation of the value
            let value = if value then 1UL else 0UL
            // Set the bit in the bucket to the desired value
            buckets[bucket] <- (buckets[bucket] &&& ~~~(1UL <<< offset)) ||| (value <<< offset)


module BitSetTracker =

    let map (f: int<JobId> -> int<MachineId> -> int<OperationId> -> 'Result) (b: BitSetTracker) =
        let acc = Stack<'Result> ()
        let mutable i = 0
        let length = b.Values.Length

        // Source of algorithm: https://lemire.me/blog/2018/02/21/iterating-over-set-bits-quickly/
        while i < length do
            let mutable bitSet = b.Values[i]
            while bitSet <> 0UL do
                let r = System.Numerics.BitOperations.TrailingZeroCount bitSet
                let location = i * 64 + r
                let jobId =
                    location / (b.MachineCount * b.OperationCount)
                    |> LanguagePrimitives.Int32WithMeasure<JobId>
                let machineId =
                    (location - (int jobId) * (b.MachineCount * b.OperationCount)) / b.OperationCount
                    |> LanguagePrimitives.Int32WithMeasure<MachineId>
                let operationId =
                    location - (int jobId) * (b.MachineCount * b.OperationCount) - (int machineId) * b.OperationCount
                    |> LanguagePrimitives.Int32WithMeasure<OperationId>

                let result = f jobId machineId operationId
                acc.Push result

                bitSet <- bitSet ^^^ (1UL <<< r)

            i <- i + 1

        acc.ToArray()

module Details =
    // mrange: I was no overhead of putting the computation in
    //  an inline function. Tuples return values are optimized away
    //  release builds at least.
    let inline computeBucketAndMask (jobId: int<JobId>) jobCount (machineId: int<MachineId>) machineCount (operationId: int<OperationId>) operationCount =
        // mrange: A crude optimization here is to not these checks and let
        //  it crash if the index is out of bounds
        //  One check that is missing here if the ids are negative.
        //  That can be avoided with uints
        if (int machineId) >= machineCount then
            raise (IndexOutOfRangeException (nameof machineId))
        if (int jobId) >= jobCount then
            raise (IndexOutOfRangeException (nameof jobId))
        if (int operationId) >= operationCount then
            raise (IndexOutOfRangeException (nameof operationId))

        // mrange: I did experiment with a bunch of approaches here
        //  pre computing a machineCount*operationCount
        //  or direct multiplication like the original code
        //  The code on my hardware seems to do best
        //  A multiplication is quick but have some latency so it's possible
        //  running 3 independent multiplications is faster than 2 dependent
        //  ones.
        let location  = int jobId
        let location  = location*machineCount    + int machineId
        let location  = location*operationCount  + int operationId
        let bucket    = location >>> 6
        let offset    = location &&& 0x3F
        let mask      = 1UL <<< offset

        bucket, mask
open Details

// mrange: Same as BitSetTracker but changed to supporting inlining of get/set
//  inlining means the method prelude and epilogue are removed which helps performance
//  The jitter is not smart enough from my testing to avoid reloading the same
//  state from the object from what I could see
//  I think it also could be interesting to add a C# version to see how that compares
type InliningBitSetTracker (jobCount, machineCount, operationCount: int) =
    let uint64sRequired         = ((jobCount * machineCount * operationCount) + 63) >>> 6
    let buckets : uint64 array  = Array.zeroCreate uint64sRequired

    // mrange: These needs to be public to support inlining
    member _.JobCount           = jobCount
    member _.MachineCount       = machineCount
    member _.OperationCount     = operationCount
    member _.Buckets            = buckets

    member inline x.Get (jobId: int<JobId>) (machineId: int<MachineId>) (operationId: int<OperationId>) =
        let bucketId, mask  = computeBucketAndMask jobId x.JobCount machineId x.MachineCount operationId x.OperationCount
        let buckets         = x.Buckets
        let bucket          = buckets[bucketId]

        bucket &&& mask <> 0UL

    member inline x.Set (jobId: int<JobId>) (machineId: int<MachineId>) (operationId: int<OperationId>) value =
        let bucketId, mask  = computeBucketAndMask jobId x.JobCount machineId x.MachineCount operationId x.OperationCount
        let buckets         = x.Buckets
        let bucket          = buckets[bucketId]
        let current         = bucket &&& mask <> 0UL

        // mrange: While seeming a bit stupid this in my testing seems to do
        //  better than mask and update code
        //  One reason I suspect is that CPUs AFAIK can execute both branches at
        //  the same time and just keep the winner. Maybe that helps here.
        if current <> value then
          if value then
              buckets[bucketId] <- bucket ||| mask
          else
              buckets[bucketId] <- bucket &&& ~~~mask

    member x.SetMany (indices: struct (int<JobId> * int<MachineId> * int<OperationId>) array) value =
        // mrange: Preloads these, hopefully the jitter can keep them around if there
        //  enough registers
        let jobCount        = jobCount
        let machineCount    = machineCount
        let operationCount  = operationCount

        for jobId, machineId, operationId in indices do
            let bucketId, mask  = computeBucketAndMask jobId jobCount machineId machineCount operationId operationCount
            let buckets         = x.Buckets
            let bucket          = buckets[bucketId]
            let current         = bucket &&& mask <> 0UL

            if current <> value then
              if value then
                  buckets[bucketId] <- bucket ||| mask
              else
                  buckets[bucketId] <- bucket &&& ~~~mask


[<MemoryDiagnoser>]
type Benchmarks () =

    let rng               = Random 123

    let jobIdBound        = 1_000
    let machineIdBound    = 10
    let operationIdBound  = 100
    let valueCount        = 1_000

    let values =
        [|for _ in 1..valueCount ->
            let jobId = rng.Next jobIdBound |> LanguagePrimitives.Int32WithMeasure<JobId>
            let machineId = rng.Next machineIdBound |> LanguagePrimitives.Int32WithMeasure<MachineId>
            let operationId = rng.Next operationIdBound |> LanguagePrimitives.Int32WithMeasure<OperationId>
            struct (jobId, machineId, operationId)
        |]

    let removeCount = 20
    let removeValues =
        [|for _ in 1..removeCount ->
            values[rng.Next values.Length]
        |]

    // mrange: I did some perf runs of my own and if I start filling up the
    //  the bitsets the HashSet based solution can't keep up due to
    //  it being much less efficient with memory
    //  However, for smaller numbers like this HashSet does pretty good.
    let addCount = 20
    let addValues =
        [|for _ in 1..addCount ->
            let jobId = rng.Next jobIdBound |> LanguagePrimitives.Int32WithMeasure<JobId>
            let machineId = rng.Next machineIdBound |> LanguagePrimitives.Int32WithMeasure<MachineId>
            let operationId = rng.Next operationIdBound |> LanguagePrimitives.Int32WithMeasure<OperationId>
            struct (jobId, machineId, operationId)
        |]

    let hashSet =
        values
        |> Array.map (fun struct (jobId, machineId, operationId) -> Assignment.create jobId machineId operationId)
        |> HashSet

    let bitSet =
        let b = BitSetTracker (jobIdBound, machineIdBound, operationIdBound)
        for jobId, machineId, operationId in values do
            b.[jobId, machineId, operationId] <- true
        b


    let inliningBitSet =
        let b = InliningBitSetTracker (jobIdBound, machineIdBound, operationIdBound)
        for jobId, machineId, operationId in values do
            b.Set jobId machineId operationId true
        b
// mrange: Added these to perform short runs
//  IMHO while BenchmarkDotnet is great the perfs runs are also quite slow.
//  So it's hard to do iterative processes for me. So I like therefore to have
//  short runs to improve developer inner loop. When I am happy I enable the long runs
//  again
#if SHORTRUN
    [<Benchmark>]
    member _.HashSetAdd () =

        for jobId, machineId, operationId in addValues do
            let assignment = Assignment.create jobId machineId operationId
            hashSet.Add assignment |> ignore


    [<Benchmark>]
    member _.BitSetAdd () =

        for jobId, machineId, operationId in addValues do
            bitSet[jobId, machineId, operationId] <- true


    [<Benchmark>]
    member _.InliningBitSetAdd () =

        for jobId, machineId, operationId in addValues do
            inliningBitSet.Set jobId machineId operationId true
#else
    [<Benchmark>]
    member _.HashSetAdd () =

        for jobId, machineId, operationId in addValues do
            let assignment = Assignment.create jobId machineId operationId
            hashSet.Add assignment |> ignore


    [<Benchmark>]
    member _.HashSetRemove () =

        for jobId, machineId, operationId in removeValues do
            let assignment = Assignment.create jobId machineId operationId
            hashSet.Remove assignment |> ignore


    [<Benchmark>]
    member _.HashSetMap () =

        hashSet
        |> Seq.map Assignment.decompose
        |> Seq.toArray


    [<Benchmark>]
    member _.BitSetAdd () =

        for jobId, machineId, operationId in addValues do
            bitSet[jobId, machineId, operationId] <- true


    [<Benchmark>]
    member _.BitSetAddMany () =

        bitSet.SetMany addValues true


    [<Benchmark>]
    member _.BitSetRemove () =

        for jobId, machineId, operationId in removeValues do
            bitSet[jobId, machineId, operationId] <- false


    [<Benchmark>]
    member _.BitSetRemoveMany () =
        bitSet.SetMany removeValues false


    [<Benchmark>]
    member _.BitSetMap () =

        bitSet
        |> BitSetTracker.map (fun a b c -> struct (a, b, c))


    [<Benchmark>]
    member _.InliningBitSetAdd () =

        for jobId, machineId, operationId in addValues do
            inliningBitSet.Set jobId machineId operationId true


    [<Benchmark>]
    member _.InliningBitSetAddMany () =

        inliningBitSet.SetMany addValues true


    [<Benchmark>]
    member _.InliningBitSetRemove () =

        for jobId, machineId, operationId in removeValues do
            inliningBitSet.Set jobId machineId operationId false


    [<Benchmark>]
    member _.InliningBitSetRemoveMany () =
        inliningBitSet.SetMany removeValues false

#if TODO
    // mrange: Not implemented yet
    [<Benchmark>]
    member _.InliningBitSetMap () =

        inliningBitSet
        |> BitSetTracker.map (fun a b c -> struct (a, b, c))
#endif
#endif

let _ = BenchmarkRunner.Run<Benchmarks>()
