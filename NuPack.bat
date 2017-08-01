@echo off
fsi BumpVersion.fsx && fsi Build.fsx && fsi NuPack.fsx Cone && Tools\NuGet Pack Cone.nuspec && fsi NuPack.fsx Conesole && Tools\NuGet Pack Conesole.nuspec
