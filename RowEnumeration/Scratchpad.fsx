#load "Row.fs"


open RowEnumeration

[<Measure>] type Chicken

let x = Row<Chicken,_> [|1 .. 10|]

let test (x: Row<_,_>) =
    let mutable acc = 0
    
    for chickenId, value in x do
        acc <- acc + value
        
        
test x


