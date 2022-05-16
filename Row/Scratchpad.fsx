#load "Row.fs"

open Row

let a = [|1 .. 10|]
let b = [|1|]

(a, b)
||> Array.map2 (fun aValue bValue -> aValue = bValue)