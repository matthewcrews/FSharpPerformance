﻿module rec Row

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


[<Struct>]
type Bar<[<Measure>] 'Measure, 'T> internal (values: 'T[]) =
    
    new (count, value) =
        let newValues = Array.create count value
        Bar<_,_> newValues
    
    /// WARNING: This member is not intended for public consumption
    /// It is public to support inlining
    member _._Values = values
    
    member inline b.Length = LanguagePrimitives.Int32WithMeasure<'Measure> b._Values.Length
    
    member b.Item
        with inline get(i: int<'Measure>) =
            b._Values[int i]
            
    member b.Item
        with inline get(i: byte<'Measure>) =
            b._Values[int (byte i)]
            
            
module Bar =
    
    let create (values: (int<'Measure> * 'T) seq) =
        values
        |> Helpers.checkInputSeq
        |> Bar<'Measure, 'T>
    
    [<CompiledName("Iterate")>]
    let inline iter ([<InlineIfLambda>] f: 'a -> unit) (row: Bar<'Measure, _>) =
        let array = row._Values
        for i = 0 to array.Length - 1 do
            let i = LanguagePrimitives.Int32WithMeasure<'Measure> i
            f array[int i]
    
    [<CompiledName("IterateIndexed")>]
    let inline iteri ([<InlineIfLambda>] f: int<'Measure> -> 'a -> unit) (row: Bar<'Measure, _>) =
        let array = row._Values
        for i = 0 to array.Length - 1 do
            let i = LanguagePrimitives.Int32WithMeasure<'Measure> i
            f i array[int i]

//    [<CompiledName("Map")>]
//    let inline map ([<InlineIfLambda>] f) (row: Bar<'Measure, _>) =
//            let array = row._Values
//            let res = Array.zeroCreate array.Length
//
//            for i = 0 to array.Length - 1 do
//                res[i] <- f array[i]
//            
//            Bar<'Measure, _> res


type Row<[<Measure>] 'Measure, 'T>(values: array<'T>) =
    do if isNull values then
        raise (new System.ArgumentNullException(nameof values)) 

    new (length: int<'Measure>, value: 'T) =
        Row (Array.create (int length) value)

    new (other: Row<'Measure, 'T>) =
        let newValues = other.Values
        Row<'Measure, _> newValues
        
    new (values: (int<'Measure> * 'T) seq) =
        let newValues = Helpers.checkInputSeq values
        Row<'Measure, 'T> newValues
        
    /// WARNING: This member is not intended for public consumption
    /// It is public to support inlining
    member _.Values : 'T array = values

    member r.Item
        with inline get (i: int<'Measure>) =
            r.Values[int i]

        and inline set (index: int<'Measure>) value =
            r.Values[int index] <- value

    member _.Length = LanguagePrimitives.Int32WithMeasure<'Measure> values.Length

    member r.Bar = Bar<'Measure, _> r.Values
    
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

    let inline create (count: int<'Measure>) value =
        let values = Array.create (int count) value
        Row<'Measure, _> values
    

    let inline sum (row: Row<'Measure, 'T>) =
        let mutable acc = LanguagePrimitives.GenericZero<'T>
        let array = row.Values
        for i = 0 to array.Length - 1 do
            acc <- acc + array[i]

        acc


    let inline sumBy ([<InlineIfLambda>] f) (row: Row<_,_>) =
        let mutable acc = LanguagePrimitives.GenericZero<'T>
        let array = row.Values
        for i = 0 to array.Length - 1 do
            acc <- acc + (f array[i])

        acc
    

    let inline iter ([<InlineIfLambda>] f) (row: Row<_,_>) =
        let array = row.Values
        for i = 0 to array.Length - 1 do
            f array[i]
  
            
    let inline iteri ([<InlineIfLambda>] f: int<'Measure> -> 'a -> unit) (row: Row<'Measure, _>) =
        let array = row.Values
        for i = 0 to array.Length - 1 do
            let i = LanguagePrimitives.Int32WithMeasure<'Measure> i
            f i array[int i]
        

    let inline iteri2 ([<InlineIfLambda>] f: int<'Measure> -> 'a -> 'b -> unit) (a: Row<'Measure, 'a>) (b: Row<'Measure, 'b>) =
        (a.Values, b.Values)
        ||> Array.iteri2 (fun i aValue bValue ->
            let i = LanguagePrimitives.Int32WithMeasure<'Measure> i
            f i aValue bValue)
        

    let inline map ([<InlineIfLambda>] f) (row: Row<'Measure, _>) =
        let array = row.Values
        let res = Array.zeroCreate array.Length

        for i = 0 to array.Length - 1 do
            res[i] <- f array[i]
        
        Row<'Measure, _> res

        
    let inline mapi ([<InlineIfLambda>] f: int<'Measure> -> 'a -> 'b) (row: Row<'Measure, _>) =
        let array = row.Values
        let res = Array.zeroCreate array.Length

        for i = 0 to array.Length - 1 do
            let i = LanguagePrimitives.Int32WithMeasure<'Measure> i
            res[int i] <- f i array[int i]
        
        Row<'Measure, _> res

        
    let inline max (row: Row<'Measure, _>) =
        let array = row.Values
        if array.Length = 0 then invalidArg (nameof row) "Row cannot be empty"
        let mutable acc = array[0]
        for i = 1 to array.Length - 1 do
            let curr = array[i]
            if curr > acc then 
                acc <- curr
        acc
        

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
        

    let inline min (row: Row<'Measure, _>) =
        let array = row.Values
        if array.Length = 0 then invalidArg (nameof row) "Row cannot be empty"
        let mutable acc = array[0]
        for i = 1 to array.Length - 1 do
            let curr = array[i]
            if curr < acc then 
                acc <- curr
        acc
        

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
        
        
    module InPlace =
                
        let inline add (source: Row<'Measure, _>) (target: Row<'Measure, _>) =
            
            for i = 0 to target.Values.Length - 1 do
                target.Values[i] <- target.Values[i] + source.Values[i]
