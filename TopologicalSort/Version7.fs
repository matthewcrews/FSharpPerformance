module TopologicalSort.Version7
// Yeah, we're using pointers :)
#nowarn "9"

(*
Version 7:
We are going to use a StackStack to reduce the amount of memory allocation
and reduce the overhead compared to the .NET Stack and Queue
*)

open System
open System.Collections.Generic
open System.Runtime.CompilerServices
open FSharp.NativeInterop
open Row

     
let inline stackalloc<'a when 'a: unmanaged> (length: int): Span<'a> =
    let p = NativePtr.stackalloc<'a> length |> NativePtr.toVoidPtr
    Span<'a>(p, length)
     
     
[<Struct; IsByRefLike>]
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


module StackStack =

    let inline create length =
        StackStack (stackalloc<_> length)
        

[<RequireQualifiedAccess>]
module private Units =

    [<Measure>] type Node
    [<Measure>] type Edge


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


type Sources = Bar<Units.Node, Edge[]>
type Targets = Bar<Units.Node, Edge[]>

type Graph = {
    Sources : Sources
    Targets : Targets
}
    
module Graph =
    
    let private getNodeCount (edges: Edge[]) =
        let nodes = HashSet()
        
        for edge in edges do
            let source = Edge.getSource edge
            let target = Edge.getTarget edge
            nodes.Add source |> ignore
            nodes.Add target |> ignore
            
        nodes.Count
    
    let private createSourcesAndTargets (nodeCount: int) (edges: Edge[]) =
        let nodeCount = LanguagePrimitives.Int32WithMeasure<Units.Node> nodeCount
        let mutable sourcesAcc = Row.create nodeCount []
        let mutable targetsAcc = Row.create nodeCount []
        
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

        
    let create (edges: Edge[]) =
        let nodeCount = getNodeCount edges
        let sources, targets = createSourcesAndTargets nodeCount edges
        {
            Sources = sources
            Targets = targets
        }


let sort (graph: Graph) =
        
    let sources = graph.Sources
    let targets = graph.Targets
    
    let mutable toProcess = StackStack.create (int graph.Sources.Length)
    let mutable sortedNodes = StackStack.create (int graph.Sources.Length)

    for i in 0 .. (int graph.Sources.Length) - 1 do
        let nodeId = LanguagePrimitives.Int32WithMeasure<Units.Node> i
        let edges = sources[nodeId]
        if edges.Length = 0 then
            toProcess.Push nodeId
        
    let remainingEdges = EdgeTracker (int targets.Length)

    targets
    |> Bar.iter (fun edges ->
        for edge in edges do
            remainingEdges.Add edge)
    
    while toProcess.Count > 0 do
        let nextNode = toProcess.Pop()
        sortedNodes.Push nextNode

        for edge in targets[nextNode] do
            let target = Edge.getTarget edge
            remainingEdges.Remove edge
            
            let noRemainingSources =
                sources[target]
                |> Array.forall (remainingEdges.Contains >> not)
                
            if noRemainingSources then
                toProcess.Push target

    if remainingEdges.Count > 0 then
        None
    else
        Some (sortedNodes.ToArray())
