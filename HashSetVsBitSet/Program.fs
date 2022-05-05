open System
open System.Collections
open System.Collections.Generic
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running

[<Measure>] type JobId
[<Measure>] type MachineId
[<Measure>] type OperationId



type BitSetTracker (jobCount, machineCount, operationCount: int) =
    let uint64sRequired = ((jobCount * machineCount * operationCount) + 63) / 64
    let values : uint64 array = Array.zeroCreate uint64sRequired
    
    member internal _.JobCount = jobCount
    member internal _.MachineCount = machineCount
    member internal _.OperationCount = operationCount
    member internal _.Values = values
    
    member _.Item
        with get (jobId: int<JobId>, machineId: int<MachineId>, operationId: int<OperationId>) =
            if (int jobId) >= jobCount then
                raise (IndexOutOfRangeException (nameof jobId))
            if (int machineId) >= machineCount then
                raise (IndexOutOfRangeException (nameof machineId))
            if (int operationId) >= operationCount then
                raise (IndexOutOfRangeException (nameof operationId))
                
            
            let location = (int jobId) * (machineCount * operationCount) + (int machineId) * operationCount + (int operationId)
            // The int64 we will need to lookup
            let bucket = location / 64
            // The bit in the int64 we want to return
            let offset = location - (bucket * 64)
            // Mask to check with
            let mask = 1UL <<< offset
            // Return whether the bit at the offset is set to 1 or not
            values[bucket] &&& mask <> 0UL
            
        and set (jobId: int<JobId>, machineId: int<MachineId>, operationId: int<OperationId>) value =
            if (int jobId) >= jobCount then
                raise (IndexOutOfRangeException (nameof jobId))
            if (int machineId) >= machineCount then
                raise (IndexOutOfRangeException (nameof machineId))
            if (int operationId) >= operationCount then
                raise (IndexOutOfRangeException (nameof operationId))
            
            let location = (int jobId) * (machineCount * operationCount) + (int machineId) * operationCount + (int operationId)
            // The int64 we will need to lookup
            let bucket = location / 64
            // The bit in the int64 we want to update
            let offset = location - (bucket * 64)
            // Get the int representation of the value
            let value = if value then 1UL else 0UL
            // Set the bit in the bucket to the desired value
            values[bucket] <- (values[bucket] &&& ~~~(1UL <<< offset)) ||| (value <<< offset)
    
            
module BitSetTracker =
    
    let map (f: int<JobId> -> int<MachineId> -> int<OperationId> -> 'Result) (b: BitSetTracker) =
        let acc = Stack<'Result> ()
        let mutable i = 0
        let length = b.Values.Length

        while i < length do
            let mutable bitSet = b.Values[i]
            while bitSet <> 0UL do
                let r = System.Numerics.BitOperations.TrailingZeroCount bitSet
                let location = i * 64 + r
                let jobId =
                    location / (b.MachineCount * b.OperationCount)
                    |> LanguagePrimitives.Int32WithMeasure<JobId>
                let machineId =
                    (location - (int jobId) * (b.MachineCount * b.OperationCount)) / b.OperationCount
                    |> LanguagePrimitives.Int32WithMeasure<MachineId>
                let operationId =
                    location - (int jobId) * (b.MachineCount * b.OperationCount) - (int machineId) * b.OperationCount
                    |> LanguagePrimitives.Int32WithMeasure<OperationId>
                
                let result = f jobId machineId operationId
                acc.Push result
                
                bitSet <- bitSet ^^^ (1UL <<< r)
                
            i <- i + 1
                
        acc.ToArray()
        
            
        
let t = BitSetTracker (100, 4, 4)
let jobId = 30<JobId>
let machineId = 3<MachineId>
let operationId = 2<OperationId>
t[jobId, machineId, operationId] <- true
t[jobId, machineId, operationId + 1<OperationId>] <- true
t[jobId, machineId, operationId] <- false
let r = BitSetTracker.map (fun a b c -> $"Job{a}, Machine{b}, Operation{c}") t
            
printfn $"{r}"
            
// For more information see https://aka.ms/fsharp-console-apps
printfn "Hello from F#"
