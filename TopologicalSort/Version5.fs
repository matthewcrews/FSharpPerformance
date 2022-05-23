module TopologicalSort.Version5

(*
Version 5:
Instead of a HashSet for tracking the remaining Edges, we use
a custom BitSetTracker. BitSetTracker is faster because indexing
into an array is faster than a HashSet contains check even though
both of a complexity of O(1)
*)

open System.Collections.Generic
open Row

[<RequireQualifiedAccess>]
module private Unit =

    [<Measure>] type Node
    [<Measure>] type Edge


type Node = int<Unit.Node>

module Node =
    
    let create (i: int) =
        if i < 0 then
            invalidArg (nameof i) "Cannot have a Node less than 0"
            
        LanguagePrimitives.Int32WithMeasure<Unit.Node> i


type Edge = int64<Unit.Edge>

module Edge =

    let create (source: Node) (target: Node) =
        (((int64 source) <<< 32) ||| (int64 target))
        |> LanguagePrimitives.Int64WithMeasure<Unit.Edge>
        
    let getSource (edge: Edge) =
        ((int64 edge) >>> 32)
        |> int
        |> LanguagePrimitives.Int32WithMeasure<Unit.Node>

    let getTarget (edge: Edge) =
        int edge
        |> LanguagePrimitives.Int32WithMeasure<Unit.Node>
        

type EdgeTracker (nodeCount: int) =
    let bitsRequired = ((nodeCount * nodeCount) + 63) / 64
    let values = Array.create bitsRequired 0UL
    
    // Public for the purposes of inlining
    member b.NodeCount = nodeCount
    member b.Values = values
    
    member inline b.Add (edge: Edge) =
        let source = Edge.getSource edge
        let target = Edge.getTarget edge
        let location = (int source) * b.NodeCount + (int target)
        let bucket = location >>> 6
        let offset = location &&& 0x3F
        let mask = 1UL <<< offset
        b.Values[bucket] <- b.Values[bucket] ||| mask
        
    member inline b.Remove (edge: Edge) =
        let source = Edge.getSource edge
        let target = Edge.getTarget edge
        let location = (int source) * b.NodeCount + (int target)
        let bucket = location >>> 6
        let offset = location &&& 0x3F
        let mask = 1UL <<< offset
        b.Values[bucket] <- b.Values[bucket] &&& (~~~mask)

    member inline b.Contains (edge: Edge) =
        let source = Edge.getSource edge
        let target = Edge.getTarget edge
        let location = (int source) * b.NodeCount + (int target)
        let bucket = location >>> 6
        let offset = location &&& 0x3F
        ((b.Values[bucket] >>> offset) &&& 1UL) = 1UL

    member b.Clear () =
        for i = 0 to b.Values.Length - 1 do
            b.Values[i] <- 0UL

    member b.Count =
        let mutable count = 0
        
        for i = 0 to b.Values.Length - 1 do
            count <- count + (System.Numerics.BitOperations.PopCount b.Values[i])

        count


type Graph = Graph of Edge[]
    
module Graph =
    
    let private getDistinctNodes (Graph edges) =

        let distinctNodes = HashSet()
        
        for edge in edges do
            let source = Edge.getSource edge
            let target = Edge.getTarget edge
            distinctNodes.Add source |> ignore
            distinctNodes.Add target |> ignore
        
        Array.ofSeq distinctNodes

    
    let private createSourcesAndTargets (nodeCount: int) (Graph edges) =
        let sourcesAcc =
            [|for _ in 0 .. nodeCount - 1 -> Stack ()|]
            |> Row<Unit.Node, _>
        let targetsAcc =
            [|for _ in 0 .. nodeCount - 1 -> Stack ()|]
            |> Row<Unit.Node, _>
            
        for edge in edges do
            let source = Edge.getSource edge
            let target = Edge.getTarget edge
            
            sourcesAcc[target].Push edge
            targetsAcc[source].Push edge
            
        let finalSources =
            sourcesAcc
            |> Row.map (fun stack -> stack.ToArray())
            
        let finalTargets =
            targetsAcc
            |> Row.map (fun stack -> stack.ToArray())
            
        finalSources.Bar, finalTargets.Bar

        
    let decompose (graph: Graph) =
        let nodes = getDistinctNodes graph
        let sources, targets = createSourcesAndTargets nodes.Length graph
        let sourceNodes =
            nodes
            |> Array.filter (fun node -> sources[node].Length = 0)

        sourceNodes, nodes.Length, sources, targets


let sort (graph: Graph) =
        
    let sourceNodes, nodeCount, sources, targets = Graph.decompose graph
    let toProcess = Stack<Node> ()
    let sortedNodes = Queue<Node> ()

    for node in sourceNodes do
        toProcess.Push node
        
    let remainingEdges = EdgeTracker nodeCount
    let (Graph edges) = graph
    for edge in edges do
        remainingEdges.Add edge |> ignore
    
    while toProcess.Count > 0 do
        let nextNode = toProcess.Pop()
        sortedNodes.Enqueue nextNode
        
        targets[nextNode]
        |> Array.iter (fun edge ->
            let target = Edge.getTarget edge
            remainingEdges.Remove edge |> ignore
            
            let noRemainingSources =
                sources[target]
                |> Array.forall (remainingEdges.Contains >> not)
                
            if noRemainingSources then
                toProcess.Push target
        
        )

    if remainingEdges.Count > 0 then
        None
    else
        Some (sortedNodes.ToArray())
