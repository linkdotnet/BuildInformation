using System.Diagnostics;

namespace LinkDotNet.BuildInformation;

public static class GitRetriever
{
    public static GitInformationInfo GetGitInformation(bool useGitInfo)
    {
        if (!useGitInfo)
        {
            return new GitInformationInfo();
        }
        
        return new GitInformationInfo
        {
            Branch = GetGitInfoByCommand("rev-parse --abbrev-ref HEAD"),
            Commit = GetGitInfoByCommand("rev-parse HEAD"),
            NearestTag = GetGitInfoByCommand("describe --tags --abbrev=0"),
            DetailedTagDescription = GetGitInfoByCommand("describe --tags"),
        };
        
        static string GetGitInfoByCommand(string command)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = command,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            var process = new Process { StartInfo = processInfo };
                
            process.Start();  
            var result = process.StandardOutput.ReadToEnd().Trim();  
            process.WaitForExit();
            return result;
        }
    }
}