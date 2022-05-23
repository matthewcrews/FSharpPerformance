module TopologicalSort.Version3

(*
Version 3:
Instead of storing the Sources and Targets in a Map, we use a ReadOnlyDictionary
for faster lookup.
We also convert to using HashSet instead of a Set to keep track
of which Edges are still in the Graph.
*)

open System.Collections.Generic
open System.Collections.ObjectModel


type Node =
    {
        Name : string
    }

type Edge =
    {
        Source : Node
        Target : Node
    }

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
                let newNodeSources =
                    match sources.TryGetValue edge.Target with
                    | true, sourceNodes -> edge :: sourceNodes
                    | false, _ -> [edge]
                sources[edge.Target] <- newNodeSources
                
                let newNodeTargets =
                    match targets.TryGetValue edge.Source with
                    | true, targetNodes -> edge :: targetNodes
                    | false, _ -> [edge]
                targets[edge.Source] <- newNodeTargets
                
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
        let noRemainingSources =
            sources[edge.Target]
            |> List.forall (remainingEdges.Contains >> not)
            
        if noRemainingSources then
            remainingEdges, (edge.Target :: toProcess)
            
        else
            remainingEdges, toProcess
    
    
    let rec loop (sources: Sources) (targets: Targets)  (remainingEdges: Edge HashSet) (toProcess: Node list) (sortedNodes: Node list) =
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
