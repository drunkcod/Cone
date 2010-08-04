#r "Tools\DotNetZip\Ionic.Zip.Reduced.dll"
open System.Diagnostics
open System.IO
open Ionic.Zip

let build() =
  let fxPath = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory()  
  let msBuild4 = Path.Combine(fxPath, @"..\v4.0.30319\MSBuild.exe")
  let build =
    Process.Start(
      ProcessStartInfo(
        FileName = msBuild4,
        Arguments = "Cone.sln /nologo /m /p:Configuration=Release /p:NUnitVersion=2.5.7.10213",
        UseShellExecute = false))
  build.WaitForExit()
  build.ExitCode = 0

let package() =
    use zip = new ZipFile()

    ["Bin\Cone.dll"; "Bin\Cone.Addin.dll"]
    |> zip.AddFiles

    zip.AddDirectory("Samples", "Samples") |> ignore
    zip.AddFile("Cone.Samples.sln") |> ignore
    zip.AddFile("Install.txt") |> ignore
    zip.Save(@"Bin\Cone-0.0.1.0.zip")
    true

build() && package()
