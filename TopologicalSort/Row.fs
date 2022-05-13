module rec TopologicalSort.Row

open System
open System.Collections
open System.Collections.Generic
open System.Diagnostics.SymbolStore

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


type Row<[<Measure>] 'Measure, 'T>(values: array<'T>) =

    new (length: int<'Measure>, value: 'T) =
        Row (Array.create (int length) value)

    new (other: Row<'Measure, 'T>) =
        let newValues = other.Values
        Row<'Measure, _> newValues
        
    new (values: (int<'Measure> * 'T) seq) =
        let newValues = Helpers.checkInputSeq values
        Row<'Measure, 'T> newValues
        
    member _.Values : 'T array = Array.copy values

    member _.Item
        with get (i: int<'Measure>) =
            values[int i]

        and set (index: int<'Measure>) value =
            values[int index] <- value

    member _.Length = LanguagePrimitives.Int32WithMeasure<'Measure> values.Length

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
    
    let inline iter ([<InlineIfLambda>] f) (row: Row<_,_>) =
        row.Values
        |> Array.iter f
        
    let inline iteri ([<InlineIfLambda>] f: int<'Measure> -> 'a -> unit) (row: Row<'Measure, _>) =
        row.Values
        |> Array.iteri (fun i v ->
            let i = LanguagePrimitives.Int32WithMeasure<'Measure> i
            f i v)
        
    let inline iteri2 ([<InlineIfLambda>] f: int<'Measure> -> 'a -> 'b -> unit) (a: Row<'Measure, 'a>) (b: Row<'Measure, 'b>) =
        (a.Values, b.Values)
        ||> Array.iteri2 (fun i aValue bValue ->
            let i = LanguagePrimitives.Int32WithMeasure<'Measure> i
            f i aValue bValue)
        
    let inline map ([<InlineIfLambda>] f) (row: Row<'Measure, _>) =
        row.Values
        |> Array.map f
        |> Row<'Measure, _>
        
    let inline mapi ([<InlineIfLambda>] f: int<'Measure> -> 'a -> 'b) (row: Row<'Measure, _>) =
        row.Values
        |> Array.mapi (fun i v ->
            let i = LanguagePrimitives.Int32WithMeasure<'Measure> i
            f i v)
        |> Row<'Measure, _>
        
    let inline max (row: Row<'Measure, _>) =
        Array.max row.Values
        
    let inline maxBy ([<InlineIfLambda>] f) (row: Row<'Measure, _>) =
        row.Values
        |> Array.maxBy f
        
    let inline min (row: Row<'Measure, _>) =
        Array.min row.Values
        
    let inline minBy ([<InlineIfLambda>] f) (row: Row<'Measure, _>) =
        row.Values
        |> Array.minBy f
        
    let inline scale a (row: Row<'Measure, _>) =
        row.Values
        |> Array.map (fun elem -> a * elem)
        |> Row<'Measure, _>
        
        
    module InPlace =
                
        let inline add (source: Row<'Measure, _>) (target: Row<'Measure, _>) =
            
            for i = 0 to target.Values.Length - 1 do
                target.Values[i] <- target.Values[i] + source.Values[i]


type ReadOnlyRow<[<Measure>] 'Measure, 'T>(values: array<'T>) =

    new (values: (int<'Measure> * 'T) seq) =
        let newValues = Helpers.checkInputSeq values
        ReadOnlyRow<'Measure, 'T> newValues
    
    new (row: Row<'Measure, 'T>) =
        let newValues =
            row.Values
            |> Array.copy
        ReadOnlyRow<'Measure, 'T> newValues
    
    member _.Values = values

    member _.Item
        with get (i: int<'Measure>) =
            values[int i]

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
    
    let inline iter ([<InlineIfLambda>] f) (row: ReadOnlyRow<_,_>) =
        row.Values
        |> Array.iter f
        
    let inline iteri ([<InlineIfLambda>] f: int<'Measure> -> 'a -> unit) (row: ReadOnlyRow<'Measure, _>) =
        row.Values
        |> Array.iteri (fun i v ->
            let i = LanguagePrimitives.Int32WithMeasure<'Measure> i
            f i v)
        
    let inline map ([<InlineIfLambda>] f) (row: ReadOnlyRow<'Measure, _>) =
        row.Values
        |> Array.map f
        |> ReadOnlyRow<'Measure, _>
        
    let inline mapi ([<InlineIfLambda>] f: int<'Measure> -> 'a -> 'b) (row: ReadOnlyRow<'Measure, _>) =
        row.Values
        |> Array.mapi (fun i v ->
            let i = LanguagePrimitives.Int32WithMeasure<'Measure> i
            f i v)
        |> ReadOnlyRow<'Measure, _>
        
    let inline max (row: ReadOnlyRow<'Measure, _>) =
        Array.max row.Values
        
    let inline maxBy ([<InlineIfLambda>] f) (row: ReadOnlyRow<'Measure, _>) =
        row.Values
        |> Array.maxBy f
        
    let inline min (row: ReadOnlyRow<'Measure, _>) =
        Array.min row.Values
        
    let inline minBy ([<InlineIfLambda>] f) (row: ReadOnlyRow<'Measure, _>) =
        row.Values
        |> Array.minBy f
