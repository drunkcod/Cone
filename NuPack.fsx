open System
open System.IO
open System.Text
open System.Reflection

let version = Assembly.LoadFile(Path.GetFullPath("Build\\Cone.dll")).GetName().Version;
let proj = fsi.CommandLineArgs.[1]
Console.WriteLine("Creating nuspec for {0} {1}", proj, version)
File.ReadAllText(proj + ".nutemplate").Replace("$Version$", version.ToString())
|> (fun result -> File.WriteAllText(proj + ".nuspec", result, Encoding.UTF8))
