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
        
        let sourceRangesValues, sourceNodesValues = createIndexesAndValues nodeSources
        let targetRangesValues, targetNodesValues = createIndexesAndValues nodeTargets
        
        let inline createNativeBar (b: Bar<'Measure, 'a>) =
            let newArr = GC.AllocateArray(b._values.Length, pinned = true)
            Array.Copy (b._values, newArr, newArr.Length)
            Bar<'Measure, 'a> newArr
            // use newArrPtr = fixed newArr
            // NativeBar (newArrPtr, b.Length)
        
        let sourceRanges = createNativeBar sourceRangesValues
        let sourceNodes = createNativeBar sourceNodesValues
        let targetRanges = createNativeBar targetRangesValues
        let targetNodes = createNativeBar targetNodesValues
        
        {
            SourceRanges = sourceRanges
            SourceNodes = sourceNodes
            TargetRanges = targetRanges
            TargetNodes = targetNodes
        }        


let sort (graph: Graph) =
    
    // let sourceRanges = graph.SourceRanges
    // let targetRanges = graph.TargetRanges
    // let targetNodes = graph.TargetNodes
    
    
    let sourceRanges =
        use sourceRangesPtr = fixed graph.SourceRanges._values
        NativeBar (sourceRangesPtr, graph.SourceRanges.Length)
    
    let targetRanges =
        use targetRangesPtr = fixed graph.TargetRanges._values
        NativeBar (targetRangesPtr, graph.TargetRanges.Length)
    
    let targetNodes =
        use targetNodesPtr = fixed graph.TargetNodes._values
        NativeBar (targetNodesPtr, graph.TargetNodes.Length)
    
    let result = GC.AllocateUninitializedArray (int sourceRanges.Length)
    let mutable nextToProcessIdx = 0
    let mutable resultCount = 0
    
    let sourceCounts = stackalloc<uint> (int targetRanges.Length)
    let mutable nodeId = 0<_>
    
    // This is necessary due to the Span not being capture in a lambda
    while nodeId < sourceRanges.Length do
        sourceCounts[int nodeId] <- uint sourceRanges[nodeId].Length
        result[resultCount] <- nodeId
        
        let inc = retype (sourceCounts[int nodeId] = 0u)
        resultCount <- resultCount + inc
        nodeId <- nodeId + 1<_>

    
    while nextToProcessIdx < resultCount do

        let targetRange = targetRanges[result[nextToProcessIdx]]
        let mutable targetIndex = targetRange.Start
        let bound = targetRange.Start + targetRange.Length
        
        while targetIndex < bound do
            let targetNodeId = targetNodes[targetIndex]
            sourceCounts[int targetNodeId] <- sourceCounts[int targetNodeId] - 1u
            
            let inc = retype (sourceCounts[int targetNodeId] = 0u)
            result[resultCount] <- targetNodeId
            resultCount <- resultCount + inc

            targetIndex <- targetIndex + 1<_>
        
        nextToProcessIdx <- nextToProcessIdx + 1


    if resultCount = result.Length then
        ValueSome result
    else
        ValueNone
