#r "Tools\DotNetZip\Ionic.Zip.Reduced.dll"
open System
open System.Diagnostics
open System.IO
open System.Reflection
open Ionic.Zip

let clean what =
    what |> Seq.map (fun p -> try Directory.Delete(p, true); true with | :? DirectoryNotFoundException -> true | _ -> false) |> Seq.reduce (&&)

let build args =
  let fxPath = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory()  
  let msBuild4 = Path.Combine(fxPath, @"..\v4.0.30319\MSBuild.exe")
  use build =
    Process.Start(
      ProcessStartInfo(
        FileName = msBuild4,
        Arguments = "Cone.sln /nologo /v:m /p:Configuration=Release " + args,
        UseShellExecute = false))
  build.WaitForExit()
  Console.WriteLine("build {0} exited with {1}", args, build.ExitCode)
  build.ExitCode = 0

let package() =
    let version = AssemblyName.GetAssemblyName("Build\Cone.dll").Version.ToString()

    use zip = new ZipFile()

    zip.AddDirectory("Bin", "Bin") |> ignore
    zip.AddDirectory("Docs", "Docs") |> ignore
    zip.Save(@"Bin\Cone-" + version + ".zip")
    true

clean ["Build";"Bin"]
&& build ""
&& package()
