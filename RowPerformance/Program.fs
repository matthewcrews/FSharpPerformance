open System
open System.Collections.Generic
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running

type Row<[<Measure>] 'Measure, 'T> (values: 'T array) =

    member _.Item
        with get (k: int<'Measure>) =
            values[int k]
        
        and set (k: int<'Measure>) (v: 'T) =
            values[int k] <- v

[<Measure>] type JobId


type Benchmarks () =

    let rng = Random 123
    let valueCount = 100

    let rawArray =
        [|for _ in 1 .. valueCount -> rng.NextDouble() |]

    let row =
        Row<JobId, _> rawArray
    
    let map = 
        rawArray
        |> Array.mapi (fun i v -> i, v)
        |> Map

    let dictionary =
        rawArray
        |> Array.mapi (fun i v -> KeyValuePair (i, v))
        |> Dictionary

    // Keys
    let keyCount = 1_000_000


    let arrayKeys =
        [|for _ in 1 .. keyCount -> rng.Next valueCount |]

    let rowKeys =
        arrayKeys
        |> Array.map LanguagePrimitives.Int32WithMeasure<JobId>

    let mapKeys =
        arrayKeys

    let dictionaryKeys =
        arrayKeys

    // Benchmarks

    [<Benchmark(Baseline = true)>]
    member _.Array () =
        let mutable acc = 0.0

        for k in arrayKeys do
            acc <- acc + rawArray[k]

        acc

    [<Benchmark>]
    member _.Row () =
        let mutable acc = 0.0

        for k in rowKeys do
            acc <- acc + row[k]

        acc

    [<Benchmark>]
    member _.Map () =
        let mutable acc = 0.0

        for k in mapKeys do
            acc <- acc + map[k]

        acc

    [<Benchmark>]
    member _.Dictionary () =
        let mutable acc = 0.0

        for k in dictionaryKeys do
            acc <- acc + dictionary[k]

        acc


let _ = BenchmarkRunner.Run<Benchmarks>()
