module TopologicalSort.Version6

(*
Version 6:
Instead of a HashSet for tracking the remaining Edges, we use
a custom EdgeTracker. EdgeTracker is faster because indexing
into an array is faster than a HashSet contains check even though
both of a complexity of O(1)
*)

open System.Collections.Generic
open Collections

[<RequireQualifiedAccess>]
module private Units =

    [<Measure>] type Node
    [<Measure>] type Edge


type Node = int<Units.Node>

module Node =
    
    let create (i: int) =
        if i < 0 then
            invalidArg (nameof i) "Cannot have a Node less than 0"
            
        LanguagePrimitives.Int32WithMeasure<Units.Node> i


type Edge = int64<Units.Edge>

module Edge =

    let create (source: Node) (target: Node) =
        (((int64 source) <<< 32) ||| (int64 target))
        |> LanguagePrimitives.Int64WithMeasure<Units.Edge>
        
    let getSource (edge: Edge) =
        ((int64 edge) >>> 32)
        |> int
        |> LanguagePrimitives.Int32WithMeasure<Units.Node>

    let getTarget (edge: Edge) =
        int edge
        |> LanguagePrimitives.Int32WithMeasure<Units.Node>
        

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
        b.Values[bucket] <- b.Values[bucket] &&& ~~~mask

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


type Sources = Bar<Units.Node, Edge[]>
type Targets = Bar<Units.Node, Edge[]>

type Graph = {
    Sources : Sources
    Targets : Targets
}
    
module Graph =
    
    let private getNodeCount (edges: Edge[]) =
        let nodes = HashSet()
        
        for edge in edges do
            let source = Edge.getSource edge
            let target = Edge.getTarget edge
            nodes.Add source |> ignore
            nodes.Add target |> ignore
            
        nodes.Count
    
    let private createSourcesAndTargets (nodeCount: int) (edges: Edge[]) =
        let nodeCount = LanguagePrimitives.Int32WithMeasure<Units.Node> nodeCount
        let mutable sourcesAcc = Row.create nodeCount []
        let mutable targetsAcc = Row.create nodeCount []
        
        for edge in edges do
            let source = Edge.getSource edge
            let target = Edge.getTarget edge
            
            sourcesAcc[target] <- edge :: sourcesAcc[target]
            targetsAcc[source] <- edge :: targetsAcc[source]
            
        let finalSources =
            sourcesAcc
            |> Row.map Array.ofList
            
        let finalTargets =
            targetsAcc
            |> Row.map Array.ofList
            
        finalSources.Bar, finalTargets.Bar

        
    let create (edges: Edge[]) =
        let nodeCount = getNodeCount edges
        let sources, targets = createSourcesAndTargets nodeCount edges
        {
            Sources = sources
            Targets = targets
        }


let sort (graph: Graph) =
        
    let toProcess = Stack<Node> ()
    let sortedNodes = Queue<Node> ()

    graph.Sources
    |> Bar.iteri (fun nodeId edges ->
        if edges.Length = 0 then
            toProcess.Push nodeId)
        
    let remainingEdges = EdgeTracker (int graph.Targets.Length)

    graph.Targets
    |> Bar.iter (fun edges ->
        for edge in edges do
            remainingEdges.Add edge)
    
    while toProcess.Count > 0 do
        let nextNode = toProcess.Pop()
        sortedNodes.Enqueue nextNode

        graph.Targets[nextNode]
        |> Array.iter (fun edge ->
            let target = Edge.getTarget edge
            remainingEdges.Remove edge
            
            let noRemainingSources =
                graph.Sources[target]
                |> Array.forall (remainingEdges.Contains >> not)
                
            if noRemainingSources then
                toProcess.Push target
        
        )

    if remainingEdges.Count > 0 then
        None
    else
        Some (sortedNodes.ToArray())
