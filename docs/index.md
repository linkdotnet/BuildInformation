# LinkDotNet.BuildInformation

A C# source generator that writes a `BuildInformation` class into your project at compile time. It records facts about the build that produced the assembly, such as when, how and with which compiler it was built.

The values are fixed when the assembly is compiled. They are not read from the machine at runtime.

## Quick start

Install the package in an SDK-style C# project:

```bash
dotnet add package LinkDotNet.BuildInformation
```

Build once, then use the generated class. It lives in the global namespace by default, so no `using` is needed:

```csharp
Console.WriteLine($"Version {BuildInformation.AssemblyInformationalVersion}");
Console.WriteLine($"Built {BuildInformation.BuildAt:O} ({BuildInformation.Configuration})");
Console.WriteLine($"Target {BuildInformation.TargetFrameworkMoniker}, C# {BuildInformation.LanguageVersion}");
```

```text
Version 1.0.0+f1e7437a108da32c6b328b55c1bbf04ffae1fe64
Built 2026-11-20T08:19:26.4210020Z (Debug)
Target net10.0, C# 14.0
```

## What you get

| Area | Members |
|---|---|
| When | `BuildAt` |
| How | `Configuration`, `IsReleaseBuild`, `Platform`, `WarningLevel`, `Nullability`, `Deterministic`, `AnalysisLevel` |
| What | `AssemblyName`, `AssemblyVersion`, `AssemblyFileVersion`, `AssemblyInformationalVersion`, `AssemblyCompany`, `AssemblyCopyright`, `TargetFrameworkMoniker` |
| Toolchain | `Language`, `LanguageVersion`, `CompilerVersion`, `DotNetSdkVersion` |
| Opt-in | `ProjectDirectory`, `DirectDependencies`, `TransitiveDependencies`, `DependenciesJson` |

See [Generated members](reference.md) for types and example values, and [Configuration](configuration.md) for the MSBuild properties that change the output.

## Full example output

Output of the [sample project](https://github.com/linkdotnet/BuildInformation/tree/main/LinkDotNet.BuildInformation.Sample). Values depend on your build environment:

```text
Build at: 2026-11-20T08:19:26.4210020Z
Platform: AnyCpu
Warning level: 10
Configuration: Debug
Assembly version: 1.0
Assembly file version: 1.2
Assembly informational version: 1.0.0+f1e7437a108da32c6b328b55c1bbf04ffae1fe64
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
Compiler version: 5.12.0.0
DotNet SDK version: .NET 11.0.0
Release build: False
```

!!! tip "See the values in your IDE"
    Most generated members carry their value in the XML documentation. Hover over `BuildInformation.Configuration` in your IDE to see what the current build produced.

## Why a source generator?

- **No runtime dependency.** The package is an analyzer; nothing ships with your app.
- **No reflection.** Almost all members are `const`, so they cost nothing at runtime and can be used in `switch` cases and attributes.
- **No custom MSBuild targets** to copy around. The package brings its own `buildTransitive` props.
