dotnet msbuild /p:Configuration=Release
#dotnet Build\dotnet-conesole\Release\netcoreapp2.0\dotnet-conesole.dll Build\Cone.Specs\Release\net45\Cone.Specs.dll

$Version = [System.Reflection.AssemblyName]::GetAssemblyName("Build\Cone\Release\net45\Cone.dll").Version.ToString(3)
Write-Host Packing Version $Version

Tools\nuget pack Cone.nuspec -Properties Configuration=Release -Version $Version -OutputDirectory Build
Tools\nuget pack Conesole.nuspec -Properties Configuration=Release -Version $Version -OutputDirectory Build
Tools\nuget pack dotnet-conesole.nuspec -Properties Configuration=Release -Version $Version -OutputDirectory Build