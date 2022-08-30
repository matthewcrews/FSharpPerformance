module TopologicalSort.Version10

open System
open System.Collections.Generic
open System.Numerics
open System.Runtime.InteropServices
open Row

[<RequireQualifiedAccess>]
module private Units =

    [<Measure>] type Node
    [<Measure>] type Edge
    [<Measure>] type Index


type Index = int<Units.Index>

module Index =
        
    let inline create (i: int) =
        if i < 0 then
            invalidArg (nameof i) "Cannot have an Index less than 0"
            
        LanguagePrimitives.Int32WithMeasure<Units.Index> i


type Node = int<Units.Node>

module Node =
    
    let inline create (i: int) =
        if i < 0 then
            invalidArg (nameof i) "Cannot have a Node less than 0"
            
        LanguagePrimitives.Int32WithMeasure<Units.Node> i


type Edge = int64<Units.Edge>

module Edge =

    let inline create (source: Node) (target: Node) =
        (((int64 source) <<< 32) ||| (int64 target))
        |> LanguagePrimitives.Int64WithMeasure<Units.Edge>
        
    let inline getSource (edge: Edge) =
        ((int64 edge) >>> 32)
        |> int
        |> LanguagePrimitives.Int32WithMeasure<Units.Node>

    let inline getTarget (edge: Edge) =
        int edge
        |> LanguagePrimitives.Int32WithMeasure<Units.Node>
    
        
[<Struct>]
type Range =
    {
        Start : Index
        Length : Index
    }
    static member Zero =
        {
            Start = Index.create 0
            Length = Index.create 0
        }
    
module Range =
    
    let create start length =
        {
            Start = start
            Length = length
        }
    

type SourceRanges = Bar<Units.Node, Range>
type SourceEdges = Bar<Units.Index, Edge>
type TargetRanges = Bar<Units.Node, Range>
type TargetEdges = Bar<Units.Index, Edge>

[<Struct>]
type Graph = {
    SourceRanges : SourceRanges
    SourceEdges : SourceEdges
    TargetRanges : TargetRanges
    TargetEdges : TargetEdges
}
    
module Graph =
    
    let private getNodeCount (edges: Edge[]) =
        let nodes = HashSet()
        
        for edge in edges do
            let source = Edge.getSource edge
            let target = Edge.getTarget edge
            nodes.Add source |> ignore
            nodes.Add target |> ignore
            
        LanguagePrimitives.Int32WithMeasure<Units.Node> nodes.Count
    
    let private createSourcesAndTargets (nodeCount: int<Units.Node>) (edges: Edge[]) =
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

        
    let private createIndexesAndValues (nodeData: Bar<'Measure, Edge[]>) =
        let ranges = Row.create nodeData.Length Range.Zero
        let mutable nextStartIndex = Index.create 0
        
        nodeData
        |> Bar.iteri (fun nodeId nodes ->
            let length =
                nodes.Length
                |> int
                |> Index.create
            let newRange = Range.create nextStartIndex length
            ranges[nodeId] <- newRange
            nextStartIndex <- nextStartIndex + length
            )
        
        let values =
            nodeData._values
            |> Array.concat
            |> Bar<Units.Index, _>
        
        ranges.Bar, values
        
        
    let create (edges: Edge[]) =
        let nodeCount = getNodeCount edges
        let nodeSources, nodeTargets = createSourcesAndTargets nodeCount edges
        
        let sourceRanges, sourceNodes = createIndexesAndValues nodeSources
        let targetRanges, targetNodes = createIndexesAndValues nodeTargets
        
        {
            SourceRanges = sourceRanges
            SourceEdges = sourceNodes
            TargetRanges = targetRanges
            TargetEdges = targetNodes
        }        

    
    type GraphType =
        static member inline AddToTracker(tracker: Span<uint64>,nodeCount:int, edge: Edge) =
            let source = Edge.getSource edge
            let target = Edge.getTarget edge
            let location = (int source) * nodeCount + (int target)
            
            let bucket = location >>> 6
            let offset = location &&& 0x3F
            let mask = 1UL <<< offset
            tracker[bucket] <- tracker[bucket] ||| mask
        
        static member inline RemoveFromTracker(tracker: Span<uint64>, nodeCount: int, edge: Edge) =
            let source = Edge.getSource edge
            let target = Edge.getTarget edge
            let location = (int source) * nodeCount + (int target)
            let bucket = location >>> 6
            let offset = location &&& 0x3F
            let mask = 1UL <<< offset
            tracker[bucket] <- tracker[bucket] &&& ~~~mask
            target
        
        static member inline TrackerNotContains(tracker: Span<uint64>,nodeCount: int, edge: Edge) =
            let source = Edge.getSource edge
            let target = Edge.getTarget edge
            let location = (int source) * nodeCount + (int target)
            let bucket = location >>> 6
            let offset = location &&& 0x3F
            ((tracker[bucket] >>> offset) &&& 1UL) <> 1UL
        
        static member inline TrackerCount(tracker: Span<uint64>) =
            let mutable count = 0
            for i = 0 to tracker.Length - 1 do            
                count <- count + (BitOperations.PopCount tracker[i])
            count
            
            
        static member Sort(graph: inref<Graph>) =
            let sourceRanges = graph.SourceRanges
            let sourceEdges = graph.SourceEdges
            let targetRanges = graph.TargetRanges
            let targetEdges = graph.TargetEdges
            let sourceRangeLength = int sourceRanges.Length
            let result = GC.AllocateUninitializedArray sourceRangeLength
            let mutable nextToProcessIdx = 0
            let mutable resultCount = 0
            
            let mutable nodeId = 0<Units.Node>
            
            while nodeId < sourceRanges.Length do
                if sourceRanges[nodeId].Length = 0<Units.Index> then
                    result[resultCount] <- nodeId
                    resultCount <- resultCount + 1
                nodeId <- nodeId + 1<Units.Node>
            
            let bitsRequired = ((sourceRangeLength * sourceRangeLength) + 63) / 64
            let remainingEdges = (GC.AllocateUninitializedArray bitsRequired)
            let remainingEdgesSpan = remainingEdges.AsSpan()
            for edge in sourceEdges._values do
                GraphType.AddToTracker(remainingEdgesSpan, sourceRangeLength, edge)
            
            while nextToProcessIdx < result.Length && nextToProcessIdx < resultCount do

                let targetRange = targetRanges[result[nextToProcessIdx]]
                let mutable targetIndex = targetRange.Start
                let bound = targetRange.Start + targetRange.Length
                while targetIndex < bound do
                    // Check if all of the Edges have been removed for this
                    // Target Node
                    let targetNodeId = GraphType.RemoveFromTracker(remainingEdgesSpan, sourceRangeLength, targetEdges[targetIndex])
                    
                    let mutable noRemainingSourcesResult = true
                    let range = sourceRanges[targetNodeId]
                    let mutable sourceIndex = range.Start
                    let bound = range.Start + range.Length

                    while sourceIndex < bound && noRemainingSourcesResult do
                        noRemainingSourcesResult <- GraphType.TrackerNotContains(remainingEdgesSpan, sourceRangeLength, sourceEdges[sourceIndex])
                        sourceIndex <- sourceIndex + 1<_>
                                            
                    if noRemainingSourcesResult then
                        result[resultCount] <- targetNodeId
                        resultCount <- resultCount + 1

                    targetIndex <- targetIndex + 1<_>
                
                nextToProcessIdx <- nextToProcessIdx + 1


            if GraphType.TrackerCount(remainingEdges) > 0 then
                ValueNone
            else
                ValueSome result
