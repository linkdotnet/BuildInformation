// See https://aka.ms/new-console-template for more information

using LinkDotNet.BuildInformation.Sample;

Console.WriteLine($"Build at: {BuildInformation.BuildAt}");
Console.WriteLine($"Platform: {BuildInformation.Platform}");
Console.WriteLine($"Warning level: {BuildInformation.WarningLevel}");
Console.WriteLine($"Configuration: {BuildInformation.Configuration}");
Console.WriteLine($"Assembly version: {BuildInformation.AssemblyVersion}");
Console.WriteLine($"Assembly file version: {BuildInformation.AssemblyFileVersion}");
Console.WriteLine($"Assembly name: {BuildInformation.AssemblyName}");
Console.WriteLine($"Assembly copyright: {BuildInformation.AssemblyCopyright}");
Console.WriteLine($"Assembly company: {BuildInformation.AssemblyCompany}");
Console.WriteLine($"Target framework moniker: {BuildInformation.TargetFrameworkMoniker}");
Console.WriteLine($"Analysis level: {BuildInformation.Nullability}");
Console.WriteLine($"Deterministic build: {BuildInformation.Deterministic}");
Console.WriteLine($"Analysis level: {BuildInformation.AnalysisLevel}");
Console.WriteLine($"Project directory: {BuildInformation.ProjectDirectory}");
Console.WriteLine($"Language: {BuildInformation.Language}");
Console.WriteLine($"Language version: {BuildInformation.LanguageVersion}");
Console.WriteLine($"Compiler version: {BuildInformation.CompilerVersion}");
Console.WriteLine($"DotNet SDK version: {BuildInformation.DotNetSdkVersion}");