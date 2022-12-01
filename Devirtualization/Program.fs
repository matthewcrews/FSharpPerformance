open System
open Argu
open BenchmarkDotNet.Diagnosers
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running

[<AbstractClass>]
type AbstractLookup<'TKey, 'TValue>() =
    abstract member Item : 'TKey -> 'TValue

type ILookup<'TKey, 'TValue> =
    abstract member Item : 'TKey -> 'TValue

type FuncLookup<'TKey, 'TValue> = ('TKey -> 'TValue)

type ArrayLookup<'TValue>(values: 'TValue[]) =
    inherit AbstractLookup<int, 'TValue>()
    override _.Item (k: int) = values[k]

let valueCount = 10_000
let lookups = [|0..5..valueCount - 1|]

module ExternalFunction =
    let private fArrValues = [|0..valueCount|]
    let funcLookup : FuncLookup<_,_> = fun k -> fArrValues[k]



[<MemoryDiagnoser>]
[<DisassemblyDiagnoser(printSource = true, filters = [||])>]
[<HardwareCounters(
    HardwareCounter.BranchMispredictions,
    HardwareCounter.BranchInstructions,
    HardwareCounter.CacheMisses)>]
type Benchmarks () =
    let arrValues = [|0..valueCount|]
    let arrayLookupValues = [|0..valueCount|]
    let iArrValues = [|0..valueCount|]
    let fArrValues = [|0..valueCount|]

    let arrayLookup = ArrayLookup arrayLookupValues

    let iLookup = { new ILookup<_, _> with
        member _.Item k = iArrValues[k]
    }

    let funcLookup : FuncLookup<_,_> = fun k -> fArrValues[k]


    [<Benchmark>]
    member _.ConcreteArray () =
        let mutable acc = 0

        for i in lookups do
            acc <- acc + arrValues[i]

        acc

    [<Benchmark>]
    member _.AbstractClass () =
        let mutable acc = 0

        for i in lookups do
            acc <- acc + arrayLookup[i]

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
            acc <- acc + funcLookup i

        acc
        
    [<Benchmark>]
    member _.ExternalFunction () =
        let mutable acc = 0

        for i in lookups do
            acc <- acc + ExternalFunction.funcLookup i

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