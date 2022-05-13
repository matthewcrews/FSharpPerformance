module Version1

type Node =
    {
        Name : string
    }

type Edge =
    {
        Source : Node
        Target : Node
    }

type Sources = Map<Node, Edge list>
type Targets = Map<Node, Edge list>

type Graph =
    {
        Nodes : Node list
        Origins : Node list
        Edges : Edge Set
        Sources : Sources
        Targets : Targets
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
                    match sources.TryFind edge.Target with
                    | Some sourceNodes -> edge :: sourceNodes
                    | None -> [edge]
                let sources = sources.Add (edge.Target, newNodeSources)
                
                let newNodeTargets =
                    match targets.TryFind edge.Source with
                    | Some targetNodes -> edge :: targetNodes
                    | None -> [edge]
                let targets = targets.Add (edge.Source, newNodeTargets)
                
                loop remaining sources targets
                
            | [] -> sources, targets
            
        loop edges Map.empty Map.empty
    
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
            
        let processEdge (graph: Graph) (remainingEdges: Edge Set, toProcess: Node list) (edge: Edge) =
            let remainingEdges = remainingEdges.Remove edge
            let noRemainingSources =
                graph.Sources[edge.Target]
                |> List.forall (remainingEdges.Contains >> not)
                
            if noRemainingSources then
                remainingEdges, (edge.Target :: toProcess)
                
            else
                remainingEdges, toProcess
        
        
        let rec loop (graph: Graph) (remainingEdges: Edge Set) (toProcess: Node list) (sortedNodes: Node list) =
            match toProcess with
            | nextNode::toProcess ->
                
                match graph.Targets.TryFind nextNode with
                | Some nodeTargets ->
                    let remainingEdges, toProcess =
                        ((remainingEdges, toProcess), nodeTargets)
                        ||> List.fold (processEdge graph)
                    loop graph remainingEdges toProcess (nextNode :: sortedNodes)
                | None ->
                    loop graph remainingEdges toProcess (nextNode :: sortedNodes)
                        
            | [] ->
                if remainingEdges.Count > 0 then
                    None
                else
                    // Items have been stored in reverse order
                    Some (List.rev sortedNodes)

        let remainingEdges = Set graph.Edges
        loop graph remainingEdges graph.Origins []
