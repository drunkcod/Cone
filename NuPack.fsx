open System
open System.IO
open System.Text
open System.Reflection

let version = Assembly.LoadFile(Path.GetFullPath("Build\\Cone.dll")).GetName().Version;

File.ReadAllText("Cone.nutemplate").Replace("$Version$", version.ToString())
|> (fun result -> File.WriteAllText("Cone.nuspec", result, Encoding.UTF8))
