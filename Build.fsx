#r "Tools\DotNetZip\Ionic.Zip.Reduced.dll"
open System.Diagnostics
open System.IO
open System.Reflection
open Ionic.Zip

let build args =
  let fxPath = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory()  
  let msBuild4 = Path.Combine(fxPath, @"..\v4.0.30319\MSBuild.exe")
  let build =
    Process.Start(
      ProcessStartInfo(
        FileName = msBuild4,
        Arguments = "Cone.sln /nologo /m /p:Configuration=Release " + args ,
        UseShellExecute = false))
  build.WaitForExit()
  build.ExitCode = 0

let package() =
    let version = AssemblyName.GetAssemblyName("Bin\Cone.dll").Version.ToString()

    use zip = new ZipFile()

    zip.AddDirectory("Bin", "Bin") |> ignore
    zip.RemoveEntry("Bin\Cone.Addin.dll") |> ignore
    zip.AddDirectory("Samples", "Samples") |> ignore
    zip.AddFile("Cone.Samples.sln") |> ignore
    zip.AddFile("Install.txt") |> ignore
    zip.Save(@"Bin\Cone-" + version + ".zip")
    true

Directory.Delete("Build", true)
Directory.Delete("Bin", true)

build ""
&& build "/p:NUnitVersion=2.5.5.10112 /t:Cone_Addin:Rebuild"
&& build "/p:NUnitVersion=2.5.7.10213 /t:Cone_Addin:Rebuild"
&& package()
