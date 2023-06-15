using Microsoft.CodeAnalysis;

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

            var assembly = compiler.Assembly;
            var buildInformation = new BuildInformationInfo
            {
                BuildAt = DateTime.UtcNow.ToString("O"),
                Platform = compiler.Options.Platform.ToString(),
                WarningLevel = compiler.Options.WarningLevel,
                Configuration = compiler.Options.OptimizationLevel.ToString(),
                AssemblyVersion = GetAssemblyVersion(assembly) ?? string.Empty,
                AssemblyFileVersion = GetAssemblyFileVersion(assembly) ?? string.Empty,
                AssemblyName = assembly.Name,
                TargetFrameworkMoniker = targetFrameworkValue ?? string.Empty,
                Nullability = nullability,
            };

            productionContext.AddSource("LinkDotNet.BuildInformation.g", GenerateBuildInformationClass(buildInformation));
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

    private static string GenerateBuildInformationClass(BuildInformationInfo buildInformation)
    {
        return $@"
using System;
using System.Globalization;

public static class BuildInformation
{{
    /// <summary>
    /// Returns the build date (UTC).
    /// </summary>
    /// <remarks>Value is: {buildInformation.BuildAt}</remarks>
    public static readonly DateTime BuildAt = DateTime.ParseExact(""{buildInformation.BuildAt}"", ""O"", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

    /// <summary>
    /// Returns the platform.
    /// </summary>
    /// <remarks>Value is: {buildInformation.Platform}</remarks>
    public const string Platform = ""{buildInformation.Platform}"";

    /// <summary>
    /// Returns the warning level.
    /// </summary>
    /// <remarks>Value is: {buildInformation.WarningLevel}</remarks>
    public const int WarningLevel = {buildInformation.WarningLevel};

    /// <summary>
    /// Returns the configuration.
    /// </summary>
    /// <remarks>Value is: {buildInformation.Configuration}</remarks>
    public const string Configuration = ""{buildInformation.Configuration}"";

    /// <summary>
    /// Returns the assembly version.
    /// </summary>
    /// <remarks>Value is: {buildInformation.AssemblyVersion}</remarks>
    public const string AssemblyVersion = ""{buildInformation.AssemblyVersion}"";

    /// <summary>
    /// Returns the assembly file version.
    /// </summary>
    /// <remarks>Value is: {buildInformation.AssemblyFileVersion}</remarks>
    public const string AssemblyFileVersion = ""{buildInformation.AssemblyFileVersion}"";

    /// <summary>
    /// Returns the assembly name.
    /// </summary>
    /// <remarks>Value is: {buildInformation.AssemblyName}</remarks>
    public const string AssemblyName = ""{buildInformation.AssemblyName}"";

    /// <summary>
    /// Returns the target framework moniker.
    /// </summary>
    /// <remarks>Value is: {buildInformation.TargetFrameworkMoniker}</remarks>
    public const string TargetFrameworkMoniker = ""{buildInformation.TargetFrameworkMoniker}"";

    /// <summary>
    /// Returns the nullability level.
    /// </summary>
    /// <remarks>Value is: {buildInformation.Nullability}</remarks>
    public const string Nullability = ""{buildInformation.Nullability}"";
}}
";
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
    }
}
