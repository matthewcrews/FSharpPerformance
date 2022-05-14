module TopologicalSort.Version7

open System.Collections.Generic
open TopologicalSort.Row
open System
open System.Numerics

//| Method |     Mean |   Error |  StdDev | BranchInstructions/Op | BranchMispredictions/Op |  Gen 0 | Allocated |
//|------- |---------:|--------:|--------:|----------------------:|------------------------:|-------:|----------:|
//|     V6 | 283.0 ns | 0.69 ns | 0.65 ns |                   692 |                       0 | 0.0086 |      72 B |
//|     V7 | 278.1 ns | 1.63 ns | 1.36 ns |                   692 |                       0 | 0.0086 |      72 B |

[<Measure>] type Node
[<Measure>] type Edge

type Array with    
    static member inline SIMDFold f h (start:'T) (values : 'T[]) =        
        let mutable i = 0;
        let mutable v = Vector<'T>(start)
        while i < values.Length - Vector<'T>.Count do            
            v <- f v (Vector<'T>(values,i))
            i <- i + Vector<'T>.Count
        i <- 0
        let mutable result = start        
        while i < Vector<'T>.Count do
            result <- h result v.[i]
            i <- i+1
        result
        
    /// <summary>
    /// Identical to the standard map function, but you must provide
    /// A Vector mapping function.
    /// </summary>
    /// <param name="vf">A function that takes a Vector and returns a Vector. The returned vector
    /// does not have to be the same type but must be the same width</param>
    /// <param name="sf">A function to handle the leftover scalar elements if array is not divisible by Vector.count</param>
    /// <param name="array">The source array</param>
    static member inline SIMDmap
        (vf : ^T Vector -> ^U Vector) (sf : ^T -> ^U) (array : ^T[]) : ^U[] =
        let count = Vector< ^T>.Count
        if count <> Vector< ^U>.Count then invalidArg "array" "Output type must have the same width as input type."    
        
        let result = Array.zeroCreate array.Length
        
        let mutable i = 0
        while i <= array.Length-count do        
            (vf (Vector< ^T>(array,i ))).CopyTo(result,i)   
            i <- i + count
        
        i <- array.Length-array.Length%count
        while i < result.Length do
            result.[i] <- sf array.[i]
            i <- i + 1

        result

    /// <summary>
    /// Identical to the standard contains, just faster
    /// </summary>
    /// <param name="x"></param>
    /// <param name="array"></param>
    static member inline SIMDcontains (x : ^T) (array:^T[]) : bool =
        
        let count = Vector< ^T>.Count      
        let len = array.Length    
        let compareVector = Vector< ^T>(x)    
        
        let mutable found = false
        let mutable i = 0
        while i <= len-count do
            found <- Vector.EqualsAny(Vector< ^T>(array,i),compareVector)
            if found then i <- len
            else i <- i + count

        i <- len-len%count
        while i < array.Length && not found do                
            found <- x = array.[i]
            i <- i + 1

        found
        
    /// <summary>
    /// Checks if all Vectors satisfy the predicate.
    /// </summary>
    /// <param name="f">Takes a Vector and returns true or false</param>
    /// <param name="array"></param>
    static member inline SIMDforall 
        (vf : ^T Vector -> bool) 
        (sf : ^T -> bool)
        (array: ^T[]) : bool =
       
        let count = Vector< ^T>.Count
        let mutable found = true
        let len = array.Length
        
        let mutable i = 0
        while i <= len-count do
            found <- vf (Vector< ^T>(array,i))
            if not found then i <- len
            else i <- i + count

        i <- len-len%count
        while i < array.Length && found do
            found <- sf array.[i]
            i <- i + 1

        found
        
module Edge =

    let create (source: int<Node>) (target: int<Node>) =
        (((int64 source) <<< 32) ||| (int64 target))
        |> LanguagePrimitives.Int64WithMeasure<Edge>
        
    let inline getSource (edge: int64<Edge>) =
        ((int64 edge) >>> 32)
        |> int
        |> LanguagePrimitives.Int32WithMeasure<Node>

    let inline getTarget (edge: int64<Edge>) =
        int edge
        |> LanguagePrimitives.Int32WithMeasure<Node>


type Graph =
    {
        Nodes : int<Node> array
        Origins : int<Node> array
        Edges : int64<Edge> array
        Sources : ReadOnlyRow<Node, int64<Edge> array>
        Targets : ReadOnlyRow<Node, int64<Edge> array>
    }
    
module Graph =
    
    let private getDistinctNodes (edges: int64<Edge> array) =

        let distinctNodes = HashSet()
        
        for edge in edges do
            let source = Edge.getSource edge
            let target = Edge.getTarget edge
            distinctNodes.Add source |> ignore
            distinctNodes.Add target |> ignore
        
        Array.ofSeq distinctNodes

    
    let private createSourcesAndTargets (nodeCount: int) (edges: int64<Edge> array) =
        let nodeCount = LanguagePrimitives.Int32WithMeasure<Node> nodeCount
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
            |> ReadOnlyRow
            
        let finalTargets =
            targetsAcc
            |> Row.map Array.ofList
            |> ReadOnlyRow
            
        finalSources, finalTargets

        
    let create (edges: int64<Edge> array) =
        let edges = Array.distinct edges
        let nodes = getDistinctNodes edges
        let sources, targets = createSourcesAndTargets nodes.Length edges
        let originNodes =
            nodes
            |> Array.filter (fun node -> sources[node].Length = 0)
        {
            Edges = edges
            Nodes = nodes
            Sources = sources
            Targets = targets
            Origins = originNodes
        }
     
     
[<RequireQualifiedAccess>]
module Topological =
    open System.Linq
    let  private toProcess = Stack<int<Node>> (16)
    let  private sortedNodes = Queue<int<Node>> (16)
    let  private remainingEdges = HashSet<int64<Edge>> (16)
    
    let inline private targetFn graph edge =
        let target = Edge.getTarget edge
        remainingEdges.Remove edge |> ignore
//        let remEdges = Vector<int64<Edge>>(remainingEdges.ToArray().AsSpan())
//        Array.SIMDcontains 
//        let remEdgesContains x = Vector<int64<Edge>>(remainingEdges.ToArray()) |> Array.SIMDcontains x
        
        let noRemainingSources =
            graph.Sources[target]
            |> Array.forall (fun x -> Array.SIMDcontains x (remainingEdges.ToArray()))
            
        if noRemainingSources then
            toProcess.Push target
        
//        
//        let remSrc = Vector<int64<Edge>>(graph.Sources[target].AsSpan())
////        Array.SIMDforall 
//        let noRemainingSources =
//            graph.Sources[target]
//            |> Array.forall (remainingEdges.Contains >> not)
//            
//        if noRemainingSources then
//            toProcess.Push target
    let sort (graph: Graph) =
            
        toProcess.Clear()
        sortedNodes.Clear()
        remainingEdges.Clear ()
        
        for node in graph.Origins do
            toProcess.Push node    
    
        for edge in graph.Edges do
            remainingEdges.Add edge |> ignore
        
                
        while toProcess.Count > 0 do
            let nextNode = toProcess.Pop()
            sortedNodes.Enqueue nextNode
            
            graph.Targets[nextNode]
            |> Array.iter (fun edge -> targetFn graph edge)
            
//            graph.Targets[nextNode]
//            |> Array.iter (fun edge ->
//                let target = Edge.getTarget edge
//                remainingEdges.Remove edge |> ignore
//                
//                let noRemainingSources =
//                    graph.Sources[target]
//                    |> Array.forall (remainingEdges.Contains >> not)
//                    
//                if noRemainingSources then
//                    toProcess.Push target
//            
//            )

        if remainingEdges.Count > 0 then
            None
        else
            Some (sortedNodes.ToArray())