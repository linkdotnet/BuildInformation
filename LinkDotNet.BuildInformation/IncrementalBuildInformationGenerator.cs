using System.Diagnostics;
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
        var compilationAndOptionsProvider = context
            .CompilationProvider
            .Combine(context.AnalyzerConfigOptionsProvider)
            .Select((s, _) => s);

        context.RegisterSourceOutput(compilationAndOptionsProvider, static (productionContext, options) =>
        {
            var compiler = options.Left;
            var analyzer = options.Right;

            analyzer.GlobalOptions.TryGetValue("build_property.TargetFramework", out var targetFrameworkValue);
            var nullability = compiler.Options.NullableContextOptions.ToString();
            var configuration =
                Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration ??
                compiler.Options.OptimizationLevel.ToString();

            var assembly = compiler.Assembly;
            var rootNamespace = GetRootNamespace(analyzer);
            analyzer.GlobalOptions.TryGetValue("build_property.effectiveanalysislevelstyle", out var analysisLevel);
            var projectDirectory = GetProjectDirectory(analyzer);
            
            var buildAtUtc = DateTime.UtcNow;
            var buildAtIso = buildAtUtc.ToString("O");
            var isReleaseBuild = compiler.Options.OptimizationLevel == OptimizationLevel.Release;

            var buildInformation = new BuildInformationInfo
            {
                BuildAt = buildAtIso,
                Platform = compiler.Options.Platform.ToString(),
                WarningLevel = compiler.Options.WarningLevel,
                Configuration = configuration,
                AssemblyVersion = GetAssemblyVersion(assembly) ?? string.Empty,
                AssemblyFileVersion = GetAssemblyFileVersion(assembly) ?? string.Empty,
                AssemblyName = assembly.Name,
                AssemblyCopyright = GetAssemblyCopyright(assembly) ?? string.Empty,
                AssemblyCompany = GetAssemblyCompany(assembly) ?? string.Empty,
                TargetFrameworkMoniker = targetFrameworkValue ?? string.Empty,
                Nullability = nullability,
                Deterministic = compiler.Options.Deterministic,
                RootNamespace = rootNamespace,
                AnalysisLevel = analysisLevel ?? string.Empty,
                ProjectDirectory = projectDirectory,
                Language = CSharpParseOptions.Default.Language,
                LanguageVersion = CSharpParseOptions.Default.LanguageVersion.ToDisplayString(),
                IsReleaseBuild = isReleaseBuild,
                CompilerVersion = typeof(CSharpCompilation).Assembly.GetName().Version?.ToString() ?? "Unknown",
                DotNetSdkVersion = GetDotNetSdkVersion(),
            };

            productionContext.AddSource("LinkDotNet.BuildInformation.g", GenerateBuildInformationClass(buildInformation));
        });
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
            return string.Empty;
        }
        
        return !analyzer.GlobalOptions.TryGetValue("build_property.projectDir", out var projectDir) 
            ? string.Empty 
            : projectDir;
    }

    private static string GenerateBuildInformationClass(
        BuildInformationInfo buildInformation)
    {
        var rootNamespace = string.IsNullOrEmpty(buildInformation.RootNamespace)
            ? string.Empty
            : $"\nnamespace {buildInformation.RootNamespace};\n";
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
                     /// <remarks>Value is: {{buildInformation.BuildAt}}</remarks>
                     public static readonly DateTime BuildAt = DateTime.ParseExact("{{buildInformation.BuildAt}}", "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                 
                     /// <summary>
                     /// Returns the platform.
                     /// </summary>
                     /// <remarks>Value is: {{buildInformation.Platform}}</remarks>
                     public const string Platform = "{{buildInformation.Platform}}";
                 
                     /// <summary>
                     /// Returns the warning level.
                     /// </summary>
                     /// <remarks>Value is: {{buildInformation.WarningLevel}}</remarks>
                     public const int WarningLevel = {{buildInformation.WarningLevel}};
                 
                     /// <summary>
                     /// Returns the configuration.
                     /// </summary>
                     /// <remarks>Value is: {{buildInformation.Configuration}}</remarks>
                     public const string Configuration = "{{buildInformation.Configuration}}";
                 
                     /// <summary>
                     /// Returns the assembly version.
                     /// </summary>
                     /// <remarks>Value is: {{buildInformation.AssemblyVersion}}</remarks>
                     public const string AssemblyVersion = "{{buildInformation.AssemblyVersion}}";
                 
                     /// <summary>
                     /// Returns the assembly file version.
                     /// </summary>
                     /// <remarks>Value is: {{buildInformation.AssemblyFileVersion}}</remarks>
                     public const string AssemblyFileVersion = "{{buildInformation.AssemblyFileVersion}}";
                 
                     /// <summary>
                     /// Returns the assembly name.
                     /// </summary>
                     /// <remarks>Value is: {{buildInformation.AssemblyName}}</remarks>
                     public const string AssemblyName = "{{buildInformation.AssemblyName}}";
                 
                     /// <summary>
                     /// Returns the assembly copyright.
                     /// </summary>
                     /// <remarks>Value is: {{buildInformation.AssemblyCopyright}}</remarks>
                     public const string AssemblyCopyright = "{{buildInformation.AssemblyCopyright}}";
                 
                     /// <summary>
                     /// Returns the assembly company.
                     /// </summary>
                     /// <remarks>Value is: {{buildInformation.AssemblyCompany}}</remarks>
                     public const string AssemblyCompany = "{{buildInformation.AssemblyCompany}}";
                 
                     /// <summary>
                     /// Returns the target framework moniker.
                     /// </summary>
                     /// <remarks>Value is: {{buildInformation.TargetFrameworkMoniker}}</remarks>
                     public const string TargetFrameworkMoniker = "{{buildInformation.TargetFrameworkMoniker}}";
                 
                     /// <summary>
                     /// Returns the nullability level.
                     /// </summary>
                     /// <remarks>Value is: {{buildInformation.Nullability}}</remarks>
                     public const string Nullability = "{{buildInformation.Nullability}}";
                 
                     /// <summary>
                     /// Returns whether the build is deterministic.
                     /// </summary>
                     /// <remarks>Value is: {{buildInformation.Deterministic.ToString().ToLowerInvariant()}}</remarks>
                     public const bool Deterministic = {{buildInformation.Deterministic.ToString().ToLowerInvariant()}};
                     
                     /// <summary>
                     /// Returns the Analysis level of the application.
                     /// </summary>
                     /// <remarks>Value is: {{buildInformation.AnalysisLevel}}</remarks>
                     public const string AnalysisLevel = "{{buildInformation.AnalysisLevel}}";
                     
                     /// <summary>
                     /// Returns the project directory.
                     /// </summary>
                     /// <remarks>Value is: {{buildInformation.ProjectDirectory}}</remarks>
                     public const string ProjectDirectory = "{{buildInformation.ProjectDirectory}}";
                     
                     /// <summary>
                     /// Returns the language the code is compiled against (like C# or F#).
                     /// </summary>
                     /// <example>C#</example>
                     /// <remarks>Value is {{buildInformation.Language}}</remarks>
                     public const string Language = "{{buildInformation.Language}}";
                     
                     /// <summary>
                     /// Returns the language version the code is compiled against. This is only the version (like 12.0).
                     /// </summary>
                     /// <example>12.0</example>
                     /// <remarks>Value is {{buildInformation.LanguageVersion}}</remarks>
                     public const string LanguageVersion = "{{buildInformation.LanguageVersion}}";
                     
                     /// <summary>
                     /// Returns whether the build is in Release mode.
                     /// </summary>
                     /// <remarks>Value is: {{buildInformation.IsReleaseBuild.ToString().ToLowerInvariant()}}</remarks>
                     public const bool IsReleaseBuild = {{buildInformation.IsReleaseBuild.ToString().ToLowerInvariant()}};
                     
                     /// <summary>
                     /// Returns the Roslyn/C# compiler version used during the build.
                     /// </summary>
                     /// <remarks>Value is: {{buildInformation.CompilerVersion}}</remarks>
                     public const string CompilerVersion = "{{buildInformation.CompilerVersion}}";
                     
                     /// <summary>
                     /// Returns the .NET runtime/framework version on which the build occurred (.NET SDK version).
                     /// This can differ from <see cref="TargetFrameworkMoniker"/> which indicates the target framework for which the code will run.
                     /// </summary>
                     /// <remarks>Value is: {{buildInformation.DotNetSdkVersion}}</remarks>
                     public const string DotNetSdkVersion = "{{buildInformation.DotNetSdkVersion}}";
                 }
                 """;
    }

    private sealed class BuildInformationInfo
    {
        public string BuildAt { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public int WarningLevel { get; set; }
        public string Configuration { get; set; } = string.Empty;
        public string AssemblyVersion { get; set; } = string.Empty;
        public string AssemblyFileVersion { get; set; } = string.Empty;
        public string AssemblyName { get; set; } = string.Empty;
        public string AssemblyCopyright { get; set; } = string.Empty;
        public string AssemblyCompany { get; set; } = string.Empty;
        public string TargetFrameworkMoniker { get; set; } = string.Empty;
        public string Nullability { get; set; } = string.Empty;
        public bool Deterministic { get; set; }
        public string RootNamespace { get; set; } = string.Empty;
        public string AnalysisLevel { get; set; } = string.Empty;
        public string ProjectDirectory { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string LanguageVersion { get; set; } = string.Empty;
        public bool IsReleaseBuild { get; set; }
        public string CompilerVersion { get; set; } = string.Empty;
        public string DotNetSdkVersion { get; set; } = string.Empty;
    }
}