module TopologicalSort.Version5

(*
Version 5:
We move away from using the F# list and instead use Row/Bar/Array.
This improves our performance due to data locality. We also
use a .NET Queue instead of List for tracking the order of
the nodes and a Stack to track which nodes to process next.
*)

open System.Collections.Generic
open Row


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

        
    let create (edges: Edge[]) =
        let nodeCount = getNodeCount edges
        let sources, targets = createSourcesAndTargets nodeCount edges
        {
            Sources = sources
            Targets = targets
        }


let sort (graph: Graph) =
    
    let toProcess = Stack ()
    graph.Sources
    |> Bar.iteri (fun nodeId edges ->
        if edges.Length = 0 then
            toProcess.Push nodeId)
        
    let remainingEdges =
        let x = HashSet ()
        
        graph.Targets
        |> Bar.iter (fun edges ->
            for edge in edges do
                x.Add edge |> ignore)
        
        x
        
        
    let sortedNodes = Queue ()
    
    while toProcess.Count > 0 do
        let nextNode = toProcess.Pop()
        sortedNodes.Enqueue nextNode
        
        graph.Targets[nextNode]
        |> Array.iter (fun edge ->
            let target = Edge.getTarget edge
            remainingEdges.Remove edge |> ignore
            
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
