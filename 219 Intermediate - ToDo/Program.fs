open System

let getVals cat mapping = match Map.tryFind cat mapping with | Some lst -> lst | None -> List.empty
let addTo key value mapping = mapping |> Map.add key (value::(mapping |> getVals key))
let rec addToMulti keys value (mapping:Map<string,List<string>>) = 
    if keys |> Seq.isEmpty then 
            mapping
    else
        let added = mapping |> addTo (Seq.head keys) value
        added |> addToMulti (keys |> Seq.skip 1) value 
let print (key:string) vals = 
    printfn "----%s----" key
    for value in vals do printfn " - %s" value
let pairs lines = 
    let arr = Seq.toArray lines
    [ for i in 0 .. 2 .. (Array.length arr)-1 do yield arr.[i], arr.[i+1] ]

type ToDo = {Items:Map<string,List<string>>} with
    member this.add ([<ParamArray>] args: string[]) = 
        let item = args.[0]
        let cats = 
            args
            |> Seq.ofArray
            |> Seq.skip 1
            |> Seq.map (fun cat -> cat.ToUpper ())
        {Items = this.Items |> addToMulti cats item }

    member this.remove item = 
        {Items = this.Items
        |> Map.map (fun k v -> (this.Items |> getVals k) |> List.filter (fun v -> v <> item) ) 
        |> Map.filter (fun k v -> not (List.isEmpty v)) }
        
    member this.update (oldval,newval) = 
        {Items = this.Items
        |> Map.map (fun k v -> (this.Items |> getVals k) |> List.map (fun v -> match v with | item when item = oldval -> newval | other -> other) ) }

    member this.view ([<ParamArray>] args: string[]) = 
        let cats = if Array.isEmpty args then this.Items |> Map.toSeq |> Seq.map fst else args |> Array.toSeq |> Seq.map (fun cat -> cat.ToUpper ())
        for cat in cats |> Seq.sort do 
            print cat (List.rev (this.Items |> getVals cat))
            printfn ""

let load path =
    if System.IO.File.Exists(path) then 
        {Items = System.IO.File.ReadLines(path)
        |> pairs
        |> List.map (fun (cat,csv) -> cat,(csv.Split(',') |> Array.toList))
        |> Map.ofList }
    else
        {Items = Map.empty}

let save path todo = 
    let contents = (todo.Items |> Map.toArray |> Array.map (fun (cat,lst) -> sprintf "%s\n%s" (cat.ToUpper ()) (String.concat "," lst))) |> String.concat "\n"
    System.IO.File.WriteAllLines(path, [contents]) 

[<EntryPoint>]
let main argv = 
    let path = "../../todo.txt"
    let todo = (load path).add("Replace fingers", "Programming", "Music").add("Learn the mandolin", "Music").add("Demolish Taco Bell grande meal", "Food")
    let updated = todo.add("Temporary item", "Misc", "Programming").add("Remember to eat something", "Programming", "Food").remove("Temporary item").update("Replace fingers","Get robot hands")
    
    updated.view("Food","Programming","Music")
    save path updated

    System.Console.ReadKey() |> ignore
    0