open System
open Argu
open BenchmarkDotNet.Diagnosers
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running
open TopologicalSort


module Data =

    let nodeCount = 20
    let graphCount = 1_000
    let rngSeed = 123
    let randomEdgeCount =
        [|
            1
            1
            1
            1
            1
            1
            2
            2
            2
            3
        |]
        
    module Version1 =

        open TopologicalSort.Version1
        
        // Create a new random number generator with the same seed
        let rng = Random rngSeed
        
        // Create the list of Nodes that we will use
        let nodes =
            [for i in 0 .. nodeCount - 1 ->
                { Name = $"Node{i}" }]
            
        // Generate the random Graphs we will solve
        let graphs =
            [for _ in 1 .. graphCount ->
                [for sourceIdx in 0 .. nodeCount - 2 do
                    // We use a weighted distribution for the number of edges
                    for _ in 1 .. randomEdgeCount[(rng.Next randomEdgeCount.Length)] do
                        let targetIdx = rng.Next (sourceIdx + 1, nodeCount - 1)
                        { Source = nodes[sourceIdx]; Target = nodes[targetIdx] }]
                |> List.distinct    
                |> Graph
            ]
            
            
    module Version2 =
        
        open TopologicalSort.Version2
        
        // Create a new random number generator with the same seed
        let rng = Random rngSeed
        
        // Create the list of Nodes that we will use
        let nodes =
            [for i in 0 .. nodeCount - 1 ->
                { Name = $"Node{i}" }]
            
        // Generate the random Graphs we will solve
        let graphs =
            [for _ in 1 .. graphCount ->
                [for sourceIdx in 0 .. nodeCount - 2 do
                    // We use a weighted distribution for the number of edges
                    for _ in 1 .. randomEdgeCount[(rng.Next randomEdgeCount.Length)] do
                        let targetIdx = rng.Next (sourceIdx + 1, nodeCount - 1)
                        { Source = nodes[sourceIdx]; Target = nodes[targetIdx] }]
                |> List.distinct    
                |> Graph.create
            ]
            
            
    module Version3 =
        
        open TopologicalSort.Version3
        
        // Create a new random number generator with the same seed
        let rng = Random rngSeed
        
        // Create the list of Nodes that we will use
        let nodes =
            [for i in 0 .. nodeCount - 1 ->
                { Name = $"Node{i}" }]
            
        // Generate the random Graphs we will solve
        let graphs =
            [for _ in 1 .. graphCount ->
                [for sourceIdx in 0 .. nodeCount - 2 do
                    // We use a weighted distribution for the number of edges
                    for _ in 1 .. randomEdgeCount[(rng.Next randomEdgeCount.Length)] do
                        let targetIdx = rng.Next (sourceIdx + 1, nodeCount - 1)
                        { Source = nodes[sourceIdx]; Target = nodes[targetIdx] }]
                |> List.distinct    
                |> Graph.create
            ]
            
            
    module Version4 =
        
        open TopologicalSort.Version4
        
        // Create a new random number generator with the same seed
        let rng = Random rngSeed
        
        // Create the list of Nodes that we will use
        let nodes =
            [for i in 0 .. nodeCount - 1 ->
                Node.create i]
            
        // Generate the random Graphs we will solve
        let graphs =
            [for _ in 1 .. graphCount ->
                [for sourceIdx in 0 .. nodeCount - 2 do
                    // We use a weighted distribution for the number of edges
                    for _ in 1 .. randomEdgeCount[(rng.Next randomEdgeCount.Length)] do
                        let targetIdx = rng.Next (sourceIdx + 1, nodeCount - 1)
                        let source = nodes[sourceIdx]
                        let target = nodes[targetIdx]
                        Edge.create source target ]
                |> List.distinct    
                |> Graph.create
            ]


    module Version5 =
        
        open TopologicalSort.Version5
        
        // Create a new random number generator with the same seed
        let rng = Random rngSeed
        
        // Create the list of Nodes that we will use
        let nodes =
            [for i in 0 .. nodeCount - 1 ->
                Node.create i]
            
        // Generate the random Graphs we will solve
        let graphs =
            [for _ in 1 .. graphCount ->
                [|for sourceIdx in 0 .. nodeCount - 2 do
                     // We use a weighted distribution for the number of edges
                     for _ in 1 .. randomEdgeCount[(rng.Next randomEdgeCount.Length)] do
                         let targetIdx = rng.Next (sourceIdx + 1, nodeCount - 1)
                         let source = nodes[sourceIdx]
                         let target = nodes[targetIdx]
                         Edge.create source target |]
                |> Array.distinct    
                |> Graph.create
            ]

            
    module Version6 =
        
        open TopologicalSort.Version6
        
        // Create a new random number generator with the same seed
        let rng = Random rngSeed
        
        // Create the list of Nodes that we will use
        let nodes =
            [for i in 0 .. nodeCount - 1 ->
                Node.create i]
            
        // Generate the random Graphs we will solve
        let graphs =
            [for _ in 1 .. graphCount ->
                [|for sourceIdx in 0 .. nodeCount - 2 do
                     // We use a weighted distribution for the number of edges
                     for _ in 1 .. randomEdgeCount[(rng.Next randomEdgeCount.Length)] do
                         let targetIdx = rng.Next (sourceIdx + 1, nodeCount - 1)
                         let source = nodes[sourceIdx]
                         let target = nodes[targetIdx]
                         Edge.create source target |]
                |> Array.distinct    
                |> Graph.create
            ]
            
    module Version7 =
        
        open TopologicalSort.Version7
        
        // Create a new random number generator with the same seed
        let rng = Random rngSeed
        
        // Create the list of Nodes that we will use
        let nodes =
            [for i in 0 .. nodeCount - 1 ->
                Node.create i]
            
        // Generate the random Graphs we will solve
        let graphs =
            [for _ in 1 .. graphCount ->
                [|for sourceIdx in 0 .. nodeCount - 2 do
                     // We use a weighted distribution for the number of edges
                     for _ in 1 .. randomEdgeCount[(rng.Next randomEdgeCount.Length)] do
                         let targetIdx = rng.Next (sourceIdx + 1, nodeCount - 1)
                         let source = nodes[sourceIdx]
                         let target = nodes[targetIdx]
                         Edge.create source target |]
                |> Array.distinct    
                |> Graph.create
            ]
            
    module Version8 =
        
        open TopologicalSort.Version8
        
        // Create a new random number generator with the same seed
        let rng = Random rngSeed
        
        // Create the list of Nodes that we will use
        let nodes =
            [for i in 0 .. nodeCount - 1 ->
                Node.create i]
            
        // Generate the random Graphs we will solve
        let graphs =
            [for _ in 1 .. graphCount ->
                [|for sourceIdx in 0 .. nodeCount - 2 do
                     // We use a weighted distribution for the number of edges
                     for _ in 1 .. randomEdgeCount[(rng.Next randomEdgeCount.Length)] do
                         let targetIdx = rng.Next (sourceIdx + 1, nodeCount - 1)
                         let source = nodes[sourceIdx]
                         let target = nodes[targetIdx]
                         Edge.create source target |]
                |> Array.distinct    
                |> Graph.create
            ]
            
    module Version9 =
        
        open TopologicalSort.Version9
        
        // Create a new random number generator with the same seed
        let rng = Random rngSeed
        
        // Create the list of Nodes that we will use
        let nodes =
            [for i in 0 .. nodeCount - 1 ->
                Node.create i]
            
        // Generate the random Graphs we will solve
        let graphs =
            [for _ in 1 .. graphCount ->
                [|for sourceIdx in 0 .. nodeCount - 2 do
                     // We use a weighted distribution for the number of edges
                     for _ in 1 .. randomEdgeCount[(rng.Next randomEdgeCount.Length)] do
                         let targetIdx = rng.Next (sourceIdx + 1, nodeCount - 1)
                         let source = nodes[sourceIdx]
                         let target = nodes[targetIdx]
                         Edge.create source target |]
                |> Array.distinct    
                |> Graph.create
            ]
            
    module Version10 =
        
        open TopologicalSort.Version10
        
        // Create a new random number generator with the same seed
        let rng = Random rngSeed
        
        // Create the list of Nodes that we will use
        let nodes =
            [for i in 0 .. nodeCount - 1 ->
                Node.create i]
            
        // Generate the random Graphs we will solve
        let graphs =
            [for _ in 1 .. graphCount ->
                [|for sourceIdx in 0 .. nodeCount - 2 do
                     // We use a weighted distribution for the number of edges
                     for _ in 1 .. randomEdgeCount[(rng.Next randomEdgeCount.Length)] do
                         let targetIdx = rng.Next (sourceIdx + 1, nodeCount - 1)
                         let source = nodes[sourceIdx]
                         let target = nodes[targetIdx]
                         Edge.create source target |]
                |> Array.distinct    
                |> Graph.create
            ]
            
    module Version11 =
    
        open TopologicalSort.Version11
        
        // Create a new random number generator with the same seed
        let rng = Random rngSeed
        
        // Create the list of Nodes that we will use
        let nodes =
            [for i in 0 .. nodeCount - 1 ->
                Node.create i]
            
        // Generate the random Graphs we will solve
        let graphs =
            [for _ in 1 .. graphCount ->
                [|for sourceIdx in 0 .. nodeCount - 2 do
                     // We use a weighted distribution for the number of edges
                     for _ in 1 .. randomEdgeCount[(rng.Next randomEdgeCount.Length)] do
                         let targetIdx = rng.Next (sourceIdx + 1, nodeCount - 1)
                         let source = nodes[sourceIdx]
                         let target = nodes[targetIdx]
                         Edge.create source target |]
                |> Array.distinct    
                |> Graph.create
            ]
            
            
    module Version12 =
        
        open TopologicalSort.Version12
        
        // Create a new random number generator with the same seed
        let rng = Random rngSeed
        
        // Create the list of Nodes that we will use
        let nodes =
            [for i in 0 .. nodeCount - 1 ->
                Node.create i]
            
        // Generate the random Graphs we will solve
        let graphs =
            [for _ in 1 .. graphCount ->
                [|for sourceIdx in 0 .. nodeCount - 2 do
                     // We use a weighted distribution for the number of edges
                     for _ in 1 .. randomEdgeCount[(rng.Next randomEdgeCount.Length)] do
                         let targetIdx = rng.Next (sourceIdx + 1, nodeCount - 1)
                         let source = nodes[sourceIdx]
                         let target = nodes[targetIdx]
                         Edge.create source target |]
                |> Array.distinct    
                |> Graph.create
            ]
    
    
[<MemoryDiagnoser>]
[<HardwareCounters(
   HardwareCounter.BranchMispredictions,
   HardwareCounter.BranchInstructions,
   HardwareCounter.CacheMisses)>]
// [<DisassemblyDiagnoser>]
type Benchmarks () =
    
   [<Benchmark>]
    member _.V01 () =
        let mutable result = None
        
        for graph in Data.Version1.graphs do
            // I separate the assignment so I can set a breakpoint in debugging
            let sortedOrder = Version1.sort graph
            result <- sortedOrder

        result        
        
   [<Benchmark>]
    member _.V02 () =
        let mutable result = None
        
        for graph in Data.Version2.graphs do
            // I separate the assignment so I can set a breakpoint in debugging
            let sortedOrder = Version2.sort graph
            result <- sortedOrder

        result  
        
   [<Benchmark>]
    member _.V03 () =
        let mutable result = None
        
        for graph in Data.Version3.graphs do
            // I separate the assignment so I can set a breakpoint in debugging
            let sortedOrder = Version3.sort graph
            result <- sortedOrder

        result  
        
    [<Benchmark>]
    member _.V04 () =
        let mutable result = None
        
        for graph in Data.Version4.graphs do
            // I separate the assignment so I can set a breakpoint in debugging
            let sortedOrder = Version4.sort graph
            result <- sortedOrder

        result  
        
    [<Benchmark>]
    member _.V05 () =
        let mutable result = None
        
        for graph in Data.Version5.graphs do
            // I separate the assignment so I can set a breakpoint in debugging
            let sortedOrder = Version5.sort graph
            result <- sortedOrder

        result  

    [<Benchmark>]
    member _.V06 () =
        let mutable result = None
        
        for graph in Data.Version6.graphs do
            // I separate the assignment so I can set a breakpoint in debugging
            let sortedOrder = Version6.sort graph
            result <- sortedOrder

        result
        
    [<Benchmark>]
    member _.V07 () =
        let mutable result = None
        
        for graph in Data.Version7.graphs do
            // I separate the assignment so I can set a breakpoint in debugging
            let sortedOrder = Version7.sort graph
            result <- sortedOrder

        result 

    [<Benchmark>]
    member _.V08 () =
        let mutable result = None
        
        for graph in Data.Version8.graphs do
            // I separate the assignment so I can set a breakpoint in debugging
            let sortedOrder = Version8.sort graph
            result <- sortedOrder

        result

    [<Benchmark>]
    member _.V09 () =
        let mutable result = None
        
        for graph in Data.Version9.graphs do
            // I separate the assignment so I can set a breakpoint in debugging
            let sortedOrder = Version9.sort graph
            result <- sortedOrder

        result

        
    [<Benchmark>]
    member _.V10 () =
        let mutable result = ValueNone
        
        for graph in Data.Version10.graphs do
            // I separate the assignment so I can set a breakpoint in debugging
            let sortedOrder = Version10.Graph.GraphType.Sort &graph
            result <- sortedOrder

        result
        
    [<Benchmark>]
    member _.V11 () =
        let mutable result = None
        
        for graph in Data.Version11.graphs do
            // I separate the assignment so I can set a breakpoint in debugging
            let sortedOrder = Version11.sort graph
            result <- sortedOrder

        result
        
        
    [<Benchmark>]
    member _.V12 () =
        let mutable result = None
        
        for graph in Data.Version12.graphs do
            // I separate the assignment so I can set a breakpoint in debugging
            let sortedOrder = Version12.sort graph
            result <- sortedOrder

        result



let profile (version: string) loopCount =
    
    let b = Benchmarks ()
    let mutable result = 0
    
    match version.ToLower() with
    | "v01" ->
        for i in 1 .. loopCount do
            match b.V01 () with
            | Some order -> result <- result + 1
            | None -> result <- result - 1
            
    | "v02" ->
        for i in 1 .. loopCount do
            match b.V02 () with
            | Some order -> result <- result + 1
            | None -> result <- result - 1
            
    | "v03" ->
        for i in 1 .. loopCount do
            match b.V03 () with
            | Some order -> result <- result + 1
            | None -> result <- result - 1
            
            
    | "v04" ->
        for i in 1 .. loopCount do
            match b.V04 () with
            | Some order -> result <- result + 1
            | None -> result <- result - 1
            
            
    | "v05" ->
        for i in 1 .. loopCount do
            match b.V05 () with
            | Some order -> result <- result + 1
            | None -> result <- result - 1
            
    | "v06" ->
        for i in 1 .. loopCount do
            match b.V06 () with
            | Some order -> result <- result + 1
            | None -> result <- result - 1

    | "v07" ->
        for i in 1 .. loopCount do
            match b.V07 () with
            | Some order -> result <- result + 1
            | None -> result <- result - 1
            
    | "v08" ->
        for i in 1 .. loopCount do
            match b.V08 () with
            | Some order -> result <- result + 1
            | None -> result <- result - 1
            
    | "v09" ->
        for i in 1 .. loopCount do
            match b.V09 () with
            | Some order -> result <- result + 1
            | None -> result <- result - 1
            
    | "v10" ->
        for i in 1 .. loopCount do
            match b.V10 () with
            | ValueSome order -> result <- result + 1
            | ValueNone -> result <- result - 1
            
    | "v11" ->
        for i in 1 .. loopCount do
            match b.V11 () with
            | Some order -> result <- result + 1
            | None -> result <- result - 1
            
    | "v12" ->
        for i in 1 .. loopCount do
            match b.V12 () with
            | Some order -> result <- result + 1
            | None -> result <- result - 1

            
    | unknownVersion -> failwith $"Unknown version: {unknownVersion}" 
            
    result

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
