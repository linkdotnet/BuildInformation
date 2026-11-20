# Dependencies

Since 2.3.0, the generator can embed the resolved NuGet packages and project references of your project. This is opt-in.

```xml
<PropertyGroup>
    <IncludeDependenciesInBuildInformation>true</IncludeDependenciesInBuildInformation>
</PropertyGroup>
```

## Example

```xml title="MyApp.csproj"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <IncludeDependenciesInBuildInformation>true</IncludeDependenciesInBuildInformation>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="LinkDotNet.BuildInformation" Version="2.3.0" PrivateAssets="all" />
    <PackageReference Include="Serilog.Sinks.Console" Version="6.0.0" />
  </ItemGroup>
</Project>
```

```csharp title="Program.cs"
foreach (var dependency in BuildInformation.DirectDependencies)
{
    Console.WriteLine($"direct: {dependency.Name} {dependency.Version}");
}

foreach (var dependency in BuildInformation.TransitiveDependencies)
{
    Console.WriteLine($"transitive: {dependency}");
}

Console.WriteLine(BuildInformation.DependenciesJson);
```

```text title="Output"
direct: LinkDotNet.BuildInformation 2.3.0
direct: Serilog.Sinks.Console 6.0.0
transitive: Serilog 4.0.0
{"direct":[{"name":"LinkDotNet.BuildInformation","version":"2.3.0"},{"name":"Serilog.Sinks.Console","version":"6.0.0"}],"transitive":[{"name":"Serilog","version":"4.0.0"}]}
```

`Serilog` is transitive because the project only references `Serilog.Sinks.Console`, which depends on it.

## Use cases

- An `/about` or `/health` endpoint that shows what a deployed build contains:

    ```csharp
    app.MapGet("/about", () => Results.Content(BuildInformation.DependenciesJson, "application/json"));
    ```

- Logging the dependency list at startup to correlate bug reports with package versions.
- Checking in a test that a package was not pulled in by accident:

    ```csharp
    Assert.DoesNotContain(BuildInformation.TransitiveDependencies, d => d.Name == "Newtonsoft.Json");
    ```

## How it works

When the property is `true`, the package adds the project's `project.assets.json` (written by `dotnet restore` into `obj/`) as an `AdditionalFiles` item. The generator reads the section for the current target framework:

- **Every** resolved package or project in that section is a dependency.
- A dependency is **direct** if it is declared in the project file (`PackageReference` or `ProjectReference`); everything else is **transitive**.
- Versions are the **resolved** versions, not the ranges from the project file. `Version="6.*"` shows up as the version NuGet picked, e.g. `6.0.0`.

Everything is computed at compile time: there is no runtime cost and `DependenciesJson` is a `const`. The generator step takes a few milliseconds per compilation (about 1 ms for ~50 packages, about 7 ms for a 770 KB assets file with 4 target frameworks). In the IDE it only runs again when the assets file changes.

With multi-targeting, each target framework gets its own list.

## What is and isn't included

| Reference | Included? |
|---|---|
| `PackageReference` | Yes |
| `PackageReference` with `PrivateAssets="all"` (analyzers, build tools, this package) | Yes |
| `ProjectReference` | Yes, with the project's version |
| `ProjectReference` with `ReferenceOutputAssembly="false"` (e.g. a local analyzer) | No |
| `FrameworkReference` (e.g. `Microsoft.AspNetCore.App`) and the shared framework | No |
| `Reference` to a plain DLL | No |
| Packages from `packages.config` projects | No |

Development-only packages appear in the list because NuGet resolves them like any other package. Filter them yourself if you only want runtime dependencies.

## Why it is opt-in

- **Security.** Exact package versions end up in your assembly, where anyone can read them and look up known vulnerabilities.
- **Size.** Every dependency adds strings to the binary.

## Not an SBOM

`DependenciesJson` is a lightweight name/version list. It has no hashes, licenses or package URLs and follows no standard format. For CycloneDX or SPDX documents use [CycloneDX for .NET](https://github.com/CycloneDX/cyclonedx-dotnet) or [Microsoft.Sbom.Targets](https://github.com/microsoft/sbom-tool).

## Troubleshooting

!!! warning "Lists are empty"
    - Run `dotnet restore`; without `obj/project.assets.json` there is nothing to read.
    - With the .NET 8 SDK and multi-targeting, the assets file uses a format the generator does not match, so the lists are empty. Use the .NET 9 SDK or later, or a single target framework.
    - Check that the property value is exactly `true`.
