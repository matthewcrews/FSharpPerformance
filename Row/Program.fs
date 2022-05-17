open System
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running
open Row

[<Measure>] type ChickenId

type Size =
    | ``1`` = 0
    | ``10`` = 1
    | ``100`` = 2
    | ``1_000`` = 3
    | ``10_000`` = 4


[<MemoryDiagnoser>]
type Benchmarks () =
    let rng = Random 123
    let maxValue = 100
    let valueCounts =
        [|
            1
            10
            100
            1_000
            10_000
        |]
    
    let arrays = 
        [|
            [| for _ in 1 .. valueCounts[int Size.``1``] -> rng.Next maxValue |]
            [| for _ in 1 .. valueCounts[int Size.``10``] -> rng.Next maxValue |]
            [| for _ in 1 .. valueCounts[int Size.``100``] -> rng.Next maxValue |]
            [| for _ in 1 .. valueCounts[int Size.``1_000``] -> rng.Next maxValue |]
            [| for _ in 1 .. valueCounts[int Size.``10_000``] -> rng.Next maxValue |]
        |]

    let otherArrays =
        [|
            [| for _ in 1 .. valueCounts[int Size.``1``] -> rng.Next maxValue |]
            [| for _ in 1 .. valueCounts[int Size.``10``] -> rng.Next maxValue |]
            [| for _ in 1 .. valueCounts[int Size.``100``] -> rng.Next maxValue |]
            [| for _ in 1 .. valueCounts[int Size.``1_000``] -> rng.Next maxValue |]
            [| for _ in 1 .. valueCounts[int Size.``10_000``] -> rng.Next maxValue |]
        |]

    let rows =
        arrays
        |> Array.copy
        |> Array.map Row<ChickenId, _>

    let otherRows =
        otherArrays
        |> Array.copy
        |> Array.map Row<ChickenId, _>


//    [<Params(Size.``1``, Size.``10``, Size.``100``, Size.``1_000``, Size.``10_000``)>]
    [<Params(Size.``1``, Size.``10``, Size.``100``)>]
//    [<Params(Size.``1``, Size.``10``)>]
    member val Size = Size.``1`` with get, set
//
//
//    [<Benchmark>]
//    member b.ArraySum () =
//        let a = arrays[int b.Size]
//        a
//        |> Array.sum
//
//    [<Benchmark>]
//    member b.RowSum () =
//        let a = rows[int b.Size]
//        a
//        |> Row.sum
//
//    [<Benchmark>]
//    member b.ArrayIteri () =
//        let a = arrays[int b.Size]
//        a
//        |> Array.iteri (fun i v -> a[i] <- 1 + v)
//
//    [<Benchmark>]
//    member b.RowIteri () =
//        let a = rows[int b.Size]
//        a
//        |> Row.iteri (fun i v -> a[i] <- 1 + v)
//
//    [<Benchmark>]
//    member b.ArrayMin () =
//        let a = arrays[int b.Size]
//        a
//        |> Array.min
//
//    [<Benchmark>]
//    member b.RowMin () =
//        let a = rows[int b.Size]
//        a
//        |> Row.min
//
//    [<Benchmark>]
//    member b.ArrayMinBy () =
//        let a = arrays[int b.Size]
//        a
//        |> Array.minBy (fun v -> v * 2)
//
//    [<Benchmark>]
//    member b.RowMinBy () =
//        let a = rows[int b.Size]
//        a
//        |> Row.minBy (fun v -> v * 2)
//
//    [<Benchmark>]
//    member b.ArrayIter () =
//        let mutable acc = 0
//        let a = arrays[int b.Size]
//        a
//        |> Array.iter (fun v -> acc <- acc + v)
//
//    [<Benchmark>]
//    member b.RowIter () =
//        let mutable acc = 0
//        let a = rows[int b.Size]
//        a
//        |> Row.iter (fun v -> acc <- acc + v)
//
//    [<Benchmark>]
//    member b.ArrayIteri2 () =
//        let a = arrays[int b.Size]
//        let other = otherArrays[int b.Size]
//
//        (a, other)
//        ||> Array.iteri2 (fun i aValue otherValue -> a[i] <- aValue + otherValue)
//
//    [<Benchmark>]
//    member b.RowIteri2 () =
//        let a = rows[int b.Size]
//        let other = otherRows[int b.Size]
//
//        (a, other)
//        ||> Row.iteri2 (fun i aValue otherValue -> a[i] <- aValue + otherValue)
    
    [<Benchmark>]
    member b.ArrayMap () =
        let a = arrays[int b.Size]
        a
        |> Array.map (fun v -> v * 2)

    [<Benchmark>]
    member b.RowMap () =
        let a = rows[int b.Size]
        a
        |> Row.map (fun v -> v * 2)

    [<Benchmark>]
    member b.ArrayMapi () =
        let a = arrays[int b.Size]
        a
        |> Array.mapi (fun i v -> v + (int i))

    [<Benchmark>]
    member b.RowMapi () =
        let a = rows[int b.Size]
        a
        |> Row.mapi (fun i v -> v + (int i))

    [<Benchmark>]
    member b.ArrayMap2 () =
        let a = arrays[int b.Size]
        let other = otherArrays[int b.Size]

        (a, other)
        ||> Array.map2 (fun aValue otherValue -> aValue + otherValue)

    [<Benchmark>]
    member b.RowMap2 () =
        let a = rows[int b.Size]
        let other = otherRows[int b.Size]

        (a, other)
        ||> Row.map2 (fun aValue otherValue -> aValue + otherValue)

    [<Benchmark>]
    member b.ArrayMapi2 () =
        let a = arrays[int b.Size]
        let other = otherArrays[int b.Size]

        (a, other)
        ||> Array.mapi2 (fun i aValue otherValue -> a[i] <- aValue + otherValue)
        
    [<Benchmark>]
    member b.RowMapi2 () =
        let a = rows[int b.Size]
        let other = otherRows[int b.Size]

        (a, other)
        ||> Row.mapi2 (fun i aValue otherValue -> a[i] <- aValue + otherValue)

let _ = BenchmarkRunner.Run<Benchmarks>()
