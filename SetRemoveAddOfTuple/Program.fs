open System
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running

[<Measure>] type JobId
[<Measure>] type MachineId
[<Measure>] type Assignment

type RefAssignment =
    {
        JobId : int<JobId>
        MachineId : int<MachineId>
    }

// [<Struct; CustomEquality; CustomComparison>]
[<Struct>]
type StructAssignment =
    {
        JobId : int<JobId>
        MachineId : int<MachineId>
    }


[<Struct>]
type CompactAssignment (value: int64) =

    new (jobId: int<JobId>, machineId: int<MachineId>) =
        let value = ((int64 jobId) <<< 32) ^^^ (int64 machineId)
        CompactAssignment value

    member _.JobId = int (value >>> 32)

    member _.MachineId = int value



type Benchmarks () =
    
    let rng = Random 123
    let maxJobId = 100
    let maxMachineId = 100
    let valueCount = 100

    let tupleValues =
        [for _ in 0 .. valueCount - 1 -> 
            let jobId = LanguagePrimitives.Int32WithMeasure<JobId> (rng.Next maxJobId)
            let machineId = LanguagePrimitives.Int32WithMeasure<MachineId> (rng.Next maxMachineId)
            jobId, machineId
        ] |> Set

    let structTupleValues =
        tupleValues
        |> Set.map (fun (jobId, machineId) -> struct (jobId, machineId))

    let assignmentValues =
        tupleValues
        |> Set.map (fun (jobId, machineId) ->
            ((int64 jobId) <<< 32) ^^^ (int64 machineId)
            |> LanguagePrimitives.Int64WithMeasure<Assignment>
        )

    let refAssignmentValues =
        tupleValues
        |> Set.map (fun (jobId, machineId) -> { RefAssignment.JobId = jobId; MachineId = machineId })

    let structAssignmentValues =
        tupleValues
        |> Set.map (fun (jobId, machineId) -> { StructAssignment.JobId = jobId; MachineId = machineId })


    let compactAssignmentValues =
        tupleValues
        |> Set.map (fun (jobId, machineId) -> CompactAssignment (jobId, machineId))


    let addValueCount = 4

    let tupleAddValues =
        [|for _ in 1 .. addValueCount ->
            let jobId =
                rng.Next maxJobId
                |> LanguagePrimitives.Int32WithMeasure<JobId>
            let machineId =
                rng.Next maxMachineId
                |> LanguagePrimitives.Int32WithMeasure<MachineId>
            jobId, machineId
        |]

    let structTupleAddValues =
        tupleAddValues
        |> Array.map (fun (jobId, machineId) -> struct (jobId, machineId))

    let assignmentAddValues =
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


    let removeValueCount = 4

    let tupleRemoveValues =
        let values = Array.ofSeq tupleValues
        [|for _ in 1..removeValueCount ->
            values[rng.Next values.Length]
        |]

    let structTupleRemoveValues =
        tupleRemoveValues
        |> Array.map (fun (jobId, machineId) -> struct (jobId, machineId))

    let assignmentRemoveValues =
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
    member _.AssignmentAdd () =
        (assignmentValues, assignmentAddValues)
        ||> Array.fold addElement

    [<Benchmark>]
    member _.RefAssignmenteAdd () =
        (refAssignmentValues, refAssignmentAddValues)
        ||> Array.fold addElement


    [<Benchmark>]
    member _.StructAssignmenteAdd () =
        (structAssignmentValues, structAssignmentAddValues)
        ||> Array.fold addElement

        
    [<Benchmark>]
    member _.CompactAssignmentAdd () =
        (compactAssignmentValues, compactAssignmentAddValues)
        ||> Array.fold addElement


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
    member _.AssignmentRemove () =
        (assignmentValues, assignmentRemoveValues)
        ||> Array.fold removeElement


    [<Benchmark>]
    member _.RefAssignmenteRemove () =
        (refAssignmentValues, refAssignmentRemoveValues)
        ||> Array.fold removeElement


    [<Benchmark>]
    member _.StructAssignmenteRemove () =
        (structAssignmentValues, structAssignmentRemoveValues)
        ||> Array.fold removeElement

        
    [<Benchmark>]
    member _.CompactAssignmentRemove () =
        (compactAssignmentValues, compactAssignmentRemoveValues)
        ||> Array.fold removeElement


let profile iterations =
    let b = Benchmarks ()

    for _ in 1 .. iterations do
        b.RefAssignmenteAdd()
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