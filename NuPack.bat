@echo off
fsi BumpVersion.fsx && fsi Build.fsx && fsi NuPack.fsx && NuGet Pack

