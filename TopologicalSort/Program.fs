open System
open BenchmarkDotNet.Diagnosers
open TopologicalSort
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running

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
                |> Graph
            ]
            
            
    module Version3 =
        
        open TopologicalSort.Version3
        
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
                |> Graph
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
                [|for sourceIdx in 0 .. nodeCount - 2 do
                     // We use a weighted distribution for the number of edges
                     for _ in 1 .. randomEdgeCount[(rng.Next randomEdgeCount.Length)] do
                         let targetIdx = rng.Next (sourceIdx + 1, nodeCount - 1)
                         let source = nodes[sourceIdx]
                         let target = nodes[targetIdx]
                         Edge.create source target |]
                |> Array.distinct    
                |> Graph
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
                |> Graph
            ]
            
    
[<MemoryDiagnoser>]
[<HardwareCounters(HardwareCounter.BranchMispredictions,
                   HardwareCounter.BranchInstructions,
                   HardwareCounter.CacheMisses)>]
type Benchmarks () =
    
    
    [<Benchmark>]
    member _.V1 () =
        let mutable result = None
        
        for graph in Data.Version1.graphs do
            // I separate the assignment so I can set a breakpoint in debugging
            let sortedOrder = Version1.sort graph
            result <- sortedOrder

        result        
        
    [<Benchmark>]
    member _.V2 () =
        let mutable result = None
        
        for graph in Data.Version2.graphs do
            // I separate the assignment so I can set a breakpoint in debugging
            let sortedOrder = Version2.sort graph
            result <- sortedOrder

        result  
        
        
    [<Benchmark>]
    member _.V3 () =
        let mutable result = None
        
        for graph in Data.Version3.graphs do
            // I separate the assignment so I can set a breakpoint in debugging
            let sortedOrder = Version3.sort graph
            result <- sortedOrder

        result  
        
        
    [<Benchmark>]
    member _.V4 () =
        let mutable result = None
        
        for graph in Data.Version4.graphs do
            // I separate the assignment so I can set a breakpoint in debugging
            let sortedOrder = Version4.sort graph
            result <- sortedOrder

        result  
        
        
    [<Benchmark>]
    member _.V5 () =
        let mutable result = None
        
        for graph in Data.Version5.graphs do
            // I separate the assignment so I can set a breakpoint in debugging
            let sortedOrder = Version5.sort graph
            result <- sortedOrder

        result  


let profile (version: string) loopCount =
    
    let b = Benchmarks ()
    let mutable result = 0
    
    match version.ToLower() with
    | "v1" ->
        for i in 1 .. loopCount do
            match b.V1 () with
            | Some order -> result <- result + 1
            | None -> result <- result - 1
            
    | "v2" ->
        for i in 1 .. loopCount do
            match b.V2 () with
            | Some order -> result <- result + 1
            | None -> result <- result - 1
            
    | "v3" ->
        for i in 1 .. loopCount do
            match b.V3 () with
            | Some order -> result <- result + 1
            | None -> result <- result - 1
            
            
    | "v4" ->
        for i in 1 .. loopCount do
            match b.V4 () with
            | Some order -> result <- result + 1
            | None -> result <- result - 1
            
            
    | "v5" ->
        for i in 1 .. loopCount do
            match b.V5 () with
            | Some order -> result <- result + 1
            | None -> result <- result - 1

            
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
