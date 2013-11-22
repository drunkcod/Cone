#r @"Build\Cone.dll"
open System
open System.IO
open Cone.Core

let path = @"Source\Version.cs"
File.WriteAllText(path, VersionUpdater.Update(DateTime.Today, File.ReadAllText(path)))
