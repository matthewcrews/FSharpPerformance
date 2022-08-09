namespace rec RowEnumeration

open System.Collections.Generic

[<Struct>]
type RowEnumerator<[<Measure>] 'Measure, 'T> =
    val mutable private currentIndex : int
    val private row : Row<'Measure, 'T>

    new (row: Row<'Measure, 'T>) =
        {
            currentIndex = -1
            row = row
        }
        
    member this.MoveNext () : bool =
        if this.currentIndex + 1 < this.row._values.Length then
            this.currentIndex <- this.currentIndex + 1
            true
        else
            false
            
    member this.Current : struct(int<'Measure> * 'T) =
        let currentIndexWithMeasure = LanguagePrimitives.Int32WithMeasure<'Measure> this.currentIndex
        currentIndexWithMeasure, this.row._values[int currentIndexWithMeasure]
        
    // interface IEnumerator<struct(int<'Measure> * 'T)> with
    //     member this.MoveNext () : bool =
    //         if this.currentIndex + 1 < this.row._values.Length then
    //             this.currentIndex <- this.currentIndex + 1
    //             true
    //         else
    //             false
    //             
    //     member this.Reset () : unit =
    //         this.currentIndex <- -1
    //         
    //     member this.Current : struct(int<'Measure> * 'T) =
    //         let currentIndexWithMeasure = LanguagePrimitives.Int32WithMeasure<'Measure> this.currentIndex
    //         currentIndexWithMeasure, this.row._values[int currentIndexWithMeasure]
    //         
    //     member this.Current : obj =
    //         let currentIndexWithMeasure = LanguagePrimitives.Int32WithMeasure<'Measure> this.currentIndex
    //         box (currentIndexWithMeasure, this.row._values[int currentIndexWithMeasure])
            
        // member _.Dispose () = ()


[<Struct>]
type Row<[<Measure>] 'Measure, 'T>(values: 'T[]) =

    new (length: int, value: 'T) =
        Row (Array.create (int length) value)
        
    /// WARNING: This member is public for the purposes of inlining but
    /// it is not meant for public consumption. This is a limitation of
    /// the F# Compiler.
    member _._values : 'T[] = values

    
    member r.Item
        with inline get (i: int<'Measure>) : 'T =
            r._values[int i]

        and inline set (i: int<'Measure>) value : unit =
            r._values[int i] <- value


    member inline r.Length = LanguagePrimitives.Int32WithMeasure<'Measure> r._values.Length


    override row.ToString () =
        $"Row %A{row._values}"

    
    member this.GetEnumerator () = RowEnumerator<'Measure, 'T>(this)
    
    // interface IEnumerable<struct (int<'Measure> * 'T)> with
    //     member this.GetEnumerator () : IEnumerator<struct (int<'Measure> * 'T)> =
    //         new RowEnumerator<'Measure, 'T>(this)
    //
    //     member this.GetEnumerator () : System.Collections.IEnumerator =
    //         (this :> IEnumerable<_>).GetEnumerator() :> System.Collections.IEnumerator
    
    // interface IEnumerable<KeyValuePair<int<'Measure>, 'T>> with
    //         member r.GetEnumerator () : IEnumerator<KeyValuePair<int<'Measure>, 'T>> =
    //             let values = values
    //             let x =
    //                 0
    //                 |> Seq.unfold (fun i ->
    //                     if i < values.Length then
    //                         let index = LanguagePrimitives.Int32WithMeasure<'Measure> i
    //                         let next = KeyValuePair (index, values[i])
    //                         Some (next, i + 1)
    //                     else
    //                         None )
    //
    //             x.GetEnumerator ()
    //
    //         member r.GetEnumerator () : IEnumerator =
    //             (r :> IEnumerable<_>).GetEnumerator() :> IEnumerator


module Row =

    module private Helpers =
        
        let inline invalidArgDifferentRowLength rowAName lengthA rowBName lengthB =
            $"{rowAName}.Length = {lengthA}, {rowBName}.Length = {lengthB}"
    
    
    [<CompiledName("Create")>]
    let inline create (count: int<'Measure>) value =
        let values = Array.create (int count) value
        Row<'Measure, _> values

    
    [<CompiledName("Sum")>]
    let inline sum (row: Row<'Measure, 'T>) =
        let mutable acc = LanguagePrimitives.GenericZero<'T>
        let array = row._values
        for i = 0 to array.Length - 1 do
            acc <- acc + array[i]

        acc


    [<CompiledName("SumBy")>]
    let inline sumBy ([<InlineIfLambda>] f) (row: Row<'Measure, 'T>) =
        let mutable acc = LanguagePrimitives.GenericZero<'T>
        let array = row._values
        for i = 0 to array.Length - 1 do
            acc <- acc + (f array[i])

        acc
    

    [<CompiledName("Iterate")>]
    let inline iter ([<InlineIfLambda>] f) (row: Row<_,_>) =
        let array = row._values
        for i = 0 to array.Length - 1 do
            f array[i]
       
       
    [<CompiledName("Iterate2")>]
    let inline iter2 ([<InlineIfLambda>] f: 'a -> 'b -> unit) (a: Row<'Measure, 'a>) (b: Row<'Measure, 'b>) =
        let array1 = a._values
        let array2 = b._values
        if array1.Length <> array2.Length then
            raise (invalidArg (nameof a) "Cannot iterate through arrays of different lengths")

        for i = 0 to array1.Length - 1 do
            f array1[i] array2[i]
  
            
    [<CompiledName("IterateIndexed")>]
    let inline iteri ([<InlineIfLambda>] f: int<'Measure> -> 'a -> unit) (row: Row<'Measure, _>) =
        let array = row._values
        for i = 0 to array.Length - 1 do
            let i = LanguagePrimitives.Int32WithMeasure<'Measure> i
            f i array[int i]

    
    [<CompiledName("IterateIndexed2")>]
    let inline iteri2 ([<InlineIfLambda>] f: int<'Measure> -> 'a -> 'b -> unit) (a: Row<'Measure, 'a>) (b: Row<'Measure, 'b>) =
        let array1 = a._values
        let array2 = b._values
        if array1.Length <> array2.Length then
            raise (invalidArg (nameof a) "Cannot iterate through arrays of different lengths")

        for i = 0 to array1.Length - 1 do
            let i = LanguagePrimitives.Int32WithMeasure<'Measure> i
            f i array1[int i] array2[int i]


    [<CompiledName("Map")>]
    let inline map ([<InlineIfLambda>] f) (row: Row<'Measure, _>) =
        let array = row._values
        let res = System.GC.AllocateUninitializedArray (array.Length, false)

        for i = 0 to array.Length - 1 do
            res[i] <- f array[i]
        
        Row<'Measure, _> res


    [<CompiledName("Map2")>]
    let inline map2 ([<InlineIfLambda>] f: 'a -> 'b -> 'c) (a: Row<'Measure, 'a>) (b: Row<'Measure, 'b>) =
        let array1 = a._values
        let array2 = b._values
        if array1.Length <> array2.Length then
            let msg = Helpers.invalidArgDifferentRowLength (nameof a) a.Length (nameof b) b.Length
            raise (invalidArg (nameof a) msg)

        let res = System.GC.AllocateUninitializedArray (array1.Length, false)

        for i = 0 to array1.Length - 1 do
            res[i] <- f array1[i] array2[i]

        Row<'Measure, _> res

            
    [<CompiledName("MapIndexed")>]
    let inline mapi ([<InlineIfLambda>] f: int<'Measure> -> 'a -> 'b) (row: Row<'Measure, _>) =
        let array = row._values
        let res = System.GC.AllocateUninitializedArray (array.Length, false)

        for i = 0 to array.Length - 1 do
            let i = LanguagePrimitives.Int32WithMeasure<'Measure> i
            res[int i] <- f i array[int i]
        
        Row<'Measure, _> res


    [<CompiledName("MapIndexed2")>]
    let inline mapi2 ([<InlineIfLambda>] f: int<'Measure> -> 'a -> 'b -> 'c) (a: Row<'Measure, 'a>) (b: Row<'Measure, 'b>) =
        let array1 = a._values
        let array2 = b._values
        if array1.Length <> array2.Length then
            let msg = Helpers.invalidArgDifferentRowLength (nameof a) a.Length (nameof b) b.Length
            raise (invalidArg (nameof a) msg)

        let res = System.GC.AllocateUninitializedArray (array1.Length, false)

        for i = 0 to array1.Length - 1 do
            let i = LanguagePrimitives.Int32WithMeasure<'Measure> i
            res[int i] <- f i array1[int i] array2[int i]

        Row<'Measure, _> res

        
    [<CompiledName("Max")>]
    let inline max (row: Row<'Measure, _>) =
        let array = row._values
        if array.Length = 0 then invalidArg (nameof row) "Row cannot be empty"
        let mutable acc = array[0]
        for i = 1 to array.Length - 1 do
            let curr = array[i]
            if curr > acc then 
                acc <- curr
        acc
        

    [<CompiledName("MaxBy")>]
    let inline maxBy ([<InlineIfLambda>] f) (row: Row<'Measure, _>) =
        let array = row._values
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
        let array = row._values
        if array.Length = 0 then invalidArg (nameof row) "Row cannot be empty"
        let mutable acc = array[0]
        for i = 1 to array.Length - 1 do
            let curr = array[i]
            if curr < acc then 
                acc <- curr
        acc
        

    [<CompiledName("MinBy")>]
    let inline minBy ([<InlineIfLambda>] f) (row: Row<'Measure, _>) =
        let array = row._values
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

    [<CompiledName("Copy")>]
    let inline copy (row: Row<'Measure, _>) =
        let newValues = Array.copy row._values
        Row<'Measure, _> newValues
