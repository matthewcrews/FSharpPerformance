open System
open Argu
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running
open BitSetEnumeration


[<Measure>]
type Chicken


[<MemoryDiagnoser>]
type Benchmarks() =

    let rng = Random 123
    let capacity = 100
    let setBitCount = 20

    let randomSetBitPositions =
        [| for _ in 1..setBitCount -> (rng.Next capacity) * 1<Chicken> |]

    let iteriBitSet =
        let b = Iter.BitSet<Chicken> capacity

        for randomBit in randomSetBitPositions do
            b.Add randomBit

        b

    let enumerableBitSet =
        let b = Enumerable.BitSet capacity

        for randomBit in randomSetBitPositions do
            b.Add randomBit

        b

    let structEnumerableBitSet =
        let b = StructEnumerable.BitSet capacity

        for randomBit in randomSetBitPositions do
            b.Add randomBit

        b
        
    let duckTypingBitSet =
        let b = DuckTyping.BitSet capacity

        for randomBit in randomSetBitPositions do
            b.Add randomBit

        b


    [<Benchmark>]
    member _.Iter() =
        let mutable acc = 0<_>

        iteriBitSet |> Iter.BitSet.iter (fun x -> acc <- acc + x)

        acc

    [<Benchmark>]
    member _.Enumerable() =
        let mutable acc = 0<_>

        for x in enumerableBitSet do
            acc <- acc + x

        acc

    [<Benchmark>]
    member _.StructEnumerable() =
        let mutable acc = 0<_>

        for x in structEnumerableBitSet do
            acc <- acc + x

        acc
        
    [<Benchmark>]
    member _.DuckTyping() =
        let mutable acc = 0<_>

        for x in duckTypingBitSet do
            acc <- acc + x

        acc


let profile (benchmark: string) (iterations: int) =
    let b = Benchmarks()
    let mutable acc = LanguagePrimitives.Int32WithMeasure<_> 0
    
    printfn "Starting loops..."
    match benchmark.ToLower() with
    | "iter" ->
        for _ in 1 .. iterations do
            acc <- acc + b.Iter()
    | "enumerable" ->
        for _ in 1 .. iterations do
            acc <- acc + b.Enumerable()
    | "structenumerable" ->
        for _ in 1 .. iterations do
            acc <- acc + b.StructEnumerable()
    | "ducktyping" ->
        for _ in 1 .. iterations do
            acc <- acc + b.DuckTyping()
            
    | _ -> failwith "Unknown benchmark"


[<RequireQualifiedAccess>]
type Args =
    | Task of task: string
    | Benchmark of method: string
    | Iterations of iterations: int
    
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Task _ -> "Which task to perform. Options: Benchmark or Profile"
            | Benchmark _ -> "Which Benchmark to profile"
            | Iterations _ -> "Number of iterations of the Method to perform for profiling"


[<EntryPoint>]
let main (args: string[]) =

    let parser = ArgumentParser.Create<Args> (programName = "Topological Sort")
    let results = parser.Parse args
    let task = results.GetResult Args.Task

    
    match task.ToLower() with
    | "benchmark" -> 
        let _ = BenchmarkRunner.Run<Benchmarks>()
        ()

    | "profile" ->
        let benchmark = results.GetResult Args.Benchmark
        let iterations = results.GetResult Args.Iterations
        let _ = profile benchmark iterations
        ()
        
    | unknownTask -> failwith $"Unknown task: {unknownTask}"
    
    1
