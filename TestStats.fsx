open System
open System.Xml

let nodes = seq {
    use reader = XmlReader.Create("TestResult.xml")
    while reader.Read() do
        match reader.Name with
        | "test-case" -> yield reader.["name"], TimeSpan.FromSeconds(float(reader.["time"]))
        | _ -> () }

nodes 
|> Seq.sortBy snd
|> Seq.iter (fun (name, time) -> Console.WriteLine("{0} {1}", name, time.TotalSeconds))