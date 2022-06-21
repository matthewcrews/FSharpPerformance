module StructOfArrays.Naive

open System

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

type House =
    {
        State : State
        ZipCode : ZipCode
        Area : uint<ft^2>
    }
    
type Purchase =
    {
        House : House
        AskingPrice : AskingPrice
        SoldPrice : SoldPrice
        SettlementDate : SettlementDate
    }
