open System
open Argu
open BenchmarkDotNet.Diagnosers
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running

type ILookup<'K, 'V> =
    abstract member Item : 'K -> 'V

type FLookup<'K, 'V> = ('K -> 'V)


[<MemoryDiagnoser>]
[<HardwareCounters(
    HardwareCounter.BranchMispredictions,
    HardwareCounter.BranchInstructions,
    HardwareCounter.CacheMisses)>]
type Benchmarks () =
    let valueCount = 10_000
    let lookups = [|0..5..valueCount - 1|]
    let arrValues = [|0..valueCount|]
    let iArrValues = [|0..valueCount|]
    let fArrValues = [|0..valueCount|]

    let iLookup = { new ILookup<_, _> with
        member _.Item k = iArrValues[k]
    }

    let fLookup : FLookup<_,_> = fun k -> fArrValues[k]


    [<Benchmark>]
    member _.Array () =
        let mutable acc = 0

        for i in lookups do
            acc <- acc + arrValues[i]

        acc

    [<Benchmark>]
    member _.Interface () =
        let mutable acc = 0

        for i in lookups do
            acc <- acc + iLookup[i]

        acc

    [<Benchmark>]
    member _.Function () =
        let mutable acc = 0

        for i in lookups do
            acc <- acc + fLookup i

        acc


[<RequireQualifiedAccess>]
type Args =
    | Task of task: string
    | Method of method: string
    | Iterations of iterations: int
    
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Task _ -> "Which task to perform. Options: Benchmark or Profile"
            | Method _ -> "Which Method to profile. Options: V<number>. <number> = 01 - 10"
            | Iterations _ -> "Number of iterations of the Method to perform for profiling"


let profile (version: string) loopCount =
    
    ()


[<EntryPoint>]
let main argv =

    printfn $"Args: {argv}"
    
    let parser = ArgumentParser.Create<Args> (programName = "Topological Sort")
    let results = parser.Parse argv
    let task = results.GetResult Args.Task

    match task.ToLower() with
    | "benchmark" -> 
        let _ = BenchmarkRunner.Run<Benchmarks>()
        ()

    | "profile" ->
        let method = results.GetResult Args.Method
        let iterations = results.GetResult Args.Iterations
        let _ = profile method iterations
        ()
        
    | unknownTask -> failwith $"Unknown task: {unknownTask}"
    
    1