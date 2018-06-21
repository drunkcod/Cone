---
layout: default
title: Getting started with dotnet conesole
---
# Getting started with dotnet conesole
_dotnet conesole is the easiest way to run specs for any platform supported by Cone._

Ensure you have both the standard Cone package and the dotnet-conesole cli packages referenced from your project file:
```xml
<ItemGroup>
    <PackageReference Include="Cone" Version="2018.6.21" /> 
    <DotNetCliToolReference Include="dotnet-conesole" Version="2018.6.21" /> 
</ItemGroup>
```

To build and run your specs, place yourself next to the relevant .csproj
ensure all packages are restored and invoke dotnet conesole.
```
$ cd <path to spec assembly>
$ dotnet restore
$ dotnet conesole
```

To avoid the build step that's done by passing `--no-build`
```
$ dotnet conesole --no-build
```

Additional parameters are passed to the runner by appending them efter a "--".
To enable multicore mode do:
```
$ dotnet conesole -- --multicore
```
Most parameters from the classic Conesole runner are supported, some useful ones:

| Option ||
|-|-|
| `--multicore`                       | Uses multiple threads to run tests.                        |
| `--include-tests=<pattern>`         | Run only tests matchingt pattern. '*' acts as wildcard.    |
| `--run-list=<file>`                 | Executes tests in specified order. One test name per line. |
| `--categories=<include>,!<exclude>` | Select categories to run.                                  |
| `--dry-run`                         | Show tests that would have run.                            |
| `--debug`                           | Attach debugger on startup.                                |
| `--labels`                          | Display test names while running.                          |
| `--test-names`                    | Display test names, sutiable for runlist                   |
| `--xml-console`                     | Output results as XML.                                     |
| `--teamcity`                        | TeamCity formatted output.                                 |
