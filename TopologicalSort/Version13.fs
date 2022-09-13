module TopologicalSort.Version13
// This is so what we can use stackalloc without a warning
#nowarn "9"
#nowarn "42"

(*
Version 12:
Branchless
*)

open System
open System.Collections.Generic
open FSharp.NativeInterop
open Collections

     
let inline retype<'T,'U> (x: 'T) : 'U = (# "" x: 'U #)

let inline stackalloc<'a when 'a: unmanaged> (length: int): Span<'a> =
  let p = NativePtr.stackalloc<'a> length |> NativePtr.toVoidPtr
  Span<'a>(p, length)
        

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


[<Struct>]
type Edge =
    {
        Source : Node
        Target : Node
    }
    
module Edge =
    
    let create source target =
        {
            Source = source
            Target = target
        }


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


type TargetRanges = Bar<Units.Node, Range>
type TargetNodes = Bar<Units.Index, Node>
type SourceRanges = Bar<Units.Node, Range>
type SourceNodes = Bar<Units.Index, Node>

[<Struct>]
type Graph = {
    TargetRanges : TargetRanges
    TargetNodes : TargetNodes
    SourceRanges : SourceRanges
    SourceNodes : SourceNodes
}
    
module Graph =
    
    let private getNodeCount (edges: Edge[]) =
        let nodes = HashSet()
        
        for edge in edges do
            nodes.Add edge.Source |> ignore
            nodes.Add edge.Target |> ignore
            
        LanguagePrimitives.Int32WithMeasure<Units.Node> nodes.Count
    
    let private createSourcesAndTargets (nodeCount: int<Units.Node>) (edges: Edge[]) =
        let mutable sourcesAcc = Row.create nodeCount []
        let mutable targetsAcc = Row.create nodeCount []
        
        for edge in edges do
            sourcesAcc[edge.Target] <- edge.Source :: sourcesAcc[edge.Target]
            targetsAcc[edge.Source] <- edge.Target :: targetsAcc[edge.Source]
            
        let finalSources =
            sourcesAcc
            |> Row.map Array.ofList
            
        let finalTargets =
            targetsAcc
            |> Row.map Array.ofList
            
        finalSources.Bar, finalTargets.Bar

        
    let private createIndexesAndValues (nodeData: Bar<'Measure, Node[]>) =
        let mutable ranges = Row.create nodeData.Length Range.Zero
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
            SourceNodes = sourceNodes
            TargetRanges = targetRanges
            TargetNodes = targetNodes
        }        


let sort (graph: Graph) =
    
    use sourceRanges = fixed graph.SourceRanges._values
    use targetRanges = fixed graph.TargetRanges._values
    use targetNodes = fixed graph.TargetNodes._values
    
    let sourceRanges = NativeBar (sourceRanges, graph.SourceRanges.Length)
    let targetRanges = NativeBar (targetRanges, graph.SourceRanges.Length)
    let targetNodes = NativeBar (targetNodes, graph.TargetNodes.Length)
    
    
    let resultArray = GC.AllocateUninitializedArray (int sourceRanges.Length)
    use resultPtr = fixed resultArray
    let mutable result = NativeArray (resultPtr, resultArray.Length)
    let mutable nextToProcessIdx = 0
    let mutable resultCount = 0
    
    let sourceCountData = NativePtr.stackalloc<uint> (int targetRanges.Length)
    let mutable sourceCounts = NativeArray (sourceCountData, int targetRanges.Length)
    let mutable nodeId = 0<_>
    let bound = sourceRanges.Length
    
    // This is necessary due to the Span not being capture in a lambda
    while nodeId < bound do
        sourceCounts[int nodeId] <- uint sourceRanges[nodeId].Length
        result[resultCount] <- nodeId
        resultCount <- resultCount + (retype (sourceCounts[int nodeId] = 0u))
        nodeId <- nodeId + 1<_>

    
    while nextToProcessIdx < resultCount do

        let targetRange = targetRanges[result[nextToProcessIdx]]
        let mutable targetIndex = targetRange.Start
        let bound = targetRange.Start + targetRange.Length
        
        while targetIndex < bound do
            let targetNodeId = targetNodes[targetIndex]
            sourceCounts[int targetNodeId] <- sourceCounts[int targetNodeId] - 1u
            result[resultCount] <- targetNodeId
            resultCount <- resultCount + (retype (sourceCounts[int targetNodeId] = 0u))
            targetIndex <- targetIndex + 1<_>
        
        nextToProcessIdx <- nextToProcessIdx + 1


    if resultCount = resultArray.Length then
        ValueSome resultArray
    else
        ValueNone
