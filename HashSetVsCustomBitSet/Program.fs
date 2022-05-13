open System
open System.Collections.Generic
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running


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
    let uint64sRequired = ((jobCount * machineCount * operationCount) + 63) / 64
    let buckets : uint64 array = Array.zeroCreate uint64sRequired

    member internal _.JobCount = jobCount
    member internal _.MachineCount = machineCount
    member internal _.OperationCount = operationCount
    member internal _.Values = buckets

    member _.Add (jobId: int<JobId>, machineId: int<MachineId>, operationId: int<OperationId>) =
        
        if (int jobId) >= jobCount || (int jobId) < 0 then
            raise (IndexOutOfRangeException (nameof jobId))
        
        if (int machineId) >= machineCount || (int machineId) < 0 then
            raise (IndexOutOfRangeException (nameof machineId))
        
        if (int operationId) >= operationCount || (int operationId) < 0 then
            raise (IndexOutOfRangeException (nameof operationId))

        let location = int jobId
        let location = location * machineCount    + (int machineId)
        let location = location * operationCount  + (int operationId)
        // The int64 we will need to lookup
        let bucket = location / 64
        // The bit in the int64 we want to return
        let offset = location - bucket * 64
        // Set the bit in the bucket to 1
        buckets[bucket] <- buckets[bucket] ||| (1UL <<< offset)


    member _.Remove (jobId: int<JobId>, machineId: int<MachineId>, operationId: int<OperationId>) =
        
        if (int jobId) >= jobCount || (int jobId) < 0 then
            raise (IndexOutOfRangeException (nameof jobId))
        
        if (int machineId) >= machineCount || (int machineId) < 0 then
            raise (IndexOutOfRangeException (nameof machineId))
        
        if (int operationId) >= operationCount || (int operationId) < 0 then
            raise (IndexOutOfRangeException (nameof operationId))

        let location = int jobId
        let location = location*machineCount    + int machineId
        let location = location*operationCount  + int operationId
        // The int64 we will need to lookup
        let bucket = location / 64
        // The bit in the int64 we want to return
        let offset = location - bucket * 64
        // Set the bit in the bucket to 0
        buckets[bucket] <- buckets[bucket] &&& ~~~(1UL <<< offset)


    member _.Map (f: int<JobId> -> int<MachineId> -> int<OperationId> -> 'Result) =
        let acc = Stack<'Result> ()
        let mutable i = 0
        let length = buckets.Length

        // Source of algorithm: https://lemire.me/blog/2018/02/21/iterating-over-set-bits-quickly/
        while i < length do
            let mutable bitSet = buckets[i]

            while bitSet <> 0UL do
                let r = System.Numerics.BitOperations.TrailingZeroCount bitSet
                let location = i * 64 + r
                let jobId =
                    location / (machineCount * operationCount)
                    |> LanguagePrimitives.Int32WithMeasure<JobId>
                let machineId =
                    (location - (int jobId) * (machineCount * operationCount)) / operationCount
                    |> LanguagePrimitives.Int32WithMeasure<MachineId>
                let operationId =
                    location - (int jobId) * (machineCount * operationCount) - (int machineId) * operationCount
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

        if (uint jobId) >= (uint jobCount) then
            raise (IndexOutOfRangeException (nameof jobId))

        if (uint machineId) >= (uint machineCount) then
            raise (IndexOutOfRangeException (nameof machineId))

        if (uint operationId) >= (uint operationCount) then
            raise (IndexOutOfRangeException (nameof operationId))

        // mrange: I did experiment with a bunch of approaches here
        //  pre computing a machineCount*operationCount
        //  or direct multiplication like the original code
        //  The code on my hardware seems to do best
        //  A multiplication is quick but have some latency so it's possible
        //  running 3 independent multiplications is faster than 2 dependent
        //  ones.
        let location  = int jobId
        let location  = location * machineCount   + (int machineId)
        let location  = location * operationCount + (int operationId)
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


    member inline x.Add (jobId: int<JobId>, machineId: int<MachineId>, operationId: int<OperationId>) =
        let bucketId, mask  = computeBucketAndMask jobId x.JobCount machineId x.MachineCount operationId x.OperationCount
        let buckets         = x.Buckets
        let bucket          = buckets[bucketId]
        buckets[bucketId] <- bucket ||| mask


    member inline x.Remove (jobId: int<JobId>, machineId: int<MachineId>, operationId: int<OperationId>) =
        let bucketId, mask  = computeBucketAndMask jobId x.JobCount machineId x.MachineCount operationId x.OperationCount
        let buckets         = x.Buckets
        let bucket          = buckets[bucketId]
        buckets[bucketId] <- bucket &&& ~~~mask

    member inline x.Map ([<InlineIfLambda>] f: int<JobId> -> int<MachineId> -> int<OperationId> -> 'Result) =
        let acc = Stack<'Result> (x.JobCount)
        let mutable i = 0
        let machineMulOp = x.MachineCount * x.OperationCount
        // Source of algorithm: https://lemire.me/blog/2018/02/21/iterating-over-set-bits-quickly/
        while i < x.Buckets.Length do
            let mutable bitSet = x.Buckets[i]
            
            while bitSet <> 0UL do
                let r = System.Numerics.BitOperations.TrailingZeroCount bitSet
                let location = i <<< 6 + r
                let jobId =
                    location / machineMulOp
                    |> LanguagePrimitives.Int32WithMeasure<JobId>
                let jobIdMulMachineMulOp = (int jobId) * machineMulOp
                let machineId =
                    (location - jobIdMulMachineMulOp) / x.OperationCount
                    |> LanguagePrimitives.Int32WithMeasure<MachineId>
                let operationId =
                    location - jobIdMulMachineMulOp - (int machineId) * x.OperationCount
                    |> LanguagePrimitives.Int32WithMeasure<OperationId>

                acc.Push (f jobId machineId operationId)

                bitSet <- bitSet ^^^ (1UL <<< r)

            i <- i + 1
        acc.ToArray()


[<MemoryDiagnoser>]
type Benchmarks () =

    let rng            = Random 123
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
            b.Add (jobId, machineId, operationId)
        b

    let inliningBitSet =
        let b = InliningBitSetTracker (jobIdBound, machineIdBound, operationIdBound)
        for jobId, machineId, operationId in values do
            b.Add (jobId, machineId, operationId)
        b


//    [<Benchmark>]
//    member _.HashSetAdd () =
//
//        for jobId, machineId, operationId in addValues do
//            let assignment = Assignment.create jobId machineId operationId
//            hashSet.Add assignment |> ignore
//
//
//    [<Benchmark>]
//    member _.HashSetRemove () =
//
//        for jobId, machineId, operationId in removeValues do
//            let assignment = Assignment.create jobId machineId operationId
//            hashSet.Remove assignment |> ignore


//    [<Benchmark>]
//    member _.HashSetMap () =
//
//        hashSet
//        |> Seq.map Assignment.decompose
//        |> Seq.toArray

//
//    [<Benchmark>]
//    member _.BitSetAdd () =
//
//        for jobId, machineId, operationId in addValues do
//            bitSet.Add (jobId, machineId, operationId)
//
//
//    [<Benchmark>]
//    member _.BitSetRemove () =
//
//        for jobId, machineId, operationId in removeValues do
//            bitSet.Remove (jobId, machineId, operationId)


    [<Benchmark>]
    member _.BitSetMap () =

        bitSet.Map (fun a b c -> struct (a, b, c))


//    [<Benchmark>]
//    member _.InliningBitSetAdd () =
//
//        for jobId, machineId, operationId in addValues do
//            inliningBitSet.Add (jobId, machineId, operationId)
//
//
//    [<Benchmark>]
//    member _.InliningBitSetRemove () =
//
//        for jobId, machineId, operationId in removeValues do
//            inliningBitSet.Remove (jobId, machineId, operationId)

    [<Benchmark>]
    member _.InliningBitSetMap () =

        inliningBitSet.Map (fun a b c -> struct (a, b, c))

let _ = BenchmarkRunner.Run<Benchmarks>()
