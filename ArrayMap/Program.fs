open System
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running

type Bar<[<Measure>] 'Measure, 'T> (values: 'T array) =

    member _.Item
        with get (k: int<'Measure>) =
            values[int k]


    member _.Add (k: int<'Measure>, v: 'T) =
        let newValues = Array.copy values
        newValues[int k] <- v
        Bar newValues

[<Measure>] type JobId

type Size =
    | ``10`` = 0
    | ``100`` = 1
    | ``1_000`` = 2
    | ``10_000`` = 3
    | ``100_000`` = 4
    | ``1_000_000`` = 5


type Benchmarks () =

    let rng = Random 123

    let values =
        [|
            [|for i in 0 .. 10 - 1 ->
                LanguagePrimitives.Int32WithMeasure<JobId> i, i|]
            
            [|for i in 0 .. 100 - 1 ->
                LanguagePrimitives.Int32WithMeasure<JobId> i, i|]
            
            [|for i in 0 .. 1_000 - 1 ->
                LanguagePrimitives.Int32WithMeasure<JobId> i, i|]
            
            [|for i in 0 .. 10_000 - 1 ->
                LanguagePrimitives.Int32WithMeasure<JobId> i, i|]

            [|for i in 0 .. 100_000 - 1 ->
                LanguagePrimitives.Int32WithMeasure<JobId> i, i|]

            [|for i in 0 .. 1_000_000 - 1 ->
                LanguagePrimitives.Int32WithMeasure<JobId> i, i|]
        |]

    let keyCount = 100

    let keys =
        [|
            [|for _ in 1 .. keyCount ->
                LanguagePrimitives.Int32WithMeasure<JobId> (rng.Next values[int Size.``10``].Length) |]
            
            [|for _ in 1 .. keyCount ->
                LanguagePrimitives.Int32WithMeasure<JobId> (rng.Next values[int Size.``100``].Length) |]
            
            [|for _ in 1 .. keyCount ->
                LanguagePrimitives.Int32WithMeasure<JobId> (rng.Next values[int Size.``1_000``].Length) |]
            
            [|for _ in 1 .. keyCount ->
                LanguagePrimitives.Int32WithMeasure<JobId> (rng.Next values[int Size.``10_000``].Length) |]
            
            [|for _ in 1 .. keyCount ->
                LanguagePrimitives.Int32WithMeasure<JobId> (rng.Next values[int Size.``100_000``].Length) |]
            
            [|for _ in 1 .. keyCount ->
                LanguagePrimitives.Int32WithMeasure<JobId> (rng.Next values[int Size.``1_000_000``].Length) |]
        |]

    let maps =
        values
        |> Array.map Map

    let bars =
        values
        |> Array.map (Array.map snd >> Bar<JobId, _>)

    
    [<Params(Size.``10``, Size.``100``, Size.``1_000``, Size.``10_000``, Size.``100_000``, Size.``1_000_000``)>]
    member val Size = Size.``10`` with get, set


    [<Benchmark>]
    member b.Map () =
        let sizeIndex = int b.Size
        let m = maps[sizeIndex]
        let keys = keys[sizeIndex]
        let mutable result = Map.empty
        
        for k in keys do
            result <- m.Add (k, 1)

        result

    
    [<Benchmark>]
    member b.Bar () =
        let sizeIndex = int b.Size
        let bar = bars[sizeIndex]
        let keys = keys[sizeIndex]
        let mutable result = Bar<_,_> Array.empty
        
        for k in keys do
            result <- bar.Add (k, 1)
            
        result


let _ = BenchmarkRunner.Run<Benchmarks>()
