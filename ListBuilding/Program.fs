open System
open System.Buffers
open System.Collections.Generic
open BenchmarkDotNet.Running
open BenchmarkDotNet.Attributes

[<Struct>]
type Entry =
    {
        I : int
    }

[<MemoryDiagnoser>]
type Benchmarks () =

    [<Benchmark(Baseline = true)>]
    member _.ListComprehension () =
        [ for i in 0..99 do
            { I = i } ]

    // Note: Unrealistic since input size is unknown
    [<Benchmark>]
    member _.AllocateUninitializedArrayToList() =
        let array = GC.AllocateUninitializedArray 100

        for i = 0 to 99 do
            array[i] <- { I = i }

        array |> List.ofArray

    
    [<Benchmark>]
    member _.ManualTracking() =
        // Create a large array. Real-world use would require overflow logic
        let array = GC.AllocateUninitializedArray 100
        let mutable count = 0
        
        for i = 0 to 99 do
            count <- count + 1
            array[i] <- { I = i }

        let mutable acc = []
        
        while count > 0 do
            count <- count - 1
            acc <- array[count] :: acc
            
        acc
        
        
    // Realistic version of AllocateUninitializedArrayToList
    [<Benchmark>]
    member _.ArrayPool() =
        // Rent a large array. Real-world use would require overflow logic
        let array = ArrayPool.Shared.Rent 200
        let mutable count = 0
        
        for i = 0 to 99 do
            count <- count + 1
            array[i] <- { I = i }
    
        let mutable acc = []
        
        while count > 0 do
            count <- count - 1
            acc <- array[count] :: acc
            
        ArrayPool.Shared.Return array
            
        acc


[<EntryPoint>]
let main (args: string[]) =

    let _ = BenchmarkRunner.Run<Benchmarks>()

    1