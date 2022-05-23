module TopologicalSort.Version2

(*
Version 2:
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


type Graph = Graph of Edge list

module Graph =
    
    let private getDistinctNodes (Graph edges) =

        let distinctNodes = HashSet()
        
        for edge in edges do
            distinctNodes.Add edge.Source |> ignore
            distinctNodes.Add edge.Target |> ignore
            
        distinctNodes    

    let private createSourcesAndTargets (Graph edges) =
        
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
    

    let decompose (graph: Graph) =
        let nodes = getDistinctNodes graph
        let sources, targets = createSourcesAndTargets graph
        let sourceNodes =
            nodes
            |> Seq.filter (sources.ContainsKey >> not)
            |> List.ofSeq

        sourceNodes, sources, targets


type Sources = ReadOnlyDictionary<Node, Edge list>
type Targets = ReadOnlyDictionary<Node, Edge list>


let sort (graph: Graph) =
        
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

    let (Graph edges) = graph
    let remainingEdges = HashSet edges
    let sourceNodes, sources, targets = Graph.decompose graph
    loop sources targets remainingEdges sourceNodes []
