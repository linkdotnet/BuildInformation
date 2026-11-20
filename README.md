# Incremental Build Information Generator

[![.NET](https://github.com/linkdotnet/BuildInformation/actions/workflows/dotnet.yml/badge.svg)](https://github.com/linkdotnet/BuildInformation/actions/workflows/dotnet.yml)
[![Nuget](https://img.shields.io/nuget/dt/LinkDotNet.BuildInformation)](https://www.nuget.org/packages/LinkDotNet.BuildInformation/)
[![GitHub tag](https://img.shields.io/github/v/tag/linkdotnet/BuildInformation?include_prereleases&logo=github&style=flat-square)](https://github.com/linkdotnet/BuildInformation/releases)

This project provides a C# source generator that embeds build information in a `BuildInformation` class at compile time. The values describe the build that produced the assembly; they are not read from the machine at runtime.

## Features
* Embeds build date (in UTC) in your code. Honors [`SOURCE_DATE_EPOCH`](https://reproducible-builds.org/specs/source-date-epoch/) for reproducible builds
* Embeds platform (AnyCPU, x86, x64, ...) information in your code
* Embeds compiler warning level in your code
* Embeds build configuration (e.g., Debug, Release) in your code
* Embeds assembly version, file version, informational version, name, company, and copyright
* Embeds the target framework moniker in your code
* Embeds the nullable-context setting, analysis level, and deterministic-build flag
* Embeds the project directory when explicitly enabled (disabled by default to avoid exposing local paths)
* Embeds the language and configured language version (like "C#" and "12.0")
* Embeds whether the build uses Release optimization, plus the Roslyn compiler and .NET SDK runtime versions
* Optionally embeds the direct and transitive dependencies (name and version)

The generator is an analyzer and adds no runtime dependency. The generated `BuildInformation` class is `internal` by default, in the global namespace, and its members are public.

## Configuration
The NuGet package makes all needed MSBuild properties visible to the generator automatically, so setting the property is enough.

To put the generated class in the project's root namespace (useful when multiple projects reference each other), add this to your project file:
```xml
<PropertyGroup>
    <UseRootNamespaceForBuildInformation>true</UseRootNamespaceForBuildInformation>
</PropertyGroup>
```

With this option enabled, include the root namespace in your `using` directive.

The `AllowProjectDirectoryBuildOutput` property includes the project directory in the generated class. It is disabled by default because local paths may be sensitive.
```xml
<PropertyGroup>
    <AllowProjectDirectoryBuildOutput>true</AllowProjectDirectoryBuildOutput>
</PropertyGroup>
```

### Dependencies
Set `IncludeDependenciesInBuildInformation` to embed the resolved direct and transitive dependencies (NuGet packages and project references) of the current target framework:
```xml
<PropertyGroup>
    <IncludeDependenciesInBuildInformation>true</IncludeDependenciesInBuildInformation>
</PropertyGroup>
```

```csharp
foreach (var dependency in BuildInformation.DirectDependencies)
{
    Console.WriteLine($"{dependency.Name} {dependency.Version}");
}

foreach (var dependency in BuildInformation.TransitiveDependencies)
{
    Console.WriteLine($"{dependency.Name} {dependency.Version}");
}

// {"direct":[{"name":"Serilog.Sinks.Console","version":"6.0.0"}],"transitive":[{"name":"Serilog","version":"4.0.0"}]}
Console.WriteLine(BuildInformation.DependenciesJson);
```

#### How it works
When enabled, the package adds the project's `project.assets.json` (written by `dotnet restore`) as an `AdditionalFiles` item. The generator reads the entry for the current target framework: every resolved package or project is a dependency, and the ones declared in the project file are the direct ones; everything else is transitive. Versions are the resolved versions, not the ranges from the project file.

Everything is computed at compile time, so there is no runtime cost and `DependenciesJson` is a `const`. The generator step costs a few milliseconds per compilation (about 1 ms for ~50 packages and about 7 ms for a 770 KB assets file with 4 target frameworks), and in the IDE it only runs again when the assets file changes.

The feature is opt-in because the exact package versions are embedded in your assembly, where anyone can read them and look up known vulnerabilities. It also adds a string for every dependency to the binary. `DependenciesJson` is a lightweight list and not a standardized SBOM; use tools like [CycloneDX](https://github.com/CycloneDX/cyclonedx-dotnet) or [Microsoft.Sbom.Targets](https://github.com/microsoft/sbom-tool) for CycloneDX or SPDX documents.

### Reproducible builds
If the `SOURCE_DATE_EPOCH` environment variable (or MSBuild property) is set to a Unix timestamp, it is used for `BuildAt` instead of the current time. This makes the generated code identical across builds of the same commit:
```bash
SOURCE_DATE_EPOCH=$(git log -1 --format=%ct) dotnet build
```

## Usage
Install the NuGet package in an SDK-style C# project:

```no-class
dotnet add package LinkDotNet.BuildInformation
```

Build the project, then use the generated class in your code. With the default global namespace, no namespace import is needed:
```csharp
using System;

Console.WriteLine($"Build at: {BuildInformation.BuildAt}");
Console.WriteLine($"Platform: {BuildInformation.Platform}");
Console.WriteLine($"Warning level: {BuildInformation.WarningLevel}");
Console.WriteLine($"Configuration: {BuildInformation.Configuration}");
Console.WriteLine($"Assembly version: {BuildInformation.AssemblyVersion}");
Console.WriteLine($"Assembly file version: {BuildInformation.AssemblyFileVersion}");
Console.WriteLine($"Assembly informational version: {BuildInformation.AssemblyInformationalVersion}");
Console.WriteLine($"Assembly name: {BuildInformation.AssemblyName}");
Console.WriteLine($"Assembly copyright: {BuildInformation.AssemblyCopyright}");
Console.WriteLine($"Assembly company: {BuildInformation.AssemblyCompany}");
Console.WriteLine($"Target framework moniker: {BuildInformation.TargetFrameworkMoniker}");
Console.WriteLine($"Nullability: {BuildInformation.Nullability}");
Console.WriteLine($"Deterministic build: {BuildInformation.Deterministic}");
Console.WriteLine($"Analysis level: {BuildInformation.AnalysisLevel}");
Console.WriteLine($"Project directory: {BuildInformation.ProjectDirectory}");
Console.WriteLine($"Language: {BuildInformation.Language}");
Console.WriteLine($"Language version: {BuildInformation.LanguageVersion}");
Console.WriteLine($"Compiler version: {BuildInformation.CompilerVersion}");
Console.WriteLine($"DotNet SDK version: {BuildInformation.DotNetSdkVersion}");
Console.WriteLine($"Release build: {BuildInformation.IsReleaseBuild}");
```

The [sample project](LinkDotNet.BuildInformation.Sample) shows all generated values and opts into the root namespace and project directory. It references the generator project directly for local development; package consumers only need the NuGet package above. Hover over the properties in an IDE to see their generated XML documentation.

An example output could look like this (values depend on the build environment):
```no-class
Build at: 2026-10-31T20:59:26.0000000Z
Platform: AnyCpu
Warning level: 10
Configuration: Debug
Assembly version: 1.0
Assembly file version: 1.2
Assembly informational version: 1.0.0+da778dfd0f233100b6cbaf0689f8f8d35f60b975
Assembly name: LinkDotNet.BuildInformation.Sample
Assembly copyright: LinkDotNet 2025
Assembly company: LinkDotNet
Target framework moniker: net10.0
Nullability: Enable
Deterministic build: True
Analysis level: 10.0
Project directory: /path/to/LinkDotNet.BuildInformation.Sample/
Language: C#
Language version: 14.0
Compiler version: 5.9.0.0
DotNet SDK version: .NET 10.0.14
Release build: False
```

The .NET SDK appends the commit SHA to `AssemblyInformationalVersion` when source revision information is available. To disable that behavior, set `<IncludeSourceRevisionInInformationalVersion>false</IncludeSourceRevisionInInformationalVersion>` in your project.

## Contributing
If you would like to contribute to the project, please submit a pull request or open an issue on the project's GitHub page. We welcome any feedback, bug reports, or feature requests.

## License
This project is licensed under the MIT License.