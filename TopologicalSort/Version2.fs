module TopologicalSort.Version2

(*
Version 2:
Instead of a Graph being a list of Edges, we create Maps
to hold the Source Edges and Target Edges for each Node
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

type Graph = {
    Sources : Sources
    Targets : Targets
}
    
module Graph =

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
        let sources, targets = createSourcesAndTargets edges
        {
            Sources = sources
            Targets = targets
        }


let sort (graph: Graph) =
        
    let createInitialEdgeSet (graph: Graph) =
        let edges =
            graph.Targets.Values
            |> Seq.collect id
            
        (edges, Set.empty)
        ||> Seq.foldBack Set.add
    
    let createInitialSourceNodes (graph: Graph) =
        graph.Targets.Keys
        |> Seq.filter (graph.Sources.ContainsKey >> not)
        |> List.ofSeq
    
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

    let remainingEdges = createInitialEdgeSet graph
    let sourceNodes = createInitialSourceNodes graph
    loop graph.Sources graph.Targets remainingEdges sourceNodes []
