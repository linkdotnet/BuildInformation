using System.Diagnostics;
using System.Reflection;
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

            var buildInformation = new BuildInformationInfo
            {
                BuildAt = DateTime.UtcNow.ToString("O"),
                Platform = compiler.Options.Platform.ToString(),
                WarningLevel = compiler.Options.WarningLevel,
                Configuration = configuration,
                AssemblyVersion = GetAssemblyVersion(assembly) ?? string.Empty,
                AssemblyFileVersion = GetAssemblyFileVersion(assembly) ?? string.Empty,
                AssemblyName = assembly.Name,
                TargetFrameworkMoniker = targetFrameworkValue ?? string.Empty,
                Nullability = nullability,
                Deterministic = compiler.Options.Deterministic,
                RootNamespace = rootNamespace,
                AnalysisLevel = analysisLevel ?? string.Empty,
                ProjectDirectory = projectDirectory,
                Language = CSharpParseOptions.Default.Language,
                LanguageVersion = CSharpParseOptions.Default.LanguageVersion.ToDisplayString(),
            };
            
            analyzer.GlobalOptions.TryGetValue("build_property.IncludeGitInformation", out var useGitInfoOption);
            var useGitInfo = useGitInfoOption?.Equals("true", StringComparison.InvariantCultureIgnoreCase) ?? false;

            var gitInfo = GitRetriever.GetGitInformation(useGitInfo);

            productionContext.AddSource("LinkDotNet.BuildInformation.g", GenerateBuildInformationClass(buildInformation, gitInfo));
        });
    }
    
    private static string? GetAssemblyFileVersion(ISymbol assembly)
    {
        var assemblyFileVersionAttribute = assembly.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass?.Name == "AssemblyFileVersionAttribute");
        var assemblyFileVersion = assemblyFileVersionAttribute is not null
            ? assemblyFileVersionAttribute.ConstructorArguments[0].Value!.ToString()
            : string.Empty;
        return assemblyFileVersion;
    }

    private static string? GetAssemblyVersion(ISymbol assembly)
    {
        var assemblyVersionAttribute = assembly.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass?.Name == "AssemblyVersionAttribute");
        var assemblyVersion = assemblyVersionAttribute is not null
            ? assemblyVersionAttribute.ConstructorArguments[0].Value!.ToString()
            : string.Empty;
        return assemblyVersion;
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
        BuildInformationInfo buildInformation,
        GitInformationInfo gitInfo)
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
                 internal static class BuildInformation
                 {
                     /// <summary>
                     /// Returns the build date (UTC).
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
                 }
                 
                 internal static class GitInformation
                 {
                     /// <summary>
                     /// Returns the branch of the git repository.
                     /// </summary>
                     /// <remarks>Value is: {{gitInfo.Branch}}</remarks>   
                     public const string Branch = "{{gitInfo.Branch}}";
                     
                     /// <summary>
                     /// Returns the commit hash of the git repository.
                     /// </summary>
                     /// <remarks>Value is: {{gitInfo.Commit}}</remarks>
                     public const string Commit = "{{gitInfo.Commit}}";
                     
                     /// <summary>
                     /// Returns the short commit hash of the git repository.
                     /// </summary>
                     /// <remarks>Value is: {{gitInfo.ShortCommit}}</remarks>
                     public const string ShortCommit = "{{gitInfo.ShortCommit}}";
                     
                     /// <summary>
                     /// Returns the nearest tag of the git repository. This uses <c>git describe --tags --abbrev=0</c>.
                     /// </summary>
                     /// <remarks>Value is: {{gitInfo.NearestTag}}</remarks>
                     public const string NearestTag = "{{gitInfo.NearestTag}}";
                     
                     /// <summary>
                     /// Returns the detailed tag description of the git repository. This uses <c>git describe --tags</c>.
                     /// </summary>
                     /// <remarks>Value is: {{gitInfo.DetailedTagDescription}}</remarks>
                     public const string DetailedTagDescription = "{{gitInfo.DetailedTagDescription}}";
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
        public string TargetFrameworkMoniker { get; set; } = string.Empty;
        public string Nullability { get; set; } = string.Empty;
        public bool Deterministic { get; set; }
        public string RootNamespace { get; set; } = string.Empty;
        public string AnalysisLevel { get; set; } = string.Empty;
        public string ProjectDirectory { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string LanguageVersion { get; set; } = string.Empty;
    }
}