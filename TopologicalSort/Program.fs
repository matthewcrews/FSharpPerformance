open System
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running
open Version1

    
[<MemoryDiagnoser>]
type Benchmarks () =
    
    let v1Nodes =
        [|
            { Name = "0" }
            { Name = "1" }
            { Name = "2" }
            { Name = "3" }
            { Name = "4" }
            { Name = "5" }
        |]

    let v1Edges =
        [
            { Source = v1Nodes[0]; Target = v1Nodes[1] }
            { Source = v1Nodes[0]; Target = v1Nodes[2] }
            { Source = v1Nodes[1]; Target = v1Nodes[3] }
            { Source = v1Nodes[1]; Target = v1Nodes[4] }
            { Source = v1Nodes[2]; Target = v1Nodes[1] }
            { Source = v1Nodes[2]; Target = v1Nodes[4] }
            { Source = v1Nodes[3]; Target = v1Nodes[5] }
            { Source = v1Nodes[4]; Target = v1Nodes[5] }
        ]
        
    let v1Graph = Graph.create v1Edges
    
    
    [<Benchmark>]
    member _.V1 () =
    
        let result = Topological.sort v1Graph
        result


let profile loopCount =
    
    let b = Benchmarks ()
    let mutable result = None
    
    for i in 1 .. loopCount do
        result <- b.V1 ()
        
    result



[<EntryPoint>]
let main args =

    match args[0].ToLower() with
    | "benchmark" ->
    
        let _ = BenchmarkRunner.Run<Benchmarks>()
        ()
        
    | "profile" ->
        let loopCount = int args[1]
        let _ = profile loopCount
        ()
        
    | unknownCommand -> failwith $"Unknown command: {unknownCommand}"
    
    
    1
