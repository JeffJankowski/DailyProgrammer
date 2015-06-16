[<EntryPoint>]
let main argv = 


    let addItem item list = 
        item :: list
    let deleteItem item list = 
        List.filter (fun i -> i <> item) list
    let printList list = 
        printf "TODO:\n"
        list |> List.rev |> List.iter (fun i -> printf "  - %s\n" i)

    List.Empty
    |> addItem "Buy food, or starve to death whatever.."
    |> addItem "Get better at F#"
    |> addItem "Probably just go to bed"
    |> deleteItem "Get better at F#"
    |> printList


    System.Console.ReadKey() |> ignore
    0

