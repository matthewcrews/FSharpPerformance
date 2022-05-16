module TopologicalSort.Version6

(*
Version 6:
Here we use the idea of Pooling resources so that we don't
have to allocation collections each time we want to perform
a sort. This reduces the overall runtime if sort is called
frequently.
*)

open System.Collections.Generic
open Row


[<Measure>] type Node
[<Measure>] type Edge


module Edge =

    let create (source: int<Node>) (target: int<Node>) =
        (((int64 source) <<< 32) ||| (int64 target))
        |> LanguagePrimitives.Int64WithMeasure<Edge>
        
    let getSource (edge: int64<Edge>) =
        ((int64 edge) >>> 32)
        |> int
        |> LanguagePrimitives.Int32WithMeasure<Node>

    let getTarget (edge: int64<Edge>) =
        int edge
        |> LanguagePrimitives.Int32WithMeasure<Node>


type Graph =
    {
        Nodes : int<Node> array
        Origins : int<Node> array
        Edges : int64<Edge> array
        Sources : ReadOnlyRow<Node, int64<Edge> array>
        Targets : ReadOnlyRow<Node, int64<Edge> array>
    }
    
module Graph =
    
    let private getDistinctNodes (edges: int64<Edge> array) =

        let distinctNodes = HashSet()
        
        for edge in edges do
            let source = Edge.getSource edge
            let target = Edge.getTarget edge
            distinctNodes.Add source |> ignore
            distinctNodes.Add target |> ignore
        
        Array.ofSeq distinctNodes

    
    let private createSourcesAndTargets (nodeCount: int) (edges: int64<Edge> array) =
        let nodeCount = LanguagePrimitives.Int32WithMeasure<Node> nodeCount
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
            |> ReadOnlyRow
            
        let finalTargets =
            targetsAcc
            |> Row.map Array.ofList
            |> ReadOnlyRow
            
        finalSources, finalTargets

        
    let create (edges: int64<Edge> array) =
        let edges = Array.distinct edges
        let nodes = getDistinctNodes edges
        let sources, targets = createSourcesAndTargets nodes.Length edges
        let originNodes =
            nodes
            |> Array.filter (fun node -> sources[node].Length = 0)
        {
            Edges = edges
            Nodes = nodes
            Sources = sources
            Targets = targets
            Origins = originNodes
        }
     
     
[<RequireQualifiedAccess>]
module Topological =

    let private toProcess = Stack ()
    let private sortedNodes = Queue<int<Node>> ()
    let private remainingEdges = HashSet<int64<Edge>> ()
    
    let sort (graph: Graph) =
            
        toProcess.Clear()
        sortedNodes.Clear()
        remainingEdges.Clear ()
        
        for node in graph.Origins do
            toProcess.Push node    

        for edge in graph.Edges do
            remainingEdges.Add edge |> ignore
        
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
