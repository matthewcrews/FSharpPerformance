open System
open System.Collections.ObjectModel
open Argu
open BenchmarkDotNet.Diagnosers
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running
open StructOfArrays.StructOfArrays

module Data =
    
    let rng = Random 123
    let minZipCount = 4    
    let maxZipCount = 2_500
    let minSqFt = 1_000
    let maxSqFt = 5_000
    let minAskingPrice = 100_000
    let maxAskingPrice = 100_000_000
    let minSettlementDate = DateOnly (2020, 1, 1)
    let settlementDatePeriod = 300
    let settlementDates =
        [| for i in 0 .. settlementDatePeriod do
            minSettlementDate.AddDays i
        |]
    
    let houseCount = 100_000
    let purchaseCount = 10_000
    
    module Naive =
        open StructOfArrays.Naive
        
        let rng = Random 123
        let states =
            [| for i in 1 .. 50 do
                State $"State[{i}]"
            |]
            
        let zipCodesForState =
            [| for state in states do
                let zipCount = rng.Next (minZipCount, maxZipCount)
                [| for zip in 1 .. zipCount do
                    ZipCode $"{state}_Zip[zip]", state
                |]
            |]
            |> Array.concat
            
        let houses =
            [| for _ in 0 .. houseCount - 1 do
                let zipCode, state = zipCodesForState[rng.Next zipCodesForState.Length]
                let area = uint (rng.Next (minSqFt, maxSqFt)) * 1u<ft^2>
                {
                    State = state
                    ZipCode = zipCode
                    Area = area
                }       
            |]
            
        let purchases =
            [| for _ in 0 .. purchaseCount - 1 do
                let house = houses[rng.Next houses.Length]
                let askingPrice =
                    rng.Next (minAskingPrice, maxAskingPrice)
                    |> decimal
                let soldPrice = 
                    rng.Next (minAskingPrice, maxAskingPrice)
                    |> decimal
                let settlementDate =
                    settlementDates[rng.Next settlementDates.Length]
                    |> SettlementDate
                {
                    House = house
                    AskingPrice = { Value = askingPrice }
                    SoldPrice = { Value = soldPrice }
                    SettlementDate = settlementDate
                }       
            |]
            
    module StructOfArrays =
        
        open Row
        open StructOfArrays.Key
        open StructOfArrays.StructOfArrays

        let rng = Random 123
        let states =
            [| for i in 1 .. 50 do
                State $"State[{i}]"
            |]
            
        let zipCodesForState =
            [| for state in states do
                let zipCount = rng.Next (minZipCount, maxZipCount)
                [| for zip in 1 .. zipCount do
                    ZipCode $"{state}_Zip[zip]", state
                |]
            |]
            |> Array.concat
            
        let houses =
            let data =
                [| for i in 0 .. houseCount - 1 do
                    let houseKey = HouseKey.create i
                    let zipCode, state = zipCodesForState[rng.Next zipCodesForState.Length]
                    let area = uint (rng.Next (minSqFt, maxSqFt)) * 1u<ft^2>
                    houseKey, state, zipCode, area
                |]
                
            let state =
                data
                |> Array.map (fun (houseKey, state, _, _) -> houseKey, state)
                |> Bar.create
                
            let zipcode =
                data
                |> Array.map (fun (houseKey, _, zipCode, _) -> houseKey, zipCode)
                |> Bar.create
                
            let area =
                data
                |> Array.map (fun (houseKey, _, _, area) -> houseKey, area)
                |> Bar.create
                
            {
                State = state
                ZipCode = zipcode
                Area = area
            }
            
        let purchases =
            let data =
                [| for i in 0 .. purchaseCount - 1 do
                    let purchaseKey = PurchaseKey.create i
                    let houseKey = (rng.Next houseCount) |> HouseKey.create
                    
                    let askingPrice =
                        rng.Next (minAskingPrice, maxAskingPrice)
                        |> decimal
                    
                    let soldPrice = 
                        rng.Next (minAskingPrice, maxAskingPrice)
                        |> decimal
                    
                    let settlementDate =
                        settlementDates[rng.Next settlementDates.Length]
                        |> SettlementDate
                    
                    purchaseKey, houseKey, askingPrice, soldPrice, settlementDate     
                |]
                
            let house =
                data
                |> Array.map (fun (purchaseKey, houseKey, _, _, _) -> purchaseKey, houseKey)
                |> Bar.create
                
            let askingPrice =
                data
                |> Array.map (fun (purchaseKey, _, askingPrice, _, _) -> purchaseKey, { AskingPrice.Value = askingPrice })
                |> Bar.create

            let soldPrice =
                data
                |> Array.map (fun (purchaseKey, _, _, soldPrice, _) -> purchaseKey, { Value = soldPrice })
                |> Bar.create
                
            let settlementDate =
                data
                |> Array.map (fun (purchaseKey, _, _, _, settlementDate) -> purchaseKey, settlementDate)
                |> Bar.create
                
            {
                House = house
                AskingPrice = askingPrice
                SoldPrice = soldPrice
                SettlementDate = settlementDate
            }


type Benchmarks () =

    
    [<Benchmark>]
    member _.NaiveArrayAvgPriceByState () =
        Data.Naive.purchases
        |> Array.groupBy (fun purchase -> purchase.House.State)
        |> Array.map (fun (state, group) ->
            state, group |> Array.sumBy (fun purchase -> purchase.SoldPrice.Value))
        
    [<Benchmark>]
    member _.NaiveSeqAvgPriceByState () =
        Data.Naive.purchases
        |> Seq.groupBy (fun purchase -> purchase.House.State)
        |> Seq.map (fun (state, group) ->
            state, group |> Seq.sumBy (fun purchase -> purchase.SoldPrice.Value))
        |> Array.ofSeq
        


let profile (version: string) loopCount =
    
    let b = Benchmarks ()
    let mutable result = 0

    result


[<RequireQualifiedAccess>]
type Args =
    | Task of task: string
    | Method of method: string
    | Iterations of iterations: int
    
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Task _ -> "Which task to perform. Options: Benchmark or Profile"
            | Method _ -> "Which Method to profile. Options: V<number>. <number> = 01 - 10"
            | Iterations _ -> "Number of iterations of the Method to perform for profiling"


[<EntryPoint>]
let main argv =

    let parser = ArgumentParser.Create<Args> (programName = "Topological Sort")
    let results = parser.Parse argv
    let task = results.GetResult Args.Task

    match task.ToLower() with
    | "benchmark" -> 
        let _ = BenchmarkRunner.Run<Benchmarks>()
        ()

    | "profile" ->
        let method = results.GetResult Args.Method
        let iterations = results.GetResult Args.Iterations
        let _ = profile method iterations
        ()
        
    | unknownTask -> failwith $"Unknown task: {unknownTask}"
    
    1