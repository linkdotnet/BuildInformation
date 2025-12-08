# Incremental Build Information Generator

[![.NET](https://github.com/linkdotnet/BuildInformation/actions/workflows/dotnet.yml/badge.svg)](https://github.com/linkdotnet/BuildInformation/actions/workflows/dotnet.yml)
[![Nuget](https://img.shields.io/nuget/dt/LinkDotNet.BuildInformation)](https://www.nuget.org/packages/LinkDotNet.BuildInformation/)
[![GitHub tag](https://img.shields.io/github/v/tag/linkdotnet/BuildInformation?include_prereleases&logo=github&style=flat-square)](https://github.com/linkdotnet/BuildInformation/releases)

This project provides a simple and easy-to-use C# source generator that embeds build information, such as the build time, platform, warning level, and configuration, directly into your code. By using the `BuildInformation` class, you can quickly access and display these details.

## Features
* Embeds build date (in UTC) in your code
* Embeds platform (AnyCPU, x86, x64, ...) information in your code
* Embeds compiler warning level in your code
* Embeds build configuration (e.g., Debug, Release) in your code
* Embeds the assembly version and assembly file version in your code
* Embeds the target framework moniker in your code
* Embeds the nullability analysis level in your code
* Embeds the deterministic build flag in your code
* Embeds the project path where the generator is running in your code
* Embeds the language and language version (like "C#" and "12.0")

## Configuration
By default the created class is `internal` and is not under any namespace. This can be changed by adding the following to your project file:
```xml
<PropertyGroup>
    <UseRootNamespaceForBuildInformation>true</UseRootNamespaceForBuildInformation>
</PropertyGroup>

<ItemGroup>
    <CompilerVisibleProperty Include="UseRootNamespaceForBuildInformation" />
</ItemGroup>
```

This will use the root namespace of the project for the generated class. This is especially helpful if the generator is used in multiple projects, that might be visible to each other.

The `AllowProjectDirectoryBuildOutput` property is used to allow the generator to write the project directory into the generated class. This is disabled by default, as it might leak sensitive information.
```xml
<PropertyGroup>
    <AllowProjectDirectoryBuildOutput>true</AllowProjectDirectoryBuildOutput>
</PropertyGroup>

<ItemGroup>
    <CompilerVisibleProperty Include="AllowProjectDirectoryBuildOutput" />
</ItemGroup>
```

## Usage
To use the `BuildInformation` class in your project, add the NuGet package:

```no-class
dotnet add package LinkDotNet.BuildInformation
```

Here is some code how to use the class:
```csharp
using System;

Console.WriteLine($"Build at: {BuildInformation.BuildAt}");
Console.WriteLine($"Platform: {BuildInformation.Platform}");
Console.WriteLine($"Warning level: {BuildInformation.WarningLevel}");
Console.WriteLine($"Configuration: {BuildInformation.Configuration}");
Console.WriteLine($"Assembly version: {BuildInformation.AssemblyVersion}");
Console.WriteLine($"Assembly file version: {BuildInformation.AssemblyFileVersion}");
Console.WriteLine($"Assembly name: {BuildInformation.AssemblyName}");
Console.WriteLine($"Assembly copyright: {BuildInformation.AssemblyCopyright}");
Console.WriteLine($"Assembly company: {BuildInformation.AssemblyCompany}");
Console.WriteLine($"Target framework moniker: {BuildInformation.TargetFrameworkMoniker}");
Console.WriteLine($"Analysis level: {BuildInformation.Nullability}");
Console.WriteLine($"Deterministic build: {BuildInformation.Deterministic}");
Console.WriteLine($"Analysis level: {BuildInformation.AnalysisLevel}");
Console.WriteLine($"Project directory: {BuildInformation.ProjectDirectory}");
Console.WriteLine($"Language: {BuildInformation.Language}");
Console.WriteLine($"Language version: {BuildInformation.LanguageVersion}");
Console.WriteLine($"Compiler version: {BuildInformation.CompilerVersion}");
Console.WriteLine($"DotNet SDK version: {BuildInformation.DotNetSdkVersion}");
```

You can also hover over the properties to get the currently held value (xmldoc support). An example output could look like this:
```no-class
Build at: 08/12/2025 11:51:53
Platform: AnyCpu
Warning level: 8
Configuration: Release
Assembly version: 1.0
Assembly file version: 1.2
Assembly name: LinkDotNet.BuildInformation.Sample
Assembly copyright: LinkDotNet 2025
Assembly company: LinkDotNet
Target framework moniker: net8.0
Analysis level: Enable
Deterministic build: True
Analysis level: 8.0
Project directory: /Users/example/repo/this/Sample.csproj
Language: C#
Language version: 14.0
Compiler version: 5.0.0.0
DotNet SDK version: .NET 10.0.0
```

## Contributing
If you would like to contribute to the project, please submit a pull request or open an issue on the project's GitHub page. We welcome any feedback, bug reports, or feature requests.

## License
This project is licensed under the MIT License.