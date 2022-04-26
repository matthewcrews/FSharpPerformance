open System
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running

[<Measure>] type JobId

type RecordJobId =
    {
        Value : int
    }
    static member create i = { Value = i }

[<Struct>]
type StructRecordJobId =
    {
        Value : int
    }

type DuJobId = JobId of int

[<Struct>]
type StructDuJobId = JobId of int


type Benchmarks () =

    let rng = Random 123

    let valueCount = 100
    let values = [1 .. valueCount]

    // Sets for removing elements from
    let intSet = Set values

    let uomSet =
        values
        |> List.map LanguagePrimitives.Int32WithMeasure<JobId>
        |> Set

    let recordSet =
        values
        |> List.map (fun x -> { RecordJobId.Value = x})
        |> Set

    let structRecordSet =
        values
        |> List.map (fun x -> { StructRecordJobId.Value = x })
        |> Set

    let duSet =
        values
        |> List.map (fun x -> DuJobId.JobId x)
        |> Set

    let structDuSet =
        values
        |> List.map (fun x -> StructDuJobId.JobId x)
        |> Set

    // Test Values to remove
    let removeCount = 10

    let intValues =
        [| for _ in 1 .. 10 -> rng.Next (1, valueCount + 1) |]

    let uomValues =
        intValues
        |> Array.map LanguagePrimitives.Int32WithMeasure<JobId>

    let recordValues =
        intValues
        |> Array.map (fun x -> { RecordJobId.Value = x})

    let structRecordValues =
        intValues
        |> Array.map (fun x -> { StructRecordJobId.Value = x })

    let duValues =
        intValues
        |> Array.map (fun x -> DuJobId.JobId x)

    let structDuValues =
        intValues
        |> Array.map (fun x -> StructDuJobId.JobId x)


    [<Benchmark>]
    member _.Int () =
        let mutable result = Set.empty
        
        for v in intValues do
            result <- intSet.Remove v

        result

    [<Benchmark>]
    member _.UoM () =
        let mutable result = Set.empty
        
        for v in uomValues do
            result <- uomSet.Remove v

        result

    [<Benchmark>]
    member _.Record () =
        let mutable result = Set.empty
        
        for v in recordValues do
            result <- recordSet.Remove v

        result


    [<Benchmark>]
    member _.StructRecord () =
        let mutable result = Set.empty
        
        for v in structRecordValues do
            result <- structRecordSet.Remove v

        result

    [<Benchmark>]
    member _.DU () =
        let mutable result = Set.empty
        
        for v in duValues do
            result <- duSet.Remove v

        result


    [<Benchmark>]
    member _.StructDU () =
        let mutable result = Set.empty
        
        for v in structDuValues do
            result <- structDuSet.Remove v

        result


let _ = BenchmarkRunner.Run<Benchmarks>()
