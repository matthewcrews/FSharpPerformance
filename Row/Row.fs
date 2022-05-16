module rec Row

open System.Collections
open System.Collections.Generic

module private Helpers =

    let checkInputSeq (values: (int<'Measure> * 'T) seq) =
        let sortedValues =
            values
            |> Seq.sortBy fst

        let firstKey = LanguagePrimitives.Int32WithMeasure<'Measure> 0
        (firstKey, sortedValues)
        ||> Seq.fold (fun key (nextKey, _) ->
            if key <> nextKey then
                invalidArg (nameof values) "Cannot create Row with non-contiguous keys"
            else
                key + LanguagePrimitives.Int32WithMeasure<'Measure> 1
            )
        |> ignore
        
        let newValues =
            sortedValues
            |> Seq.map snd
            |> Array.ofSeq
        
        newValues


    let inline invalidArgDifferentRowLength rowAName lengthA rowBName lengthB =
        $"{rowAName}.Length = {lengthA}, {rowBName}.Length = {lengthB}"


type Row<[<Measure>] 'Measure, 'T>(values: 'T[]) =
    
    do if isNull values then
        raise (System.ArgumentNullException(nameof values)) 


    new (length: int<'Measure>, value: 'T) =
        Row (Array.create (int length) value)


    new (other: Row<'Measure, 'T>) =
        let newValues = other.Values
        Row<'Measure, _> newValues
        
        
    new (values: (int<'Measure> * 'T) seq) =
        let newValues = Helpers.checkInputSeq values
        Row<'Measure, 'T> newValues
        
    // Must be public to support inline
    member _.Values : 'T[] = values


    member r.Item
        with inline get (i: int<'Measure>) =
            r.Values[int i]

        and inline set (index: int<'Measure>) value =
            r.Values[int index] <- value


    member inline r.Length = LanguagePrimitives.Int32WithMeasure<'Measure> r.Values.Length


    member _.AsReadOnlyRow () =
        ReadOnlyRow<'Measure, _> values


    override row.ToString () =
        $"Row %A{row.Values}"


    interface IEnumerable<KeyValuePair<int<'Measure>, 'T>> with
            member r.GetEnumerator () : IEnumerator<KeyValuePair<int<'Measure>, 'T>> =
                let values = values
                let x =
                    0
                    |> Seq.unfold (fun i ->
                        if i < values.Length then
                            let index = LanguagePrimitives.Int32WithMeasure<'Measure> i
                            let next = KeyValuePair (index, values[i])
                            Some (next, i + 1)
                        else
                            None )

                x.GetEnumerator ()

            member r.GetEnumerator () : IEnumerator =
                (r :> IEnumerable<_>).GetEnumerator() :> IEnumerator


module Row =

    [<CompiledName("Create")>]
    let inline create (count: int<'Measure>) value =
        let values = Array.create (int count) value
        Row<'Measure, _> values
    

    [<CompiledName("Sum")>]
    let inline sum (row: Row<'Measure, 'T>) =
        let mutable acc = LanguagePrimitives.GenericZero<'T>
        let array = row.Values
        for i = 0 to array.Length - 1 do
            acc <- acc + array[i]

        acc


    [<CompiledName("SumBy")>]
    let inline sumBy ([<InlineIfLambda>] f) (row: Row<'Measure, 'T>) =
        let mutable acc = LanguagePrimitives.GenericZero<'T>
        let array = row.Values
        for i = 0 to array.Length - 1 do
            acc <- acc + (f array[i])

        acc
    

    [<CompiledName("Iterate")>]
    let inline iter ([<InlineIfLambda>] f) (row: Row<_,_>) =
        let array = row.Values
        for i = 0 to array.Length - 1 do
            f array[i]
       
       
    [<CompiledName("Iterate2")>]
    let inline iter2 ([<InlineIfLambda>] f: 'a -> 'b -> unit) (a: Row<'Measure, 'a>) (b: Row<'Measure, 'b>) =
        let array1 = a.Values
        let array2 = b.Values
        if array1.Length <> array2.Length then
            raise (invalidArg (nameof a) "Cannot iterate through arrays of different lengths")

        for i = 0 to array1.Length - 1 do
            f array1[i] array2[i]
  
            
    [<CompiledName("IterateIndexed")>]
    let inline iteri ([<InlineIfLambda>] f: int<'Measure> -> 'a -> unit) (row: Row<'Measure, _>) =
        let array = row.Values
        for i = 0 to array.Length - 1 do
            let i = LanguagePrimitives.Int32WithMeasure<'Measure> i
            f i array[int i]

    
    [<CompiledName("IterateIndexed2")>]
    let inline iteri2 ([<InlineIfLambda>] f: int<'Measure> -> 'a -> 'b -> unit) (a: Row<'Measure, 'a>) (b: Row<'Measure, 'b>) =
        let array1 = a.Values
        let array2 = b.Values
        if array1.Length <> array2.Length then
            raise (invalidArg (nameof a) "Cannot iterate through arrays of different lengths")

        for i = 0 to array1.Length - 1 do
            let i = LanguagePrimitives.Int32WithMeasure<'Measure> i
            f i array1[int i] array2[int i]


    [<CompiledName("Map")>]
    let inline map ([<InlineIfLambda>] f) (row: Row<'Measure, _>) =
        let array = row.Values
        let res = Array.zeroCreate array.Length

        for i = 0 to array.Length - 1 do
            res[i] <- f array[i]
        
        Row<'Measure, _> res


    [<CompiledName("Map2")>]
    let inline map2 ([<InlineIfLambda>] f: 'a -> 'b -> 'c) (a: Row<'Measure, 'a>) (b: Row<'Measure, 'b>) =
        let array1 = a.Values
        let array2 = b.Values
        if array1.Length <> array2.Length then
            let msg = Helpers.invalidArgDifferentRowLength (nameof a) a.Length (nameof b) b.Length
            raise (invalidArg (nameof a) msg)

        let res = Array.zeroCreate array1.Length

        for i = 0 to array1.Length - 1 do
            res[i] <- f array1[i] array2[i]

        Row<'Measure, _> res

            
    [<CompiledName("MapIndexed")>]
    let inline mapi ([<InlineIfLambda>] f: int<'Measure> -> 'a -> 'b) (row: Row<'Measure, _>) =
        let array = row.Values
        let res = Array.zeroCreate array.Length

        for i = 0 to array.Length - 1 do
            let i = LanguagePrimitives.Int32WithMeasure<'Measure> i
            res[int i] <- f i array[int i]
        
        Row<'Measure, _> res


    [<CompiledName("MapIndexed2")>]
    let inline mapi2 ([<InlineIfLambda>] f: int<'Measure> -> 'a -> 'b -> 'c) (a: Row<'Measure, 'a>) (b: Row<'Measure, 'b>) =
        let array1 = a.Values
        let array2 = b.Values
        if array1.Length <> array2.Length then
            let msg = Helpers.invalidArgDifferentRowLength (nameof a) a.Length (nameof b) b.Length
            raise (invalidArg (nameof a) msg)

        let res = Array.zeroCreate array1.Length

        for i = 0 to array1.Length - 1 do
            let i = LanguagePrimitives.Int32WithMeasure<'Measure> i
            res[int i] <- f i array1[int i] array2[int i]

        Row<'Measure, _> res

        
    [<CompiledName("Max")>]
    let inline max (row: Row<'Measure, _>) =
        let array = row.Values
        if array.Length = 0 then invalidArg (nameof row) "Row cannot be empty"
        let mutable acc = array[0]
        for i = 1 to array.Length - 1 do
            let curr = array[i]
            if curr > acc then 
                acc <- curr
        acc
        

    [<CompiledName("MayBy")>]
    let inline maxBy ([<InlineIfLambda>] f) (row: Row<'Measure, _>) =
        let array = row.Values
        if array.Length = 0 then invalidArg (nameof row) "Row cannot be empty"
        let mutable accv = array[0]
        let mutable acc = f accv
        for i = 1 to array.Length - 1 do
            let currv = array[i]
            let curr = f currv
            if curr > acc then
                acc <- curr
                accv <- currv
        accv
        

    [<CompiledName("Min")>]
    let inline min (row: Row<'Measure, _>) =
        let array = row.Values
        if array.Length = 0 then invalidArg (nameof row) "Row cannot be empty"
        let mutable acc = array[0]
        for i = 1 to array.Length - 1 do
            let curr = array[i]
            if curr < acc then 
                acc <- curr
        acc
        

    [<CompiledName("MinBy")>]
    let inline minBy ([<InlineIfLambda>] f) (row: Row<'Measure, _>) =
        let array = row.Values
        if array.Length = 0 then invalidArg (nameof row) "Row cannot be empty"
        let mutable accv = array[0]
        let mutable acc = f accv
        for i = 1 to array.Length - 1 do
            let currv = array[i]
            let curr = f currv
            if curr < acc then
                acc <- curr
                accv <- currv
        accv


type ReadOnlyRow<[<Measure>] 'Measure, 'T>(values: 'T[]) =
    
    do if isNull values then
        raise (System.ArgumentNullException(nameof values))
    

    new (values: (int<'Measure> * 'T) seq) =
        let newValues = Helpers.checkInputSeq values
        ReadOnlyRow<'Measure, 'T> newValues
    
    
    // Must be public to support inline
    member _.Values = values


    member r.Item
        with inline get (i: int<'Measure>) =
            r.Values[int i]

    
    member _.Length = LanguagePrimitives.Int32WithMeasure<'Measure> values.Length


    override _.ToString () =
        $"ReadOnlyRow $A{values}"


    interface IEnumerable<KeyValuePair<int<'Measure>, 'T>> with
        member r.GetEnumerator () : IEnumerator<KeyValuePair<int<'Measure>, 'T>> =
            let values = values
            let x =
                0
                |> Seq.unfold (fun i ->
                    if i < values.Length then
                        let index = LanguagePrimitives.Int32WithMeasure<'Measure> i
                        let next = KeyValuePair (index, values[i])
                        Some (next, i + 1)
                    else
                        None )

            x.GetEnumerator ()

        member r.GetEnumerator () : IEnumerator =
            (r :> IEnumerable<_>).GetEnumerator() :> IEnumerator


module ReadOnlyRow =
    
    [<CompiledName("Create")>]
    let inline create (count: int<'Measure>) value =
        let values = Array.create (int count) value
        ReadOnlyRow<'Measure, _> values
    

    [<CompiledName("Sum")>]
    let inline sum (row: ReadOnlyRow<'Measure, 'T>) =
        let mutable acc = LanguagePrimitives.GenericZero<'T>
        let array = row.Values
        for i = 0 to array.Length - 1 do
            acc <- acc + array[i]

        acc


    [<CompiledName("SumBy")>]
    let inline sumBy ([<InlineIfLambda>] f) (row: ReadOnlyRow<_,_>) =
        let mutable acc = LanguagePrimitives.GenericZero<'T>
        let array = row.Values
        for i = 0 to array.Length - 1 do
            acc <- acc + (f array[i])

        acc
    
        
    [<CompiledName("Iterate")>]
    let inline iter ([<InlineIfLambda>] f) (row: ReadOnlyRow<_,_>) =
        let array = row.Values
        for i = 0 to array.Length - 1 do
            f array[i]
  
    
    [<CompiledName("Iterate2")>]
    let inline iter2 ([<InlineIfLambda>] f: 'a -> 'b -> unit) (a: ReadOnlyRow<'Measure, 'a>) (b: ReadOnlyRow<'Measure, 'b>) =
        let array1 = a.Values
        let array2 = b.Values
        if array1.Length <> array2.Length then
            raise (invalidArg (nameof a) "Cannot iterate through arrays of different lengths")

        for i = 0 to array1.Length - 1 do
            f array1[i] array2[i]
          
            
    [<CompiledName("IterateIndexed")>]
    let inline iteri ([<InlineIfLambda>] f: int<'Measure> -> 'a -> unit) (row: ReadOnlyRow<'Measure, _>) =
        let array = row.Values
        for i = 0 to array.Length - 1 do
            let i = LanguagePrimitives.Int32WithMeasure<'Measure> i
            f i array[int i]
        

    [<CompiledName("IterateIndexed2")>]
    let inline iteri2 ([<InlineIfLambda>] f: int<'Measure> -> 'a -> 'b -> unit) (a: ReadOnlyRow<'Measure, 'a>) (b: ReadOnlyRow<'Measure, 'b>) =
        let array1 = a.Values
        let array2 = b.Values
        if array1.Length <> array2.Length then
            raise (invalidArg (nameof a) "Cannot iterate through arrays of different lengths")

        for i = 0 to array1.Length - 1 do
            let i = LanguagePrimitives.Int32WithMeasure<'Measure> i
            f i array1[int i] array2[int i]
        

    [<CompiledName("Map")>]
    let inline map ([<InlineIfLambda>] f) (row: ReadOnlyRow<'Measure, _>) =
        let array = row.Values
        let res = Array.zeroCreate array.Length

        for i = 0 to array.Length - 1 do
            res[i] <- f array[i]
        
        ReadOnlyRow<'Measure, _> res

    
    [<CompiledName("Map2")>]
    let inline map2 ([<InlineIfLambda>] f: 'a -> 'b -> 'c) (a: ReadOnlyRow<'Measure, 'a>) (b: ReadOnlyRow<'Measure, 'b>) =
        let array1 = a.Values
        let array2 = b.Values
        if array1.Length <> array2.Length then
            let msg = Helpers.invalidArgDifferentRowLength (nameof a) a.Length (nameof b) b.Length
            raise (invalidArg (nameof a) msg)

        let res = Array.zeroCreate array1.Length

        for i = 0 to array1.Length - 1 do
            res[i] <- f array1[i] array2[i]

        ReadOnlyRow<'Measure, _> res
        
        
    [<CompiledName("MapIndexed")>]
    let inline mapi ([<InlineIfLambda>] f: int<'Measure> -> 'a -> 'b) (row: ReadOnlyRow<'Measure, _>) =
        let array = row.Values
        let res = Array.zeroCreate array.Length

        for i = 0 to array.Length - 1 do
            let i = LanguagePrimitives.Int32WithMeasure<'Measure> i
            res[int i] <- f i array[int i]
        
        ReadOnlyRow<'Measure, _> res

    
    [<CompiledName("MapIndexed2")>]
    let inline mapi2 ([<InlineIfLambda>] f: int<'Measure> -> 'a -> 'b -> 'c) (a: ReadOnlyRow<'Measure, 'a>) (b: ReadOnlyRow<'Measure, 'b>) =
        let array1 = a.Values
        let array2 = b.Values
        if array1.Length <> array2.Length then
            let msg = Helpers.invalidArgDifferentRowLength (nameof a) a.Length (nameof b) b.Length
            raise (invalidArg (nameof a) msg)

        let res = Array.zeroCreate array1.Length

        for i = 0 to array1.Length - 1 do
            let i = LanguagePrimitives.Int32WithMeasure<'Measure> i
            res[int i] <- f i array1[int i] array2[int i]

        ReadOnlyRow<'Measure, _> res
        
        
    [<CompiledName("Max")>]
    let inline max (row: ReadOnlyRow<'Measure, _>) =
        let array = row.Values
        if array.Length = 0 then invalidArg (nameof row) "ReadOnlyRow cannot be empty"
        let mutable acc = array[0]
        for i = 1 to array.Length - 1 do
            let curr = array[i]
            if curr > acc then 
                acc <- curr
        acc
        

    [<CompiledName("MaxBy")>]
    let inline maxBy ([<InlineIfLambda>] f) (row: ReadOnlyRow<'Measure, _>) =
        let array = row.Values
        if array.Length = 0 then invalidArg (nameof row) "ReadOnlyRow cannot be empty"
        let mutable accv = array[0]
        let mutable acc = f accv
        for i = 1 to array.Length - 1 do
            let currv = array[i]
            let curr = f currv
            if curr > acc then
                acc <- curr
                accv <- currv
        accv
        

    [<CompiledName("Min")>]
    let inline min (row: ReadOnlyRow<'Measure, _>) =
        let array = row.Values
        if array.Length = 0 then invalidArg (nameof row) "ReadOnlyRow cannot be empty"
        let mutable acc = array[0]
        for i = 1 to array.Length - 1 do
            let curr = array[i]
            if curr < acc then 
                acc <- curr
        acc
        

    [<CompiledName("MinBy")>]
    let inline minBy ([<InlineIfLambda>] f) (row: ReadOnlyRow<'Measure, _>) =
        let array = row.Values
        if array.Length = 0 then invalidArg (nameof row) "ReadOnlyRow cannot be empty"
        let mutable accv = array[0]
        let mutable acc = f accv
        for i = 1 to array.Length - 1 do
            let currv = array[i]
            let curr = f currv
            if curr < acc then
                acc <- curr
                accv <- currv
        accv
