#load "Row.fs"


open RowEnumeration

[<Measure>] type Chicken

let x = Row<Chicken,_> [|1 .. 10|]

let test (x: Row<_,_>) =
    for chickenId, value in x do
        printfn $"Chicken: {chickenId} | Value: {value}"
        
        
test x


