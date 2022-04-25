open System
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running

[<Measure>] type Index

type RecordIndex =
    {
        Value : int
    }
    static member create i = { Value = i }

[<Struct>]
type StructRecordIndex =
    {
        Value : int
    }
    static member create i = { Value = i }

type DUIndex =
    | Index of int
    static member create i = Index i

[<Struct>]
type StructDUIndex =
    | Index of int
    static member create i = Index i


type Benchmarks () =
    let rng = Random 123
    let valueCount = 100
    let values =
        [for i in 0 .. 100 -> i, i]

    let keyCount = 100
    
    // Create the various lookups
    let intKeys = [| for _ in 1 .. keyCount do rng.Next valueCount|]
    
    let uomKeys =
        intKeys
        |> Array.map LanguagePrimitives.Int32WithMeasure<Index>

    let recordKeys =
        intKeys
        |> Array.map RecordIndex.create

    let structRecordKeys =
        intKeys
        |> Array.map StructRecordIndex.create

    let duKeys =
        intKeys
        |> Array.map DUIndex.create

    let structDUKeys =
        intKeys
        |> Array.map StructDUIndex.create

    let stringKeys =
        intKeys
        |> Array.map string


    // Create the Maps we will need for looking up
    let intMap =
        values
        |> Map

    let uomMap =
        values
        |> List.map (fun (k, v) -> LanguagePrimitives.Int32WithMeasure<Index> k, v)
        |> Map

    let recordMap =
        values
        |> List.map (fun (k, v) -> RecordIndex.create k, v)
        |> Map

    let structRecordMap =
        values
        |> List.map (fun (k, v) -> StructRecordIndex.create k, v)
        |> Map

    let duMap =
        values
        |> List.map (fun (k, v) -> DUIndex.create k, v)
        |> Map

    let structDUMap =
        values
        |> List.map (fun (k, v) -> StructDUIndex.create k, v)
        |> Map

    let stringMap =
        values
        |> List.map (fun (k, v) -> string k, v)
        |> Map


    [<Benchmark>]
    member _.Int () =
        let mutable acc = 1

        for k in intKeys do
            acc <- intMap[k]

        acc

    [<Benchmark>]
    member _.UoM () =
        let mutable acc = 1

        for k in uomKeys do
            acc <- uomMap[k]

        acc

    [<Benchmark>]
    member _.Record () =
        let mutable acc = 1

        for k in recordKeys do
            acc <- recordMap[k]

        acc

    [<Benchmark>]
    member _.StructRecord () =
        let mutable acc = 1

        for k in structRecordKeys do
            acc <- structRecordMap[k]

        acc

    [<Benchmark>]
    member _.DU () =
        let mutable acc = 1

        for k in duKeys do
            acc <- duMap[k]

        acc

    [<Benchmark>]
    member _.StuctDU () =
        let mutable acc = 1

        for k in structDUKeys do
            acc <- structDUMap[k]

        acc

    [<Benchmark>]
    member _.String () =
        let mutable acc = 1

        for k in stringKeys do
            acc <- stringMap[k]

        acc



[<EntryPoint>]
let main args =

    let _ = BenchmarkRunner.Run<Benchmarks>()
    
    0