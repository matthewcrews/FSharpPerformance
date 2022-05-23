module TopologicalSort.Version4

(*
Version 4:
We move away from using the F# list and instead use Row/Bar/Array.
This improves our performance due to data locality. We also
use a .NET Queue instead of List for tracking the order of
the nodes and a Stack to track which nodes to process next.
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
        let nodeCount = LanguagePrimitives.Int32WithMeasure<Unit.Node> nodeCount
        let sourcesAcc = Row.create nodeCount []
        let targetsAcc = Row.create nodeCount []
        
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

        
    let decompose (graph: Graph) =
        let nodes = getDistinctNodes graph
        let sources, targets = createSourcesAndTargets nodes.Length graph
        let sourceNodes =
            nodes
            |> Array.filter (fun node -> sources[node].Length = 0)

        sourceNodes, sources, targets
   

type Sources = Bar<Unit.Node, Edge[]>
type Targets = Bar<Unit.Node, Edge[]>

let sort (graph: Graph) =
        
    let sourceNodes, sources, targets = Graph.decompose graph
    
    let toProcess = Stack ()
    for node in sourceNodes do
        toProcess.Push node
        
    let remainingEdges =
        let (Graph edges) = graph
        HashSet edges
        
    let sortedNodes = Queue ()
    
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
