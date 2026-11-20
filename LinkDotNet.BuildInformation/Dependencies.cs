using System.Text;

namespace LinkDotNet.BuildInformation;

internal static class Dependencies
{
    public static string Generate(string? projectAssetsJson, string? targetFramework)
    {
        var direct = new StringBuilder();
        var transitive = new StringBuilder();
        var directJson = new List<string>();
        var transitiveJson = new List<string>();

        if (projectAssetsJson is not null
            && Json.Parse(projectAssetsJson) is Dictionary<string, object?> assets
            && assets.TryGetValue("targets", out var targetsValue)
            && targetsValue is Dictionary<string, object?> targets
            && FindTargetKey(targets, targetFramework) is { } key
            && targets[key] is Dictionary<string, object?> libraries)
        {
            var directNames = GetDirectNames(assets, key);
            foreach (var library in libraries.Keys)
            {
                var separator = library.IndexOf('/');
                var name = library.Substring(0, separator);
                var version = library.Substring(separator + 1);
                var isDirect = directNames.Contains(name);
                (isDirect ? direct : transitive).Append($"\n        new Dependency({IncrementalBuildInformationGenerator.Literal(name)}, {IncrementalBuildInformationGenerator.Literal(version)}),");
                (isDirect ? directJson : transitiveJson).Add($"{{\"name\":{JsonString(name)},\"version\":{JsonString(version)}}}");
            }
        }

        var json = $"{{\"direct\":[{string.Join(",", directJson)}],\"transitive\":[{string.Join(",", transitiveJson)}]}}";

        return $$"""


                     /// <summary>
                     /// Returns the direct dependencies (NuGet packages and project references) of the project.
                     /// </summary>
                     public static readonly global::System.Collections.Generic.IReadOnlyList<Dependency> DirectDependencies = new Dependency[]
                     {{{direct}}
                     };

                     /// <summary>
                     /// Returns the transitive dependencies (NuGet packages and project references) of the project.
                     /// </summary>
                     public static readonly global::System.Collections.Generic.IReadOnlyList<Dependency> TransitiveDependencies = new Dependency[]
                     {{{transitive}}
                     };

                     /// <summary>
                     /// Returns the direct and transitive dependencies as JSON, e.g. for a software bill of materials (SBOM).
                     /// </summary>
                     /// <example>{"direct":[{"name":"Serilog","version":"4.0.0"}],"transitive":[]}</example>
                     public const string DependenciesJson = {{IncrementalBuildInformationGenerator.Literal(json)}};

                     /// <summary>
                     /// A resolved dependency of the project.
                     /// </summary>
                     public sealed class Dependency
                     {
                         public Dependency(string name, string version)
                         {
                             Name = name;
                             Version = version;
                         }

                         /// <summary>
                         /// Returns the package or project name.
                         /// </summary>
                         public string Name { get; }

                         /// <summary>
                         /// Returns the resolved version.
                         /// </summary>
                         public string Version { get; }

                         public override string ToString() => Name + " " + Version;
                     }
                 """;
    }

    // Assets file v4 keys targets by TargetFramework; v3 (SDK 8) uses framework monikers.
    // ponytail: v3 multi-targeting yields no match, add a TargetFrameworkMoniker lookup if SDK 8 multi-targeting matters.
    private static string? FindTargetKey(Dictionary<string, object?> targets, string? targetFramework)
    {
        if (targetFramework is not null && targets.ContainsKey(targetFramework))
        {
            return targetFramework;
        }

        var candidates = targets.Keys.Where(k => !k.Contains('/')).ToList();
        return candidates.Count == 1 ? candidates[0] : null;
    }

    private static string JsonString(string value) => "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";

    private static HashSet<string> GetDirectNames(Dictionary<string, object?> assets, string key)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (assets.TryGetValue("projectFileDependencyGroups", out var groupsValue)
            && groupsValue is Dictionary<string, object?> groups
            && groups.TryGetValue(key, out var groupValue)
            && groupValue is List<object?> group)
        {
            foreach (var entry in group.OfType<string>())
            {
                names.Add(entry.Split(' ')[0]);
            }
        }

        return names;
    }
}
