using Microsoft.CodeAnalysis;

[Generator]
public sealed class IncrementalBuildInformationGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var compilationAndOptionsProvider = context
            .CompilationProvider
            .Select((s, _) => s);

        context.RegisterSourceOutput(compilationAndOptionsProvider, static (productionContext, options) =>
        {
            var assembly = options.Assembly;
            var buildInformation = new BuildInformationInfo
            {
                BuildAt = DateTime.UtcNow.ToString("O"),
                Platform = options.Options.Platform.ToString(),
                WarningLevel = options.Options.WarningLevel,
                Configuration = options.Options.OptimizationLevel.ToString(),
                AssemblyVersion = GetAssemblyVersion(assembly),
                AssemblyFileVersion = GetAssemblyFileVersion(assembly),
                AssemblyName = assembly.Name,
            };

            productionContext.AddSource("LinkDotNet.BuildInformation.g", GenerateBuildInformationClass(buildInformation));
        });
    }

    private static string GetAssemblyFileVersion(ISymbol assembly)
    {
        var assemblyFileVersionAttribute = assembly.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass?.Name == "AssemblyFileVersionAttribute");
        var assemblyFileVersion = assemblyFileVersionAttribute is not null
            ? assemblyFileVersionAttribute.ConstructorArguments[0].Value!.ToString()
            : string.Empty;
        return assemblyFileVersion;
    }

    private static string GetAssemblyVersion(ISymbol assembly)
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
    public const string AssemblyVersion = ""{buildInformation.AssemblyVersion ?? string.Empty}"";

    /// <summary>
    /// Returns the assembly file version.
    /// </summary>
    /// <remarks>Value is: {buildInformation.AssemblyFileVersion}</remarks>
    public const string AssemblyFileVersion = ""{buildInformation.AssemblyFileVersion ?? string.Empty}"";

    /// <summary>
    /// Returns the assembly name.
    /// </summary>
    /// <remarks>Value is: {buildInformation.AssemblyName}</remarks>
    public const string AssemblyName = ""{buildInformation.AssemblyName ?? string.Empty}"";
}}
";
    }

    private sealed class BuildInformationInfo
    {
        public string BuildAt { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public int WarningLevel { get; set; }
        public string Configuration { get; set; } = string.Empty;
        public string? AssemblyVersion { get; set; }
        public string? AssemblyFileVersion { get; set; }
        public string? AssemblyName { get; set; }
    }
}
