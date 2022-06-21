module StructOfArrays.StructOfArrays

open System
open System.Collections.ObjectModel
open Row
open Key

type AskingPrice =
    {
        Value : decimal
    }
type SoldPrice =
    {
        Value : decimal
    }
type State = State of string
type ZipCode = ZipCode of string
type SettlementDate = SettlementDate of DateOnly
[<Measure>] type ft

//type States =
//    {
//        Keys : ReadOnlyDictionary<State, StateKey>
//        Value: Bar<Key.State, State>
//    }
//
//type ZipCodes =
//    {
//        Keys : ReadOnlyDictionary<ZipCode, ZipCodeKey>
//        Value: Bar<Key.ZipCode, ZipCode>
//    }

type Houses =
    {
        State : Bar<Key.House, State>
        ZipCode : Bar<Key.House, ZipCode>
        Area : Bar<Key.House, uint<ft^2>>
    }
    
type Purchases =
    {
        House : Bar<Key.Purchase, HouseKey>
        AskingPrice : Bar<Key.Purchase, AskingPrice>
        SoldPrice : Bar<Key.Purchase, SoldPrice>
        SettlementDate : Bar<Key.Purchase, SettlementDate>
    }
