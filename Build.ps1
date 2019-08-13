dotnet restore Cone.sln
dotnet msbuild /p:Configuration=Release

$Version = [System.Reflection.AssemblyName]::GetAssemblyName("Build\Cone\Release\net452\Cone.dll").Version.ToString(3)
Write-Host Packing Version $Version

Tools\nuget pack Cone.nuspec -Properties Configuration=Release -Version $Version -OutputDirectory Build
Tools\nuget pack Conesole.nuspec -Properties Configuration=Release -Version $Version -OutputDirectory Build
Tools\nuget pack Cone.TestAdapter.nuspec -Properties Configuration=Release -Version $Version -OutputDirectory Build
Tools\nuget pack dotnet-conesole.nuspec -Properties Configuration=Release -Version $Version -OutputDirectory Build
