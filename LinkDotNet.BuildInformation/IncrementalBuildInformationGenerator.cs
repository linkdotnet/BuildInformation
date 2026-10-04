using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace LinkDotNet.BuildInformation;

[Generator]
public sealed class IncrementalBuildInformationGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var buildInformationProvider = context
            .CompilationProvider
            .Combine(context.AnalyzerConfigOptionsProvider)
            .Select(static (options, _) =>
        {
            var compiler = options.Left;
            var analyzer = options.Right;

            analyzer.GlobalOptions.TryGetValue("build_property.TargetFramework", out var targetFrameworkValue);
            var nullability = compiler.Options.NullableContextOptions.ToString();
            analyzer.GlobalOptions.TryGetValue("build_property.Configuration", out var configuration);
            configuration ??= compiler.Options.OptimizationLevel.ToString();

            var assembly = compiler.Assembly;
            var rootNamespace = GetRootNamespace(analyzer);
            analyzer.GlobalOptions.TryGetValue("build_property.effectiveanalysislevelstyle", out var analysisLevel);
            var projectDirectory = GetProjectDirectory(analyzer);
            var isReleaseBuild = compiler.Options.OptimizationLevel == OptimizationLevel.Release;
            analyzer.GlobalOptions.TryGetValue("build_property.SOURCE_DATE_EPOCH", out var sourceDateEpoch);

            return new BuildInformationInfo
            (
                Platform: compiler.Options.Platform.ToString(),
                WarningLevel: compiler.Options.WarningLevel,
                Configuration: configuration,
                AssemblyVersion: GetAssemblyVersion(assembly) ?? string.Empty,
                AssemblyFileVersion: GetAssemblyFileVersion(assembly) ?? string.Empty,
                AssemblyInformationalVersion: GetAssemblyInformationalVersion(assembly) ?? string.Empty,
                AssemblyName: assembly.Name,
                AssemblyCopyright: GetAssemblyCopyright(assembly) ?? string.Empty,
                AssemblyCompany: GetAssemblyCompany(assembly) ?? string.Empty,
                TargetFrameworkMoniker: targetFrameworkValue ?? string.Empty,
                Nullability: nullability,
                Deterministic: compiler.Options.Deterministic,
                RootNamespace: rootNamespace,
                AnalysisLevel: analysisLevel ?? string.Empty,
                ProjectDirectory: projectDirectory,
                Language: CSharpParseOptions.Default.Language,
                LanguageVersion: ((CSharpCompilation)compiler).LanguageVersion.ToDisplayString(),
                IsReleaseBuild: isReleaseBuild,
                CompilerVersion: typeof(CSharpCompilation).Assembly.GetName().Version?.ToString() ?? "Unknown",
                DotNetSdkVersion: GetDotNetSdkVersion(),
                SourceDateEpoch: sourceDateEpoch
            );
        });

        // BuildAt is excluded from the cached model so edits that don't change build info don't regenerate the source.
        context.RegisterSourceOutput(buildInformationProvider, static (productionContext, buildInformation) =>
            productionContext.AddSource("LinkDotNet.BuildInformation.g", buildInformation.GenerateBuildInformation(GetBuildAt(buildInformation.SourceDateEpoch))));
    }
    
    private static string? GetAssemblyFileVersion(ISymbol assembly)
    {
        var assemblyFileVersionAttribute = assembly.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass?.Name == nameof(AssemblyFileVersionAttribute));
        var assemblyFileVersion = assemblyFileVersionAttribute is not null
            ? assemblyFileVersionAttribute.ConstructorArguments[0].Value!.ToString()
            : string.Empty;
        return assemblyFileVersion;
    }

    private static string? GetAssemblyVersion(ISymbol assembly)
    {
        var assemblyVersionAttribute = assembly.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass?.Name == nameof(AssemblyVersionAttribute));
        var assemblyVersion = assemblyVersionAttribute is not null
            ? assemblyVersionAttribute.ConstructorArguments[0].Value!.ToString()
            : string.Empty;
        return assemblyVersion;
    }
    
    private static string? GetAssemblyCopyright(ISymbol assembly)
    {
        var assemblyCopyrightAttribute = assembly.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass?.Name == nameof(AssemblyCopyrightAttribute));
        var assemblyCopyright = assemblyCopyrightAttribute is not null
            ? assemblyCopyrightAttribute.ConstructorArguments[0].Value!.ToString()
            : string.Empty;
        return assemblyCopyright;
    }
    
    private static string? GetAssemblyCompany(ISymbol assembly)
    {
        var assemblyCompanyAttribute = assembly.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass?.Name == nameof(AssemblyCompanyAttribute));
        var assemblyCompany = assemblyCompanyAttribute is not null
            ? assemblyCompanyAttribute.ConstructorArguments[0].Value!.ToString()
            : string.Empty;
        return assemblyCompany;
    }

    private static string? GetAssemblyInformationalVersion(ISymbol assembly)
    {
        var assemblyInformationalVersionAttribute = assembly.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass?.Name == nameof(AssemblyInformationalVersionAttribute));
        var assemblyInformationalVersion = assemblyInformationalVersionAttribute is not null
            ? assemblyInformationalVersionAttribute.ConstructorArguments[0].Value!.ToString()
            : string.Empty;
        return assemblyInformationalVersion;
    }

    private static string GetDotNetSdkVersion()
    {
        return RuntimeInformation.FrameworkDescription;
    }
    
    private static string GetRootNamespace(AnalyzerConfigOptionsProvider analyzer)
    {
        analyzer.GlobalOptions.TryGetValue("build_property.UseRootNamespaceForBuildInformation", out var useRootNamespaceValue);
        var useRootNamespace = useRootNamespaceValue?.Equals("true", StringComparison.InvariantCultureIgnoreCase) ?? false;
        if (!useRootNamespace)
        {
            return string.Empty;
        }

        if (!analyzer.GlobalOptions.TryGetValue("build_property.RootNamespace", out var rootNamespaceValue))
        {
            analyzer.GlobalOptions.TryGetValue("build_property.MSBuildProjectName", out rootNamespaceValue);
            return rootNamespaceValue ?? string.Empty;
        }
        
        return rootNamespaceValue;
    }

    private static string GetProjectDirectory(AnalyzerConfigOptionsProvider analyzer)
    {
        analyzer.GlobalOptions.TryGetValue("build_property.AllowProjectDirectoryBuildOutput", out var allowOutput);
        if (!allowOutput?.Equals("true", StringComparison.InvariantCultureIgnoreCase) ?? true)
        {
            return SyntaxFactory.Literal(string.Empty).ToString();
        }


        return !analyzer.GlobalOptions.TryGetValue("build_property.projectDir", out var projectDir)
            ? SyntaxFactory.Literal(string.Empty).ToString()
            : SyntaxFactory.Literal(projectDir).ToString();
    }
    
    // SOURCE_DATE_EPOCH: https://reproducible-builds.org/specs/source-date-epoch/
    private static string GetBuildAt(string? sourceDateEpoch)
    {
        var buildAt = long.TryParse(sourceDateEpoch, NumberStyles.None, CultureInfo.InvariantCulture, out var seconds)
            ? DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime
            : DateTime.UtcNow;
        return buildAt.ToString("O");
    }

    private static string Literal(string value) => SyntaxFactory.Literal(value).ToString();

    private static string Xml(object value) => System.Security.SecurityElement.Escape(value.ToString());

    private sealed record BuildInformationInfo(
        string Platform,
        int WarningLevel,
        string Configuration,
        string AssemblyVersion,
        string AssemblyFileVersion,
        string AssemblyInformationalVersion,
        string AssemblyName,
        string AssemblyCopyright,
        string AssemblyCompany,
        string TargetFrameworkMoniker,
        string Nullability,
        bool Deterministic,
        string RootNamespace,
        string AnalysisLevel,
        string ProjectDirectory,
        string Language,
        string LanguageVersion,
        bool IsReleaseBuild,
        string CompilerVersion,
        string DotNetSdkVersion,
        string? SourceDateEpoch
    )
    {
        public string GenerateBuildInformation(string buildAt)
        {
            var rootNamespace = string.IsNullOrEmpty(RootNamespace)
            ? string.Empty
            : $"\nnamespace {RootNamespace};\n";
        return $$"""
                 // <auto-generated>
                 // This file was generated by the LinkDotNet.BuildInformation package.
                 //
                 // Changes to this file may cause incorrect behavior and will be lost if
                 // the code is regenerated.
                 // </auto-generated>

                 using System;
                 using System.Globalization;
                 {{rootNamespace}}
                 internal static partial class BuildInformation
                 {
                     /// <summary>
                     /// Returns the build date (UTC) in ISO 8601 format.
                     /// </summary>
                     /// <remarks>Value is: {{Xml(buildAt)}}</remarks>
                     public static readonly DateTime BuildAt = DateTime.ParseExact("{{buildAt}}", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                 
                     /// <summary>
                     /// Returns the platform.
                     /// </summary>
                     /// <remarks>Value is: {{Xml(Platform)}}</remarks>
                     public const string Platform = {{Literal(Platform)}};
                 
                     /// <summary>
                     /// Returns the warning level.
                     /// </summary>
                     /// <remarks>Value is: {{Xml(WarningLevel)}}</remarks>
                     public const int WarningLevel = {{WarningLevel}};
                 
                     /// <summary>
                     /// Returns the configuration.
                     /// </summary>
                     /// <remarks>Value is: {{Xml(Configuration)}}</remarks>
                     public const string Configuration = {{Literal(Configuration)}};
                 
                     /// <summary>
                     /// Returns the assembly version.
                     /// </summary>
                     /// <remarks>Value is: {{Xml(AssemblyVersion)}}</remarks>
                     public const string AssemblyVersion = {{Literal(AssemblyVersion)}};
                 
                     /// <summary>
                     /// Returns the assembly file version.
                     /// </summary>
                     /// <remarks>Value is: {{Xml(AssemblyFileVersion)}}</remarks>
                     public const string AssemblyFileVersion = {{Literal(AssemblyFileVersion)}};

                     /// <summary>
                     /// Returns the assembly informational version (e.g. 1.2.3-beta+abc123).
                     /// </summary>
                     /// <remarks>Value is: {{Xml(AssemblyInformationalVersion)}}</remarks>
                     public const string AssemblyInformationalVersion = {{Literal(AssemblyInformationalVersion)}};

                     /// <summary>
                     /// Returns the assembly name.
                     /// </summary>
                     /// <remarks>Value is: {{Xml(AssemblyName)}}</remarks>
                     public const string AssemblyName = {{Literal(AssemblyName)}};
                 
                     /// <summary>
                     /// Returns the assembly copyright.
                     /// </summary>
                     /// <remarks>Value is: {{Xml(AssemblyCopyright)}}</remarks>
                     public const string AssemblyCopyright = {{Literal(AssemblyCopyright)}};
                 
                     /// <summary>
                     /// Returns the assembly company.
                     /// </summary>
                     /// <remarks>Value is: {{Xml(AssemblyCompany)}}</remarks>
                     public const string AssemblyCompany = {{Literal(AssemblyCompany)}};
                 
                     /// <summary>
                     /// Returns the target framework moniker.
                     /// </summary>
                     /// <remarks>Value is: {{Xml(TargetFrameworkMoniker)}}</remarks>
                     public const string TargetFrameworkMoniker = {{Literal(TargetFrameworkMoniker)}};
                 
                     /// <summary>
                     /// Returns the nullability level.
                     /// </summary>
                     /// <remarks>Value is: {{Xml(Nullability)}}</remarks>
                     public const string Nullability = {{Literal(Nullability)}};
                 
                     /// <summary>
                     /// Returns whether the build is deterministic.
                     /// </summary>
                     /// <remarks>Value is: {{Deterministic.ToString().ToLowerInvariant()}}</remarks>
                     public const bool Deterministic = {{Deterministic.ToString().ToLowerInvariant()}};
                     
                     /// <summary>
                     /// Returns the Analysis level of the application.
                     /// </summary>
                     /// <remarks>Value is: {{Xml(AnalysisLevel)}}</remarks>
                     public const string AnalysisLevel = {{Literal(AnalysisLevel)}};
                     
                     /// <summary>
                     /// Returns the project directory.
                     /// </summary>
                     /// <remarks>Value is: {{Xml(ProjectDirectory)}}</remarks>
                     public const string ProjectDirectory = {{ProjectDirectory}};
                     
                     /// <summary>
                     /// Returns the language the code is compiled against (like C# or F#).
                     /// </summary>
                     /// <example>C#</example>
                     /// <remarks>Value is {{Xml(Language)}}</remarks>
                     public const string Language = {{Literal(Language)}};
                     
                     /// <summary>
                     /// Returns the language version the code is compiled against. This is only the version (like 12.0).
                     /// </summary>
                     /// <example>12.0</example>
                     /// <remarks>Value is {{Xml(LanguageVersion)}}</remarks>
                     public const string LanguageVersion = {{Literal(LanguageVersion)}};
                     
                     /// <summary>
                     /// Returns whether the build is in Release mode.
                     /// </summary>
                     /// <remarks>Value is: {{IsReleaseBuild.ToString().ToLowerInvariant()}}</remarks>
                     public const bool IsReleaseBuild = {{IsReleaseBuild.ToString().ToLowerInvariant()}};
                     
                     /// <summary>
                     /// Returns the Roslyn/C# compiler version used during the build.
                     /// </summary>
                     /// <remarks>Value is: {{Xml(CompilerVersion)}}</remarks>
                     public const string CompilerVersion = {{Literal(CompilerVersion)}};
                     
                     /// <summary>
                     /// Returns the .NET runtime/framework version on which the build occurred (.NET SDK version).
                     /// This can differ from <see cref="TargetFrameworkMoniker"/> which indicates the target framework for which the code will run.
                     /// </summary>
                     /// <remarks>Value is: {{Xml(DotNetSdkVersion)}}</remarks>
                     public const string DotNetSdkVersion = {{Literal(DotNetSdkVersion)}};
                 }
                 """;
        }
    }
}
