open System
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running
open System.Collections.Generic

/// An array backed Set to be used with Value types where the
/// number of elements is small. It is meant to have an API
/// that is compatible with the HashSet<'T> collection
type SmallSet<'T when 'T: equality> (newValues: 'T seq) =
    let mutable count = Seq.length newValues
    let mutable values = Array.zeroCreate (count * 2)
    do newValues |> Seq.iteri (fun i v -> values[i] <- v)

    /// Adds an element to the SmallSet and returns a bool
    /// indicating whether the element was added
    member _.Add newValue =
        let mutable exists = false
        let mutable i = 0

        while i < count && (not exists) do
            exists <- (EqualityComparer<'T>.Default.Equals (values[i], newValue))
            i <- i + 1
        
        // We only need to add the element if it does not exist
        if not exists then

            // Check if we have capacity in values to add the value
            if count < values.Length then
                // Add the new value to the end
                values[count] <- newValue
                // Update the number of elements stored in the SmallSet
                count <- count + 1

            else
                // Create a new, larger array to contain the values
                let newValues = Array.zeroCreate (values.Length * 2)
                // Add the values from the previous store to the new one
                values
                |> Array.iteri (fun i v -> newValues[i] <- v)
                // Add the new value to the end
                newValues[values.Length] <- newValue
                // Update the number of elements held by the SmallSet
                count <- values.Length + 1
                // Swap out the internal store
                values <- newValues

        exists

    /// Removes an element from the SmallSet returning a bool
    /// indicating whether the SmallSet contained the element
    member _.Remove value =

        let mutable i = 0
        let mutable isFound = false

        while i < count && (not isFound) do
            // Check if we have found the value of interest
            if EqualityComparer<'T>.Default.Equals(values[i], value) then
                // We overwrite the removed value with the last value
                // in the SmallSet
                values[i] <- values[count - 1]
                // We decrement the count to indicate the new number of
                // elements in the small set
                count <- count - 1
                // We update the isFound flag to break out of the loop
                isFound <- true

            i <- i + 1

        isFound

    /// Returns an int indicating the number of elements in the SmallSet
    member _.Count = count

    /// Retrieve an item by index from the SmallSet
    member _.Item
        with get k =
            if k > count - 1 then
                raise (IndexOutOfRangeException ())
            else
                values[k]

    // A simple way to iterate through the values in the SmallSet.
    member _.Values () =
        seq { for i in 0 .. count -> values[i] }


[<MemoryDiagnoser>]
type Benchmarks () =
    let rng = Random 123
    let valueCount = 100
    let maxValue = 1_000_000_000L
    let values = [|for _ in 1 .. valueCount -> rng.NextInt64 maxValue |]
    let removeCount = 10
    let addCount = 10
    let removeValues =
        [|for _ in 1 .. removeCount -> 
            values[rng.Next values.Length]
        |]
    let addValues = 
        [|for _ in 1 .. addCount ->
            rng.NextInt64 maxValue
        |]

    let hashSet = HashSet values
    let smallSet = SmallSet values
    let outArray = Array.zeroCreate (hashSet.Count * 2)
    [<Benchmark>]
    member _.HashSetAdd () =
        let mutable result = false

        for elem in addValues do
            result <- hashSet.Add elem
        
        result

    [<Benchmark>]
    member _.SmallSetAdd () =
        let mutable result = false

        for elem in addValues do
            result <- smallSet.Add elem

        result

    [<Benchmark>]
    member _.HashSetRemove () =
        let mutable result = false

        for elem in removeValues do
            result <- hashSet.Remove elem
        
        result

    [<Benchmark>]
    member _.SmallSetRemove () =
        let mutable result = false

        for elem in removeValues do
            result <- smallSet.Remove elem

        result
        
    [<Benchmark>]
    member _.HashSetSum () =
        let mutable acc = 0L
        
        for elem in hashSet do
            acc <- acc + elem
            
        acc
        
    [<Benchmark>]
    member _.HashSetSumOfArray () =
        hashSet.CopyTo outArray
        let mutable acc = 0L
        let mutable i = 0
        while i < hashSet.Count do
            acc <- acc + outArray[i]
            i <- i + 1
            
        acc
        
    [<Benchmark>]
    member _.SmallSetSum () =
        let mutable acc = 0L
        let mutable i = 0
        
        while i < smallSet.Count do
            acc <- acc + smallSet[i]
            i <- i + 1
        
        acc


let args = Environment.GetCommandLineArgs()[1..]

match args[0] with
| "profile" ->

    let b = Benchmarks()
    let mutable result = false
    printfn "Starting"
    for _ in 1 .. 1_000_000_000 do
            result <- b.SmallSetAdd()

    printfn $"{result}"

| "benchmark" ->

    let _ = BenchmarkRunner.Run<Benchmarks>()
    ()

| unknownCommand -> failwith $"Unknown command: {unknownCommand}"
