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
        |]
    
    let arrays = 
        [|
            [| for i in 1 .. valueCounts[int Size.``1``] -> rng.Next maxValue |]
            [| for i in 1 .. valueCounts[int Size.``10``] -> rng.Next maxValue |]
            [| for i in 1 .. valueCounts[int Size.``100``] -> rng.Next maxValue |]
            [| for i in 1 .. valueCounts[int Size.``1_000``] -> rng.Next maxValue |]
        |]

    let rows =
        arrays
        |> Array.map Row<ChickenId, _>


    [<Params(Size.``1``, Size.``10``, Size.``100``, Size.``1_000``)>]
    member val Size = Size.``1`` with get, set

    // [<Benchmark>]
    // member b.ArraySum () =
    //     let a = arrays[int b.Size]
    //     a
    //     |> Array.sum

    // [<Benchmark>]
    // member b.RowSum () =
    //     let r = rows[int b.Size]
    //     r
    //     |> Row.sum

    // [<Benchmark>]
    // member b.ArrayIteri () =
    //     let a = arrays[int b.Size]
    //     a
    //     |> Array.iteri (fun i v -> a[i] <- 1 + v)

    // [<Benchmark>]
    // member b.RowIteri () =
    //     let r = rows[int b.Size]
    //     r
    //     |> Row.iteri (fun i v -> r[i] <- 1 + v)

    // [<Benchmark>]
    // member b.ArrayMin () =
    //     let a = arrays[int b.Size]
    //     a
    //     |> Array.min

    // [<Benchmark>]
    // member b.RowMin () =
    //     let r = rows[int b.Size]
    //     r
    //     |> Row.min

    // [<Benchmark>]
    // member b.ArrayMinBy () =
    //     let a = arrays[int b.Size]
    //     a
    //     |> Array.minBy (fun v -> v * 2)

    // [<Benchmark>]
    // member b.RowMinBy () =
    //     let r = rows[int b.Size]
    //     r
    //     |> Row.minBy (fun v -> v * 2)

    // [<Benchmark>]
    // member b.ArrayIter () =
    //     let mutable acc = 0
    //     let a = arrays[int b.Size]
    //     a
    //     |> Array.iter (fun v -> acc <- acc + v)

    // [<Benchmark>]
    // member b.RowIter () =
    //     let mutable acc = 0
    //     let r = rows[int b.Size]
    //     r
    //     |> Row.iter (fun v -> acc <- acc + v)

    // [<Benchmark>]
    // member b.ArrayMap () =
    //     let a = arrays[int b.Size]
    //     a
    //     |> Array.map (fun v -> v * 2)

    // [<Benchmark>]
    // member b.RowMap () =
    //     let r = rows[int b.Size]
    //     r
    //     |> Row.map (fun v -> v * 2)

    [<Benchmark>]
    member b.ArrayMapi () =
        let a = arrays[int b.Size]
        a
        |> Array.mapi (fun i v -> v + (int i))

    [<Benchmark>]
    member b.RowMapi () =
        let r = rows[int b.Size]
        r
        |> Row.mapi (fun i v -> v + (int i))

// let x = 
//     [|1 .. 10|]
//     |> Row<ChickenId, _>

// x
// |> Row.iteri (fun i v -> x[i] <- v + 1)

// printfn $" %A{x.Values}"

let _ = BenchmarkRunner.Run<Benchmarks>()
