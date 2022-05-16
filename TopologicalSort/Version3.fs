module TopologicalSort.Version3

(*
Version 3:
Instead of storing the Sources and Targets in a Map, we use a ReadOnlyDictionary
for faster lookup
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

type Sources = Dictionary<Node, Edge list>
type Targets = Dictionary<Node, Edge list>

type Graph =
    {
        Nodes : Node list
        Origins : Node list
        Edges : Edge Set
        Sources : ReadOnlyDictionary<Node, Edge list>
        Targets : ReadOnlyDictionary<Node, Edge list>
    }
    
module Graph =
    
    let private getDistinctNodes (links: Edge list) =

        let rec loop (links: Edge list) (nodes: Node Set) =
            match links with
            | link::remainingLinks ->
                let newNodes =
                    nodes
                    |> Set.add link.Source
                    |> Set.add link.Target
                loop remainingLinks newNodes

            | [] -> nodes

        loop links Set.empty


    let private createSourcesAndTargets (edges: Edge list) =
        
        let rec loop (edges: Edge list) (sources: Sources) (targets: Targets) =
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
        let targets = Dictionary ()
        loop edges sources targets

        
    let create (edges: Edge list) =
        let nodes = getDistinctNodes edges
        let sources, targets = createSourcesAndTargets edges
        let originNodes =
            nodes
            |> Set.filter (sources.ContainsKey >> not)
            |> List.ofSeq
        {
            Edges = Set edges
            Nodes = List.ofSeq nodes
            Sources = sources
            Targets = targets
            Origins = originNodes
        }
     
     
[<RequireQualifiedAccess>]
module Topological =

    let sort (graph: Graph) =
            
        let processEdge (graph: Graph) (remainingEdges: Edge HashSet, toProcess: Node list) (edge: Edge) =
            remainingEdges.Remove edge |> ignore
            let noRemainingSources =
                graph.Sources[edge.Target]
                |> List.forall (remainingEdges.Contains >> not)
                
            if noRemainingSources then
                remainingEdges, (edge.Target :: toProcess)
                
            else
                remainingEdges, toProcess
        
        
        let rec loop (graph: Graph) (remainingEdges: Edge HashSet) (toProcess: Node list) (sortedNodes: Node list) =
            match toProcess with
            | nextNode::toProcess ->
                
                match graph.Targets.TryGetValue nextNode with
                | true, nodeTargets ->
                    let remainingEdges, toProcess =
                        ((remainingEdges, toProcess), nodeTargets)
                        ||> List.fold (processEdge graph)
                    loop graph remainingEdges toProcess (nextNode :: sortedNodes)
                | false, _ ->
                    loop graph remainingEdges toProcess (nextNode :: sortedNodes)
                        
            | [] ->
                if remainingEdges.Count > 0 then
                    None
                else
                    // Items have been stored in reverse order
                    Some (List.rev sortedNodes)

        let remainingEdges = HashSet graph.Edges
        loop graph remainingEdges graph.Origins []
