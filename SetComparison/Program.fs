open SetComparison


[<Measure>] type Chicken

[<EntryPoint>]
let main args =
    
    let capacity = 10<Chicken>
    let b = BitSet.create capacity
    let interactionTracker = InteractionTracker.create capacity
    
    let item1 = 1<Chicken>
    let item2 = 2<Chicken>
    
    b.Add item2
    
    interactionTracker.Add (item1, item2)
    
    let rest = interactionTracker.CheckForMatch (item1, b)
    
    1