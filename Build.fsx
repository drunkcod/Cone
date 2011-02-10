#r "Tools\DotNetZip\Ionic.Zip.Reduced.dll"
open System.Diagnostics
open System.IO
open System.Reflection
open Ionic.Zip

let NUnitPath = @"Tools\NUnit-2.5.7.10213\bin\net-2.0"

let clean what =
    what |> Seq.map (fun p -> try Directory.Delete(p, true); true with | :? DirectoryNotFoundException -> true | _ -> false) |> Seq.reduce (&&)

let build args =
  let fxPath = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory()  
  let msBuild4 = Path.Combine(fxPath, @"..\v4.0.30319\MSBuild.exe")
  let build =
    Process.Start(
      ProcessStartInfo(
        FileName = msBuild4,
        Arguments = "Cone.sln /nologo /m /v:m /p:Configuration=Release " + args ,
        UseShellExecute = false))
  build.WaitForExit()
  build.ExitCode = 0

let copyAddin() =
  let addins = Directory.CreateDirectory(Path.Combine(NUnitPath, "addins"))
  let copyToAddins source = 
      File.Copy(source, Path.Combine(addins.FullName, Path.GetFileName(source)), true)
  copyToAddins "Bin\Cone.Addin.dll"
  copyToAddins "Bin\Cone.dll"
  true

let test() =
  let nunit = 
    Process.Start(
      ProcessStartInfo(
        FileName = Path.Combine(NUnitPath, "nunit-console.exe"),
        Arguments = @"Build\Cone.Specs.dll /nologo /framework=v4.0",
        UseShellExecute = false))
  nunit.WaitForExit()
  nunit.ExitCode = 0

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

clean ["Build";"Bin"]
&& build ""
&& build "/p:NUnitVersion=2.5.5.10112 /t:Cone_Addin:Rebuild"
&& build "/p:NUnitVersion=2.5.7.10213 /t:Cone_Addin:Rebuild"
&& copyAddin()
&& test()
&& package()
