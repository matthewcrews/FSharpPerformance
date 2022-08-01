open System
open System.Collections.Generic
open Argu
open BenchmarkDotNet.Diagnosers
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running


type Int64Comparer () =
    interface IComparer<int64> with
        member _.Compare (x: int64, y: int64) = (# "cgt" x y : int #) - (# "clt" x y : int #)
    

type Benchmarks () =
  
    let elementCount = 1_000_000
    let customComparer = Int64Comparer()
    let values =
        [|for i in 1 .. elementCount -> struct (int64 i, i)|]

    let defaultPriorityQueue = PriorityQueue()
    let customPriorityQueue = PriorityQueue (customComparer)

    [<Benchmark>]
    member _.DefaultAdd () =
        defaultPriorityQueue.Clear()
        
        for (priority, value) in values do
            defaultPriorityQueue.Enqueue (value, priority)
        defaultPriorityQueue

    [<Benchmark>]
    member _.CustomAdd () =
        customPriorityQueue.Clear()
        
        for (priority, value) in values do
            customPriorityQueue.Enqueue (value, priority)
        customPriorityQueue

    [<Benchmark>]
    member _.DefaultAddRemove () =
        defaultPriorityQueue.Clear()

        for (priority, value) in values do
            defaultPriorityQueue.Enqueue (value, priority)
        
        let mutable result = 0

        while defaultPriorityQueue.Count > 0 do
            result <- defaultPriorityQueue.Dequeue()

        result

    [<Benchmark>]
    member _.CustomAddRemove () =
        customPriorityQueue.Clear()

        for (priority, value) in values do
            customPriorityQueue.Enqueue (value, priority)
        
        let mutable result = 0

        while customPriorityQueue.Count > 0 do
            result <- customPriorityQueue.Dequeue()

        result


let profile (version: string) loopCount =
    
    ()

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


[<EntryPoint>]
let main argv =

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
