module TopologicalSort.Version10
// Yeah, we're using pointers :)
#nowarn "9"

(*
Version 9:

*)

open System
open System.Collections.Generic
open System.Runtime.CompilerServices
open FSharp.NativeInterop
open Microsoft.CodeAnalysis.CSharp
open Row

     
let inline stackalloc<'a when 'a: unmanaged> (length: int): Span<'a> =
  let p = NativePtr.stackalloc<'a> length |> NativePtr.toVoidPtr
  Span<'a>(p, length)
     
     
[<Struct;IsByRefLike>]
type StackStack<'T>(values: Span<'T>) =
    [<DefaultValue>] val mutable private _count : int
    
    member s.Push v =
        if s._count < values.Length then
            values[s._count] <- v
            s._count <- s._count + 1
        else
            failwith "Exceeded capacity of StackStack"
        
    member s.Pop () =
        if s._count > 0 then
            s._count <- s._count - 1
            values[s._count]
        else
            failwith "Empty StackStack"
            
    member s.Count = s._count
            
    member s.ToArray () =
        let newArray = GC.AllocateUninitializedArray s._count
        for i in 0 .. newArray.Length - 1 do
            newArray[i] <- values[i]
        newArray
        

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
        

type EdgeTracker (nodeCount: int) =
    let bitsRequired = ((nodeCount * nodeCount) + 63) / 64
    let values = Array.create bitsRequired 0UL
    
    // Public for the purposes of inlining
    member b.NodeCount = nodeCount
    member b.Values = values
    
    member inline b.Add (edge: Edge) =
        let source = Edge.getSource edge
        let target = Edge.getTarget edge
        let location = (int source) * b.NodeCount + (int target)
        let bucket = location >>> 6
        let offset = location &&& 0x3F
        let mask = 1UL <<< offset
        b.Values[bucket] <- b.Values[bucket] ||| mask
        
    member inline b.Remove (edge: Edge) =
        let source = Edge.getSource edge
        let target = Edge.getTarget edge
        let location = (int source) * b.NodeCount + (int target)
        let bucket = location >>> 6
        let offset = location &&& 0x3F
        let mask = 1UL <<< offset
        b.Values[bucket] <- b.Values[bucket] &&& ~~~mask

    member inline b.Contains (edge: Edge) =
        let source = Edge.getSource edge
        let target = Edge.getTarget edge
        let location = (int source) * b.NodeCount + (int target)
        let bucket = location >>> 6
        let offset = location &&& 0x3F
        ((b.Values[bucket] >>> offset) &&& 1UL) = 1UL

    member b.Clear () =
        for i = 0 to b.Values.Length - 1 do
            b.Values[i] <- 0UL

    member b.Count =
        let mutable count = 0
        
        for i = 0 to b.Values.Length - 1 do
            count <- count + (System.Numerics.BitOperations.PopCount b.Values[i])

        count

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
    
    [<CompiledName("Iterate")>]
    let inline iter ([<InlineIfLambda>] f: Index -> unit) (range: Range) =
        let mutable i = range.Start
        let bound = range.Start + range.Length

        while i < bound do
            f i
            i <- i + LanguagePrimitives.Int32WithMeasure<Units.Index> 1
            
            
    let inline forall ([<InlineIfLambda>] f: Index -> bool) (range: Range) =
        let mutable result = true
        let mutable i = range.Start
        let bound = range.Start + range.Length

        while i < bound && result do
            result <- f i
            i <- i + LanguagePrimitives.Int32WithMeasure<Units.Index> 1
        
        result
            
    

type SourceRanges = Bar<Units.Node, Range>
type SourceEdges = Bar<Units.Index, Edge>
type TargetRanges = Bar<Units.Node, Range>
type TargetEdges = Bar<Units.Index, Edge>


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
            nodeData._Values
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


let sort (graph: Graph) =
    
    let sourceRanges = graph.SourceRanges
    let sourceEdges = graph.SourceEdges
    let targetRanges = graph.TargetRanges
    let targetEdges = graph.TargetEdges
    
    let result = GC.AllocateUninitializedArray (int sourceRanges.Length)
    let mutable nextToProcessIdx = 0
    let mutable resultCount = 0
    
    let mutable nodeId = 0<Units.Node>
    
    while nodeId < sourceRanges.Length do
        if sourceRanges[nodeId].Length = 0<Units.Index> then
            result[resultCount] <- nodeId
            resultCount <- resultCount + 1
        nodeId <- nodeId + 1<Units.Node>
        
    let remainingEdges = EdgeTracker (int sourceRanges.Length)

    sourceEdges
    |> Bar.iter remainingEdges.Add
    
    while nextToProcessIdx < result.Length && nextToProcessIdx < resultCount do

        let targetRange = targetRanges[result[nextToProcessIdx]]
        let mutable targetIndex = targetRange.Start
        let bound = targetRange.Start + targetRange.Length
        while targetIndex < bound do
            remainingEdges.Remove targetEdges[targetIndex]
            
            // Check if all of the Edges have been removed for this
            // Target Node
            let targetNodeId = Edge.getTarget targetEdges[targetIndex]
            let noRemainingSources =
                sourceRanges[targetNodeId]
                |> Range.forall (fun sourceIndex ->
                    remainingEdges.Contains sourceEdges[sourceIndex]
                    |> not
                    )
                
            if noRemainingSources then
                result[resultCount] <- targetNodeId
                resultCount <- resultCount + 1

            targetIndex <- targetIndex + 1<Units.Index>
        
        nextToProcessIdx <- nextToProcessIdx + 1


    if remainingEdges.Count > 0 then
        None
    else
        Some result
