#load "Row.fs"

open Row

[<Measure>] type ChickenId

let chickenCount = 10

let sizes =
    [|0 .. chickenCount - 1|]
    |> Row<ChickenId, _>

let ages =
    Row.create 10<ChickenId> 1

// Compiler error
let chicken0Size = sizes[0]

// Correct Units of Measure
let chicken0Size = sizes[0<ChickenId>]

// Manual Iteration
for i in 0<ChickenId> .. 1<ChickenId> .. sizes.Length - 1<ChickenId> do
    printfn $"{sizes[i]}"

sizes
|> Row.iter (fun size -> printfn $"{size}")

sizes
|> Row.iteri (fun i size -> printfn $"Index: {i} Size: {size}")


let newSizes =
    sizes
    |> Row.map (fun x -> x * 2)

let names =
    sizes
    |> Row.map (fun n -> $"Clucky{n}")

let nameAndSize =
    (names, sizes)
    ||> Row.map2 (fun n s -> $"Name: {n} Size: {s}")


// type Chicken =
//     {
//         Size : float
//         Name : string
//         Age : int
//     }
// let chickens : Chicken[]

type Chickens =
    {
        Size : Row<ChickenId, float>
        Name : Row<ChickenId, string>
        Age : Row<ChickenId, int>
    }