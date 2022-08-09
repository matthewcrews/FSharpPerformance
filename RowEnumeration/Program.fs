open System
open Argu
open BenchmarkDotNet.Diagnosers
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running
open RowEnumeration


[<Measure>] type Entity

[<MemoryDiagnoser>]
type Benchmarks () =
    
    let arrValues = [|1..100|]
    let rowValues = Row<Entity,_> arrValues
    
    
    [<Benchmark>]
    member _.ArraySum () =
        let mutable acc = 0
        
        for value in arrValues do
            acc <- acc + value
            
        acc
    
    
    [<Benchmark>]
    member _.RowSum () =
        let mutable acc = 0
        
        for entityId, value in rowValues do
            acc <- acc + value
            
        acc
        

let profile method iterations =
    
    for _ in 1 .. iterations do
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
            | Method _ -> "Which Method to profile"
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