module StructOfArrays.Key

[<RequireQualifiedAccess>]
module Key =

    [<Measure>] type House
    [<Measure>] type Purchase
    [<Measure>] type State
    [<Measure>] type ZipCode


type HouseKey = int<Key.House>
type PurchaseKey = int<Key.Purchase>
type StateKey = int<Key.State>
type ZipCodeKey = int<Key.ZipCode>


module HouseKey =
    
    let create i : HouseKey =
        if i < 0 then
            invalidArg (nameof i) "Cannot have HouseKey less than 0"
            
        i * 1<_> // Convert to int<Key.House>
        
        
module PurchaseKey =
    
    let create i : PurchaseKey =
        if i < 0 then
            invalidArg (nameof i) "Cannot have PurchaseKey less than 0"
            
        i * 1<_> // Convert to int<Key.Purchase>
        
        
module StateKey =
    
    let create i : StateKey =
        if i < 0 then
            invalidArg (nameof i) "Cannot have StreetKey less than 0"
            
        i * 1<_> // Convert to int<Key.Street>
        
        
module ZipCodeKey =
    
    let create i : ZipCodeKey =
        if i < 0 then
            invalidArg (nameof i) "Cannot have ZipCodeKey less than 0"
            
        i * 1<_> // Convert to int<Key.ZipCode>