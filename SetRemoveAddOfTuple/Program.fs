open System
open System.Collections
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running


[<Measure>] type JobId
[<Measure>] type MachineId
[<Measure>] type Assignment = JobId * MachineId

// Approach 1
let defaultTuple = 1<JobId> , 1<MachineId>

// Approach 2
let structTuple = struct (1<JobId>, 1<MachineId>)

// Approach 3
type RefAssignment =
    {
        JobId : int<JobId>
        MachineId : int<MachineId>
    }

// Approach 4
[<Struct>]
type StructAssignment =
    {
        JobId : int<JobId>
        MachineId : int<MachineId>
    }

// Approach 5
module AssignmentInt64 =

    let create (jobId: int<JobId>, machineId: int<MachineId>) =
        ((int64 jobId) <<< 32) ^^^ (int64 machineId)
        |> LanguagePrimitives.Int64WithMeasure<Assignment>


// Approach 6
[<Struct>]
type CompactAssignment (value: int64) =

    new (jobId: int<JobId>, machineId: int<MachineId>) =
        let value = ((int64 jobId) <<< 32) ^^^ (int64 machineId)
        CompactAssignment value

    member _.JobId = int (value >>> 32)

    member _.MachineId = int value


// Approach 7
/// This type assumes that the MachineIds and JobIds start at 0
type BitArrayTracker private (jobCount: int, machineCount: int, values: BitArray) =
    
    new (jobCount: int, machineCount: int) =
        let values = BitArray(jobCount * machineCount)
        BitArrayTracker (jobCount, machineCount, values)
        
        
    member _.Item
        with get (jobId: int<JobId>, machineId: int<MachineId>) =
            let index = (int jobId) * machineCount + (int machineId)
            values[index]
        
            
    member _.Set (jobId: int<JobId>, machineId: int<MachineId>, value) =
        let newValues = BitArray values
        let index = (int jobId) * machineCount + (int machineId)
        newValues.Set (index, value)
        BitArrayTracker (jobCount, machineCount, newValues)
        
        
    member _.SetMultiple (indices: struct (int<JobId> * int<MachineId>) array, value) =
        let newValues = BitArray values
        
        for (machineId, jobId) in indices do
            let index = (int jobId) * machineCount + (int machineId)
            newValues.Set (index, value)

        BitArrayTracker (machineCount, jobCount, newValues)


[<MemoryDiagnoser>]
type Benchmarks () =
    
    let rng = Random 123
    let jobCount = 100
    let machineCount = 10
    let valueCount = 100
    let addValueCount = 2
    let removeValueCount = 2
        
    let tupleValues =
        [for _ in 0 .. valueCount - 1 -> 
            let jobId = LanguagePrimitives.Int32WithMeasure<JobId> (rng.Next jobCount)
            let machineId = LanguagePrimitives.Int32WithMeasure<MachineId> (rng.Next machineCount)
            jobId, machineId
        ] |> Set

    let structTupleValues =
        tupleValues
        |> Set.map (fun (jobId, machineId) -> struct (jobId, machineId))

    let int64Values =
        tupleValues
        |> Set.map AssignmentInt64.create

    let refAssignmentValues =
        tupleValues
        |> Set.map (fun (jobId, machineId) -> { RefAssignment.JobId = jobId; MachineId = machineId })

    let structAssignmentValues =
        tupleValues
        |> Set.map (fun (jobId, machineId) -> { StructAssignment.JobId = jobId; MachineId = machineId })


    let compactAssignmentValues =
        tupleValues
        |> Set.map (fun (jobId, machineId) -> CompactAssignment (jobId, machineId))


    let bitArrayTrackerValues =
        let tupleValuesAsArray =
            tupleValues
            |> Array.ofSeq
            |> Array.map (fun (jobId, machineId) -> struct (jobId, machineId))
        let bitTracker = BitArrayTracker (machineCount, jobCount)
        bitTracker.SetMultiple (tupleValuesAsArray, true)

    
    // Values to Add the to Set/BitSetTracker
    let tupleAddValues =
        [|for _ in 1 .. addValueCount ->
            let jobId =
                rng.Next jobCount
                |> LanguagePrimitives.Int32WithMeasure<JobId>
            let machineId =
                rng.Next machineCount
                |> LanguagePrimitives.Int32WithMeasure<MachineId>
            jobId, machineId
        |]

    let structTupleAddValues =
        tupleAddValues
        |> Array.map (fun (jobId, machineId) -> struct (jobId, machineId))

    let int64AddValues =
        tupleAddValues
        |> Array.map (fun (jobId, machineId) ->
            ((int64 jobId) <<< 32) ^^^ (int64 machineId)
            |> LanguagePrimitives.Int64WithMeasure<Assignment>
        )

    let refAssignmentAddValues =
        tupleAddValues
        |> Array.map (fun (jobId, machineId) -> { RefAssignment.JobId = jobId; MachineId = machineId })

    let structAssignmentAddValues =
        tupleAddValues
        |> Array.map (fun (jobId, machineId) -> { StructAssignment.JobId = jobId; MachineId = machineId })

    let compactAssignmentAddValues =
        tupleAddValues
        |> Array.map (fun (jobId, machineId) -> CompactAssignment (jobId, machineId))

    let bitArrayTrackerAddValues =
        tupleAddValues
        |> Array.map (fun (jobId, machineId) -> struct (jobId, machineId))
    

    // Values to Remove
    let tupleRemoveValues =
        let values = Array.ofSeq tupleValues
        [|for _ in 1..removeValueCount ->
            values[rng.Next values.Length]
        |]

    let structTupleRemoveValues =
        tupleRemoveValues
        |> Array.map (fun (jobId, machineId) -> struct (jobId, machineId))

    let int64RemoveValues =
        tupleRemoveValues
        |> Array.map (fun (jobId, machineId) ->
            ((int64 jobId) <<< 32) ^^^ (int64 machineId)
            |> LanguagePrimitives.Int64WithMeasure<Assignment>
        )

    let refAssignmentRemoveValues =
        tupleRemoveValues
        |> Array.map (fun (jobId, machineId) -> { RefAssignment.JobId = jobId; MachineId = machineId })
        
    let structAssignmentRemoveValues =
        tupleRemoveValues
        |> Array.map (fun (jobId, machineId) -> { StructAssignment.JobId = jobId; MachineId = machineId })

    let compactAssignmentRemoveValues =
        tupleRemoveValues
        |> Array.map (fun (jobId, machineId) -> CompactAssignment (jobId, machineId))

    let bitArrayTrackerRemoveValues =
        tupleRemoveValues
        |> Array.map (fun (jobId, machineId) -> struct (jobId, machineId))

        
    let addElement (set: Set<_>) value = set.Add value
    let removeElement (set: Set<_>) value = set.Remove value
    // Add element tests

    [<Benchmark>]
    member _.TupleAdd () =

        (tupleValues, tupleAddValues)
        ||> Array.fold addElement

    [<Benchmark>]
    member _.StructTupleAdd () =
        (structTupleValues, structTupleAddValues)
        ||> Array.fold addElement

    [<Benchmark>]
    member _.Int64Add () =
        (int64Values, int64AddValues)
        ||> Array.fold addElement

    [<Benchmark>]
    member _.RefAssignmentAdd () =
        (refAssignmentValues, refAssignmentAddValues)
        ||> Array.fold addElement


    [<Benchmark>]
    member _.StructAssignmentAdd () =
        (structAssignmentValues, structAssignmentAddValues)
        ||> Array.fold addElement

        
    [<Benchmark>]
    member _.CompactAssignmentAdd () =
        (compactAssignmentValues, compactAssignmentAddValues)
        ||> Array.fold addElement

    [<Benchmark>]
    member _.BitArrayTrackerAdd () =
        bitArrayTrackerValues.SetMultiple (bitArrayTrackerAddValues, true)
    
    
    // Remove values
    [<Benchmark>]
    member _.TupleRemove () =

        (tupleValues, tupleRemoveValues)
        ||> Array.fold removeElement


    [<Benchmark>]
    member _.StructTupleRemove () =
        (structTupleValues, structTupleRemoveValues)
        ||> Array.fold removeElement

    [<Benchmark>]
    member _.Int64Remove () =
        (int64Values, int64RemoveValues)
        ||> Array.fold removeElement


    [<Benchmark>]
    member _.RefAssignmentRemove () =
        (refAssignmentValues, refAssignmentRemoveValues)
        ||> Array.fold removeElement


    [<Benchmark>]
    member _.StructAssignmentRemove () =
        (structAssignmentValues, structAssignmentRemoveValues)
        ||> Array.fold removeElement

        
    [<Benchmark>]
    member _.CompactAssignmentRemove () =
        (compactAssignmentValues, compactAssignmentRemoveValues)
        ||> Array.fold removeElement


    [<Benchmark>]
    member _.BitArrayTrackerRemove () =
        bitArrayTrackerValues.SetMultiple (bitArrayTrackerRemoveValues, false)



let profile iterations =
    let b = Benchmarks ()

    for _ in 1 .. iterations do
        b.BitArrayTrackerAdd()
        |> ignore



let args = Environment.GetCommandLineArgs().[1..]
printfn "%A" args
match args[0].ToLower() with
| "benchmark" ->

    let _ = BenchmarkRunner.Run<Benchmarks>()
    ()

| "profile" ->
    profile (int args[1])

| _ ->
    invalidArg (nameof args) "Unknown command type"
