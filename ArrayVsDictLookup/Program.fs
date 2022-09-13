open System.Collections.ObjectModel
open System.Collections.Generic
open System.Collections.Immutable
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running


type Benchmarks () =

    let rng = System.Random 123
    let lookupCount = 1_000
    let itemCount = 100

    let arrData = [|0 .. itemCount - 1|]
    let dictionaryData =
        arrData
        |> Array.map (fun x -> KeyValuePair (x, x))
        |> Dictionary

    let readOnlyDictionaryData =
        arrData
        |> Array.map (fun x -> KeyValuePair (x, x))
        |> Dictionary
        |> ReadOnlyDictionary 

    let immutableDictionaryData = 
        arrData
        |> Array.map (fun x -> KeyValuePair (x, x))
        |> ImmutableDictionary.ToImmutableDictionary

    let dictData =
        arrData
        |> Array.map (fun x -> x, x)
        |> dict

    let mapData =
        arrData
        |> Array.map (fun x -> x, x)
        |> Map

    let lookups =
        [|for _ in 1 .. lookupCount -> rng.Next itemCount|]


    [<Benchmark>]
    member _.ArrayLookup () =

        let mutable acc = 0

        for k in lookups do
            acc <- acc + arrData[k]

        acc

    [<Benchmark>]
    member _.DictionaryLookup () =
        
        let mutable acc = 0
        
        for k in lookups do
            acc <- acc + dictionaryData[k]

        acc

    [<Benchmark>]
    member _.ReadOnlyDictionaryLookup () =
        
        let mutable acc = 0
        
        for k in lookups do
            acc <- acc + readOnlyDictionaryData[k]

        acc

    [<Benchmark>]
    member _.ImmutableDictionaryLookup () =
        
        let mutable acc = 0
        
        for k in lookups do
            acc <- acc + immutableDictionaryData[k]

        acc

    [<Benchmark>]
    member _.DictLookup () =
        
        let mutable acc = 0
        
        for k in lookups do
            acc <- acc + dictData[k]

        acc

    [<Benchmark>]
    member _.MapLookup () =
        
        let mutable acc = 0
        
        for k in lookups do
            acc <- acc + mapData[k]

        acc


[<EntryPoint>]
let main (args: string[]) =

    let _ = BenchmarkRunner.Run<Benchmarks>()

    1
