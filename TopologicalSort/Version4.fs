module TopologicalSort.Version4

(*
Version 4:
The default comparison built into F# can be slow when compared
to the performance of using raw primitives. Instead of using
Records to model our domain, we use primitive types that have
been annotated with Units of Measure
*)

open System.Collections.Generic
open System.Collections.ObjectModel
open Microsoft.FSharp.Core


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


type Sources = ReadOnlyDictionary<Node, Edge list>
type Targets = ReadOnlyDictionary<Node, Edge list>

type Graph = {
    Sources : Sources
    Targets : Targets
}

module Graph =

    let private createSourcesAndTargets (edges: Edge list) =
        
        let rec loop (edges: Edge list) (sources: Dictionary<_,_>) (targets: Dictionary<_,_>) =
            match edges with
            | edge::remaining ->
                let target = Edge.getTarget edge
                let newNodeSources =
                    match sources.TryGetValue target with
                    | true, sourceNodes -> edge :: sourceNodes
                    | false, _ -> [edge]
                sources[target] <- newNodeSources
                
                let source = Edge.getSource edge
                let newNodeTargets =
                    match targets.TryGetValue source with
                    | true, targetNodes -> edge :: targetNodes
                    | false, _ -> [edge]
                targets[source] <- newNodeTargets
                
                loop remaining sources targets
                
            | [] -> ReadOnlyDictionary sources, ReadOnlyDictionary targets
            
        let sources = Dictionary()
        let targets = Dictionary()
            
        loop edges sources targets
    

    let create (edges: Edge list) =
        let sources, targets = createSourcesAndTargets edges
        {
            Sources = sources
            Targets = targets
        }


let sort (graph: Graph) =
        
    let createInitialEdgeSet (graph: Graph) =
        let edges = HashSet()
        
        for value in graph.Targets.Values do
            for edge in value do
                edges.Add edge |> ignore
                
        edges

        
    let createInitialSourceNodes (graph: Graph) =
        graph.Targets.Keys
        |> Seq.filter (graph.Sources.ContainsKey >> not)
        |> List.ofSeq
    
    
    let processEdge (sources: Sources) (remainingEdges: Edge HashSet, toProcess: Node list) (edge: Edge) =
        remainingEdges.Remove edge |> ignore
        let edgeTarget = Edge.getTarget edge
        
        let noRemainingSources =
            sources[edgeTarget]
            |> List.forall (remainingEdges.Contains >> not)
            
        if noRemainingSources then
            remainingEdges, (edgeTarget :: toProcess)
            
        else
            remainingEdges, toProcess
    
    
    let rec loop (sources: Sources) (targets: Targets) (remainingEdges: Edge HashSet) (toProcess: Node list) (sortedNodes: Node list) =
        match toProcess with
        | nextNode::toProcess ->
            
            match targets.TryGetValue nextNode with
            | true, nodeTargets ->
                let remainingEdges, toProcess =
                    ((remainingEdges, toProcess), nodeTargets)
                    ||> List.fold (processEdge sources)
                loop sources targets remainingEdges toProcess (nextNode :: sortedNodes)
            | false, _ ->
                loop sources targets remainingEdges toProcess (nextNode :: sortedNodes)
                    
        | [] ->
            if remainingEdges.Count > 0 then
                None
            else
                // Items have been stored in reverse order
                Some (List.rev sortedNodes)

    let remainingEdges = createInitialEdgeSet graph
    let sourceNodes = createInitialSourceNodes graph
    loop graph.Sources graph.Targets remainingEdges sourceNodes []
