open System
open Argu
open BenchmarkDotNet.Attributes

type RefChicken =
    {
        Age : int
    }
    
    member c.Add


type Benchmarks () =
    
    
    [<Benchmark>]
    member _.InstanceMethod () =
        
        


[<EntryPoint>]
let main (args: string[]) =

    1