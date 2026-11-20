# Limitations

What the library can't do, and what to use instead.

## Values describe the build, not the running machine

Everything is fixed at compile time. `TargetFrameworkMoniker` is the framework the code was **compiled for**, and `DotNetSdkVersion` is the runtime that **hosted the compiler**. Neither tells you what the app runs on. For that, use [`RuntimeInformation.FrameworkDescription`](https://learn.microsoft.com/dotnet/api/system.runtime.interopservices.runtimeinformation.frameworkdescription) at runtime.

`DotNetSdkVersion` reports the runtime version (e.g. `.NET 11.0.0`), not the SDK version string (`11.0.100`). Builds from Visual Studio or `MSBuild.exe` can run the compiler on .NET Framework and report that instead.

## `BuildAt` changes only when the project is compiled

If MSBuild skips compilation because nothing changed, the assembly and its `BuildAt` stay the same. `BuildAt` is the time the assembly was compiled, not the time `dotnet build` last ran.

It also makes every build produce a different binary. Use [`SOURCE_DATE_EPOCH`](configuration.md#reproducible-builds-source_date_epoch) for reproducible builds.

## No git information

There is no branch name, tag, commit date or dirty flag. The only git data is the commit SHA the .NET SDK appends to [`AssemblyInformationalVersion`](configuration.md#versions). For more, use a tool like [GitVersion](https://gitversion.net/) or [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning), whose results then flow into the version members here.

## C# only

The generator only runs for C# projects. VB.NET and F# projects are not supported. `Language` is always `C#`.

## Compiler requirement

The generator targets Roslyn 4.8 and requires a compiler at least that new: .NET SDK 8.0.100 or later, or Visual Studio 2022 17.8 or later. The project can still target any framework, including `netstandard2.0` and .NET Framework. On older compilers it is not loaded and `BuildInformation` does not exist.

## The class is `internal`

`BuildInformation` is only visible in the assembly that contains it. To read another assembly's build information, expose it there, for example through a public property, or use `InternalsVisibleTo`.

Every project with the package gets its own class. If one project sees the other's internals, use [`UseRootNamespaceForBuildInformation`](configuration.md#root-namespace) to avoid name conflicts.

## Not every member is a constant

`BuildAt`, `DirectDependencies` and `TransitiveDependencies` are `static readonly`, so they can't be used in attributes, `switch` cases or other places that need a constant.

## Dependency lists

See [What is and isn't included](dependencies.md#what-is-and-isnt-included). In short: framework references, plain DLL references and analyzer-only project references are missing; analyzer and build-tool packages are included; the output is not an SBOM.
