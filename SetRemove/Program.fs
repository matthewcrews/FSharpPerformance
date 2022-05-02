open System
open System.Collections.Immutable
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

[<Struct>]
[<CustomEquality; CustomComparison>]
type CustomStructRecordJobId =
    {
        Value : int
    }
    override this.Equals other =
        match other with
        | :? CustomStructRecordJobId as that -> this.Value = that.Value
        | _ -> false
        
    override this.GetHashCode () = this.Value.GetHashCode()

    interface IEquatable<CustomStructRecordJobId> with
        member this.Equals other = other.Value = this.Value
        
    interface IComparable<CustomStructRecordJobId> with
        member this.CompareTo other = other.Value.CompareTo this.Value
    
    interface IComparable with
        member this.CompareTo other =
            match other with
            | :? CustomStructRecordJobId as that -> (this :> IComparable<_>).CompareTo that
            | _ -> -1


type DuJobId = JobId of int

[<Struct>]
type StructDuJobId = JobId of int


[<MemoryDiagnoser>]
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

    let customStructRecordSet =
        values
        |> List.map (fun x -> { CustomStructRecordJobId.Value = x })
        |> Set
    
    let duSet =
        values
        |> List.map (fun x -> DuJobId.JobId x)
        |> Set

    let structDuSet =
        values
        |> List.map (fun x -> StructDuJobId.JobId x)
        |> Set


    let immutableHashSet =
        values.ToImmutableSortedSet()

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

    let customStructRecordValues =
        intValues
        |> Array.map (fun x -> { CustomStructRecordJobId.Value = x })
    
    let duValues =
        intValues
        |> Array.map (fun x -> DuJobId.JobId x)

    let structDuValues =
        intValues
        |> Array.map (fun x -> StructDuJobId.JobId x)

    let immutableHashSetValues =
        intValues

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
    member _.CustomStructRecord () =
        let mutable result = Set.empty
        
        for v in customStructRecordValues do
            result <- customStructRecordSet.Remove v

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

    [<Benchmark>]
    member _.ImmutableHashSet () =
        let mutable result = ImmutableSortedSet.Create()
        
        for v in intValues do
            result <- immutableHashSet.Remove v

        result

let profile iterations =
    let mutable result = Set.empty
    let b = Benchmarks ()
    
    for _ in 1 .. iterations do
        result <- b.CustomStructRecord()
        
    result


let args = Environment.GetCommandLineArgs()

match args[1].ToLower() with
| "benchmark" ->
    let _ = BenchmarkRunner.Run<Benchmarks>()
    ()
| "profile" ->
    let iterations = int args[2]
    let _ = profile iterations
    ()
| _ ->
    invalidArg (nameof args) $"Unknown command: {args[1]}"
