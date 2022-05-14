open System
open BenchmarkDotNet.Diagnosers
open TopologicalSort
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running

module V1Data =
    
    open TopologicalSort.Version1
    
    let nodes =
        [|
            { Name = "0" }
            { Name = "1" }
            { Name = "2" }
            { Name = "3" }
            { Name = "4" }
            { Name = "5" }
        |]

    let edges =
        [
            { Source = nodes[0]; Target = nodes[1] }
            { Source = nodes[0]; Target = nodes[2] }
            { Source = nodes[1]; Target = nodes[3] }
            { Source = nodes[1]; Target = nodes[4] }
            { Source = nodes[2]; Target = nodes[1] }
            { Source = nodes[2]; Target = nodes[4] }
            { Source = nodes[3]; Target = nodes[5] }
            { Source = nodes[4]; Target = nodes[5] }
        ]
        
    let graph = Graph.create edges
    
    
module V2Data =
    
    open TopologicalSort.Version2
    
    let nodes =
        [|
            { Name = "0" }
            { Name = "1" }
            { Name = "2" }
            { Name = "3" }
            { Name = "4" }
            { Name = "5" }
        |]

    let edges =
        [
            { Source = nodes[0]; Target = nodes[1] }
            { Source = nodes[0]; Target = nodes[2] }
            { Source = nodes[1]; Target = nodes[3] }
            { Source = nodes[1]; Target = nodes[4] }
            { Source = nodes[2]; Target = nodes[1] }
            { Source = nodes[2]; Target = nodes[4] }
            { Source = nodes[3]; Target = nodes[5] }
            { Source = nodes[4]; Target = nodes[5] }
        ]
        
    let graph = Graph.create edges


module V3Data =
    
    open TopologicalSort.Version3
    
    let nodes =
        [|
            { Name = "0" }
            { Name = "1" }
            { Name = "2" }
            { Name = "3" }
            { Name = "4" }
            { Name = "5" }
        |]

    let edges =
        [
            { Source = nodes[0]; Target = nodes[1] }
            { Source = nodes[0]; Target = nodes[2] }
            { Source = nodes[1]; Target = nodes[3] }
            { Source = nodes[1]; Target = nodes[4] }
            { Source = nodes[2]; Target = nodes[1] }
            { Source = nodes[2]; Target = nodes[4] }
            { Source = nodes[3]; Target = nodes[5] }
            { Source = nodes[4]; Target = nodes[5] }
        ]
        
    let graph = Graph.create edges
  
  
module V4Data =
    
    open TopologicalSort.Version4

    let nodes =
        [|
            0<Node>
            1<Node>
            2<Node>
            3<Node>
            4<Node>
            5<Node>
        |]

    let edges =
        [
            Edge.create nodes[0] nodes[1]
            Edge.create nodes[0] nodes[2]
            Edge.create nodes[1] nodes[3]
            Edge.create nodes[1] nodes[4]
            Edge.create nodes[2] nodes[1]
            Edge.create nodes[2] nodes[4]
            Edge.create nodes[3] nodes[5]
            Edge.create nodes[4] nodes[5]
        ]
        
    let graph = Graph.create edges  

    
module V5Data =

    open TopologicalSort.Version5

    let nodes =
        [|
            0<Node>
            1<Node>
            2<Node>
            3<Node>
            4<Node>
            5<Node>
        |]

    let edges =
        [|
            Edge.create nodes[0] nodes[1]
            Edge.create nodes[0] nodes[2]
            Edge.create nodes[1] nodes[3]
            Edge.create nodes[1] nodes[4]
            Edge.create nodes[2] nodes[1]
            Edge.create nodes[2] nodes[4]
            Edge.create nodes[3] nodes[5]
            Edge.create nodes[4] nodes[5]
        |]
        
    let graph = Graph.create edges 
    
    
module V6Data =

    open TopologicalSort.Version6

    let nodes =
        [|
            0<Node>
            1<Node>
            2<Node>
            3<Node>
            4<Node>
            5<Node>
        |]

    let edges =
        [|
            Edge.create nodes[0] nodes[1]
            Edge.create nodes[0] nodes[2]
            Edge.create nodes[1] nodes[3]
            Edge.create nodes[1] nodes[4]
            Edge.create nodes[2] nodes[1]
            Edge.create nodes[2] nodes[4]
            Edge.create nodes[3] nodes[5]
            Edge.create nodes[4] nodes[5]
        |]
        
    let graph = Graph.create edges
    
module V7Data =

    open TopologicalSort.Version7

    let nodes =
        [|
            0<Node>
            1<Node>
            2<Node>
            3<Node>
            4<Node>
            5<Node>
        |]

    let edges =
        [|
            Edge.create nodes[0] nodes[1]
            Edge.create nodes[0] nodes[2]
            Edge.create nodes[1] nodes[3]
            Edge.create nodes[1] nodes[4]
            Edge.create nodes[2] nodes[1]
            Edge.create nodes[2] nodes[4]
            Edge.create nodes[3] nodes[5]
            Edge.create nodes[4] nodes[5]
        |]
        
    let graph = Graph.create edges
    
    
[<MemoryDiagnoser>]
[<HardwareCounters(HardwareCounter.BranchMispredictions,
                   HardwareCounter.BranchInstructions
//                   HardwareCounter.CacheMisses
                   )>]
type Benchmarks () =
    
    
//    [<Benchmark>]
//    member _.V1 () =
//    
//        let result = Version1.Topological.sort V1Data.graph
//        match result with
//        | Some _ -> 1
//        | None -> 1
//        
//        
//    [<Benchmark>]
//    member _.V2 () =
//    
//        let result = Version2.Topological.sort V2Data.graph
//        match result with
//        | Some _ -> 1
//        | None -> 1
//        
//        
//    [<Benchmark>]
//    member _.V3 () =
//    
//        let result = Version3.Topological.sort V3Data.graph
//        match result with
//        | Some _ -> 1
//        | None -> 1
//        
//        
//    [<Benchmark>]
//    member _.V4 () =
//    
//        let result = Version4.Topological.sort V4Data.graph
//        match result with
//        | Some _ -> 1
//        | None -> 1
//        
//        
//    [<Benchmark>]
//    member _.V5 () =
//    
//        let result = Version5.Topological.sort V5Data.graph
//        match result with
//        | Some _ -> 1
//        | None -> 1
//        
        
    [<Benchmark>]
    member _.V6 () =
    
        let result = Version6.Topological.sort V6Data.graph
        match result with
        | Some _ -> 1
        | None -> 1
    
    [<Benchmark>]
    member _.V7 () =
    
        let result = Version7.Topological.sort V7Data.graph
        match result with
        | Some _ -> 1
        | None -> 1


let profile (version: string) loopCount =
    
    let b = Benchmarks ()
    let mutable result = 0
    
    match version.ToLower() with
//    | "v1" ->
//        for i in 1 .. loopCount do
//            result <- result + b.V1 ()
//            
//    | "v2" ->
//        for i in 1 .. loopCount do
//            result <- result + b.V2 ()
//            
//    | "v3" ->
//        for i in 1 .. loopCount do
//            result <- result + b.V3 ()
//            
//    | "v4" ->
//        for i in 1 .. loopCount do
//            result <- result + b.V4 ()
//            
//    | "v5" ->
//        for i in 1 .. loopCount do
//            result <- result + b.V5 ()
//            
    | "v6" ->
        for i in 1 .. loopCount do
            result <- result + b.V6 ()
    | "v7" ->
        for i in 1 .. loopCount do
            result <- result + b.V7 ()
            
    | unknownVersion -> failwith $"Unknown version: {unknownVersion}" 
            
    result



[<EntryPoint>]
let main args =

    match args[0].ToLower() with
    | "benchmark" ->
    
        let _ = BenchmarkRunner.Run<Benchmarks>()
        ()
        
    | "profile" ->
        let version = args[1]
        let loopCount = int args[2]
        let _ = profile version loopCount
        ()
        
    | unknownCommand -> failwith $"Unknown command: {unknownCommand}"
    
    
    1
