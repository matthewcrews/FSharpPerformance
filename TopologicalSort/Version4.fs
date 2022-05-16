module TopologicalSort.Version4

(*
Version 4:
The default comparison built into F# can be slow when compared
to the performance of using raw primitives. Instead of using
Records to model our domain, we use primitve types that have
been annotated with Units of Measure
*)

open System.Collections.Generic
open System.Collections.ObjectModel


[<Measure>] type Node
[<Measure>] type Edge

type Sources = Dictionary<int<Node>, int64<Edge> list>
type Targets = Dictionary<int<Node>, int64<Edge> list>

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
        Nodes : int<Node> list
        Origins : int<Node> list
        Edges : int64<Edge> Set
        Sources : ReadOnlyDictionary<int<Node>, int64<Edge> list>
        Targets : ReadOnlyDictionary<int<Node>, int64<Edge> list>
    }
    
module Graph =
    
    let private getDistinctNodes (links: int64<Edge> list) =

        let rec loop (links: int64<Edge> list) (nodes: int<Node> Set) =
            match links with
            | link::remainingLinks ->
                let newNodes =
                    nodes
                    |> Set.add (Edge.getSource link)
                    |> Set.add (Edge.getTarget link)
                loop remainingLinks newNodes

            | [] -> nodes

        loop links Set.empty


    let private createSourcesAndTargets (edges: int64<Edge> list) =
        
        let rec loop (edges: int64<Edge> list) (sources: Sources) (targets: Targets) =
            match edges with
            | edge::remaining ->
                let edgeSource = Edge.getSource edge
                let edgeTarget = Edge.getTarget edge
                
                let newNodeSources =
                    match sources.TryGetValue edgeTarget with
                    | true, sourceNodes -> edge :: sourceNodes
                    | false, _ -> [edge]
                sources[edgeTarget] <- newNodeSources
                
                let newNodeTargets =
                    match targets.TryGetValue edgeSource with
                    | true, targetNodes -> edge :: targetNodes
                    | false, _ -> [edge]
                targets[edgeSource] <- newNodeTargets
                
                loop remaining sources targets
                
            | [] -> ReadOnlyDictionary sources, ReadOnlyDictionary targets
            
        let sources = Dictionary()
        let targets = Dictionary ()
        loop edges sources targets

        
    let create (edges: int64<Edge> list) =
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
            
        let processEdge (graph: Graph) (remainingEdges: int64<Edge> HashSet, toProcess: int<Node> list) (edge: int64<Edge>) =
            remainingEdges.Remove edge |> ignore
            let edgeTarget = Edge.getTarget edge
            
            let noRemainingSources =
                graph.Sources[edgeTarget]
                |> List.forall (remainingEdges.Contains >> not)
                
            if noRemainingSources then
                remainingEdges, (edgeTarget :: toProcess)
                
            else
                remainingEdges, toProcess
        
        
        let rec loop (graph: Graph) (remainingEdges: int64<Edge> HashSet) (toProcess: int<Node> list) (sortedNodes: int<Node> list) =
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
