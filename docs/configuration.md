# Configuration

The package makes all its MSBuild properties visible to the generator, so setting a property in your project file is enough. No `CompilerVisibleProperty` items are needed.

| Property | Default | Effect |
|---|---|---|
| `UseRootNamespaceForBuildInformation` | `false` | Generate the class in the project's root namespace |
| `AllowProjectDirectoryBuildOutput` | `false` | Fill `ProjectDirectory` |
| `IncludeDependenciesInBuildInformation` | `false` | Generate `DirectDependencies`, `TransitiveDependencies`, `DependenciesJson`. See [Dependencies](dependencies.md) |
| `SOURCE_DATE_EPOCH` | unset | Fixed `BuildAt` for reproducible builds |

Standard .NET properties such as `Version`, `Company` or `Nullable` flow into the generated values as well; see [Standard properties](#standard-properties).

## Root namespace

By default `BuildInformation` is in the global namespace. If two projects use the package and one can see the other's internals (typically a test project via `InternalsVisibleTo`), both `BuildInformation` classes are visible and the compiler warns about the conflict (`CS0436`). Put each class in its project's namespace:

```xml
<PropertyGroup>
    <UseRootNamespaceForBuildInformation>true</UseRootNamespaceForBuildInformation>
</PropertyGroup>
```

=== "Default"

    ```csharp
    internal static partial class BuildInformation { ... }
    ```

    ```csharp
    Console.WriteLine(BuildInformation.AssemblyName);
    ```

=== "UseRootNamespaceForBuildInformation"

    ```csharp
    namespace MyCompany.MyApp;

    internal static partial class BuildInformation { ... }
    ```

    ```csharp
    using MyCompany.MyApp;

    Console.WriteLine(BuildInformation.AssemblyName);
    ```

The namespace is `<RootNamespace>`, falling back to the project name.

## Project directory

`ProjectDirectory` is empty unless you opt in, because it embeds a local path of the build machine into your binary:

```xml
<PropertyGroup>
    <AllowProjectDirectoryBuildOutput>true</AllowProjectDirectoryBuildOutput>
</PropertyGroup>
```

=== "Default"

    ```text
    Project directory:
    ```

=== "AllowProjectDirectoryBuildOutput"

    ```text
    Project directory: /home/runner/work/MyApp/src/MyApp/
    ```

## Dependencies

Without the property, the dependency members don't exist. Opt in to embed the resolved NuGet packages and project references:

```xml
<PropertyGroup>
    <IncludeDependenciesInBuildInformation>true</IncludeDependenciesInBuildInformation>
</PropertyGroup>
```

=== "Default"

    ```csharp
    BuildInformation.DirectDependencies; // error CS0117: 'BuildInformation' does not contain a definition for 'DirectDependencies'
    ```

=== "IncludeDependenciesInBuildInformation"

    ```csharp
    internal static partial class BuildInformation
    {
        // ...
        public static readonly IReadOnlyList<Dependency> DirectDependencies = new Dependency[]
        {
            new Dependency("LinkDotNet.BuildInformation", "2.3.0"),
            new Dependency("Serilog.Sinks.Console", "6.0.0"),
        };

        public static readonly IReadOnlyList<Dependency> TransitiveDependencies = new Dependency[]
        {
            new Dependency("Serilog", "4.0.0"),
        };

        public const string DependenciesJson = "{\"direct\":[...],\"transitive\":[...]}";
    }
    ```

    ```csharp
    Console.WriteLine(string.Join(", ", BuildInformation.DirectDependencies));
    Console.WriteLine(BuildInformation.DependenciesJson);
    ```

    ```text
    LinkDotNet.BuildInformation 2.3.0, Serilog.Sinks.Console 6.0.0
    {"direct":[{"name":"LinkDotNet.BuildInformation","version":"2.3.0"},{"name":"Serilog.Sinks.Console","version":"6.0.0"}],"transitive":[{"name":"Serilog","version":"4.0.0"}]}
    ```

The lists are per target framework and contain resolved versions. See [Dependencies](dependencies.md) for what is included, use cases and troubleshooting.

## Reproducible builds (`SOURCE_DATE_EPOCH`)

`BuildAt` is the current UTC time by default, so every build produces a different assembly. Set [`SOURCE_DATE_EPOCH`](https://reproducible-builds.org/specs/source-date-epoch/) (Unix seconds) as an environment variable or MSBuild property to make it fixed, for example to the time of the last commit:

```bash
SOURCE_DATE_EPOCH=$(git log -1 --format=%ct) dotnet build
```

```text
Build at: 2026-11-20T08:14:20.0000000Z
```

Builds of the same commit then produce identical generated code. Values that are not a non-negative integer are ignored and the current time is used.

In GitHub Actions:

```yaml
- name: Build
  run: |
    export SOURCE_DATE_EPOCH=$(git log -1 --format=%ct)
    dotnet build -c Release -p:ContinuousIntegrationBuild=true
```

## Standard properties

These are not specific to this package but determine what ends up in the generated class.

### Versions

```xml
<PropertyGroup>
    <Version>2.3.0-beta.1</Version>
</PropertyGroup>
```

```text
Assembly version: 2.3.0.0
Assembly file version: 2.3.0.0
Assembly informational version: 2.3.0-beta.1+f1e7437a108da32c6b328b55c1bbf04ffae1fe64
```

`AssemblyVersion` and `FileVersion` can be set separately. The .NET SDK appends the commit SHA (`+f1e7437…`) to the informational version when source revision information is available (a git repository and .NET 8 SDK or later). To turn that off:

```xml
<PropertyGroup>
    <IncludeSourceRevisionInInformationalVersion>false</IncludeSourceRevisionInInformationalVersion>
</PropertyGroup>
```

```text
Assembly informational version: 2.3.0-beta.1
```

If you set `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>` and write no assembly attributes yourself, the version, company and copyright members are empty strings.

### Company and copyright

```xml
<PropertyGroup>
    <Company>LinkDotNet</Company>
    <Copyright>© LinkDotNet 2026</Copyright>
</PropertyGroup>
```

Quotes, backslashes and characters like `&` or `<` are escaped correctly.

### Configuration

Custom configurations work without extra setup:

```bash
dotnet build -c Staging
```

```text
Configuration: Staging
Release build: False
```

`IsReleaseBuild` reflects compiler optimization (`<Optimize>`), not the configuration name. A `Staging` configuration with `<Optimize>true</Optimize>` reports `True`.

### Multi-targeting

Each target framework is compiled separately and gets its own values:

```xml
<PropertyGroup>
    <TargetFrameworks>net8.0;net10.0</TargetFrameworks>
</PropertyGroup>
```

```text
$ dotnet run -f net8.0
net8.0
$ dotnet run -f net10.0
net10.0
```

### Other compiler settings

| Property | Member | Example |
|---|---|---|
| `<Nullable>` | `Nullability` | `Enable` |
| `<Deterministic>` | `Deterministic` | `True` |
| `<AnalysisLevel>` | `AnalysisLevel` | `latest` → the SDK's version, e.g. `11.0` |
| `<LangVersion>` | `LanguageVersion` | `preview` → the resolved preview version |
| `<WarningLevel>` | `WarningLevel` | `10` |
| `<PlatformTarget>` | `Platform` | `x64` → `X64` |
