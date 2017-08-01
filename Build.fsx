#r "Tools\DotNetZip\Ionic.Zip.Reduced.dll"
open System
open System.Diagnostics
open System.IO
open System.Reflection
open System.Linq
open Ionic.Zip

let clean what =
    what 
    |> Seq.map (fun p -> try Directory.Delete(p, true); true with | :? DirectoryNotFoundException -> true | _ -> false) 
    |> Seq.reduce (&&)

let msBuild =
  let searchRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "MSBuild")
  Directory.GetFiles(searchRoot, "MSBuild.exe",SearchOption.AllDirectories).OrderByDescending(fun x -> x).First()

let build proj args =
  use build =
    Process.Start(
      ProcessStartInfo(
        FileName = msBuild,
        Arguments = proj + " /nologo /v:m /p:Configuration=Release " + args,
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
&& build "Source\Cone\Cone.csproj" "/p:SolutionDir=..\..\;TargetFramework=net45"
&& build "Source\Cone\Cone.csproj" "/p:SolutionDir=..\..\;TargetFramework=net452"
&& build "Source\Cone\Cone.csproj" "/p:SolutionDir=..\..\;TargetFramework=net462"
&& build "Source\Cone\Cone.csproj" "/p:SolutionDir=..\..\;TargetFramework=net47"
&& build "Source\Conesole\Conesole.csproj" "/p:SolutionDir=..\..\;TargetFramework=net45"
&& build "Source\Conesole\Conesole.csproj" "/p:SolutionDir=..\..\;TargetFramework=net452"
&& build "Source\Conesole\Conesole.csproj" "/p:SolutionDir=..\..\;TargetFramework=net462"
&& build "Source\Conesole\Conesole.csproj" "/p:SolutionDir=..\..\;TargetFramework=net47"
&& build "Cone.sln" "/p:TargetFramework=net45"
&& package()
