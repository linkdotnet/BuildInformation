namespace LinkDotNet.BuildInformation;

public sealed class GitInformationInfo
{
    public string Branch { get; init; } = string.Empty;
    public string Commit { get; init; } = string.Empty;
    public string ShortCommit => Commit.Length > 7 ? Commit[..7] : Commit;
    public string NearestTag { get; init; } = string.Empty;
    public string DetailedTagDescription { get; init; } = string.Empty;
}