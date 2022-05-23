module TopologicalSort.Version1

(*
Version 1:
This is a naive first approach to writing Kahn's algorithm using 
functional programming style and immutability
*)

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

type Graph = Graph of Edge list
    
module Graph =
    
    let private getDistinctNodes (Graph edges) =

        let rec loop edges (nodes: Node Set) =
            match edges with
            | edge::remainingEdges ->
                let newNodes =
                    nodes
                    |> Set.add edge.Source
                    |> Set.add edge.Target
                loop remainingEdges newNodes

            | [] -> nodes

        loop edges Set.empty


    let private createSourcesAndTargets (Graph edges) =
        
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
    

    let decompose (graph: Graph) =
        let nodes = getDistinctNodes graph
        let sources, targets = createSourcesAndTargets graph
        let sourceNodes =
            nodes
            |> Set.filter (sources.ContainsKey >> not)
            |> List.ofSeq

        sourceNodes, sources, targets
     

let sort (graph: Graph) =
        
    let processEdge (sources: Sources) (remainingEdges: Edge Set, toProcess: Node list) (edge: Edge) =
        let remainingEdges = remainingEdges.Remove edge
        let noRemainingSources =
            sources[edge.Target]
            |> List.forall (remainingEdges.Contains >> not)
            
        if noRemainingSources then
            remainingEdges, (edge.Target :: toProcess)
            
        else
            remainingEdges, toProcess
    
    
    let rec loop (sources: Sources) (targets: Targets) (remainingEdges: Edge Set) (toProcess: Node list) (sortedNodes: Node list) =
        match toProcess with
        | nextNode::toProcess ->
            
            match targets.TryFind nextNode with
            | Some nodeTargets ->
                let remainingEdges, toProcess =
                    ((remainingEdges, toProcess), nodeTargets)
                    ||> List.fold (processEdge sources)
                loop sources targets remainingEdges toProcess (nextNode :: sortedNodes)
            | None ->
                loop sources targets remainingEdges toProcess (nextNode :: sortedNodes)
                    
        | [] ->
            if remainingEdges.Count > 0 then
                None
            else
                // Items have been stored in reverse order
                Some (List.rev sortedNodes)

    let (Graph edges) = graph
    let remainingEdges = Set edges
    let sourceNodes, sources, targets = Graph.decompose graph
    loop sources targets remainingEdges sourceNodes []
