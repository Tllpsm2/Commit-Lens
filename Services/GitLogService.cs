using System.Diagnostics;
using CommitLens.Models;

namespace CommitLens.Services;

public class GitLogService
{
    // The output format for git log — fields separated by |
    // %H=hash, %an=author name, %ae=author email, %ad=author date, %s=subject (first line of msg)
    private const string LogFormat = "%H|%an|%ae|%ad|%s";

    public async Task<List<CommitEntry>> GetCommitsAsync(
        string repoPath,
        string? authorEmail = null,
        int days = 30)
    {
        // Check if it's a valid git repository
        if (!Directory.Exists(repoPath) ||
            !Directory.Exists(Path.Combine(repoPath, ".git")))
            return [];

        var since = DateTime.UtcNow.AddDays(-days).ToString("yyyy-MM-dd");
        var args = $"log --pretty=format:\"{LogFormat}\" --date=iso-strict --since={since}";

        if (!string.IsNullOrWhiteSpace(authorEmail))
            args += $" --author={authorEmail}";

        // Process.Start runs the 'git' program as a child process
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = args,
                RedirectStandardOutput = true,  // captures stdout to our code
                RedirectStandardError = true,
                UseShellExecute = false,        // REQUIRED to redirect output
                WorkingDirectory = repoPath
            }
        };

        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        return ParseOutput(output);
    }

    // static = can be tested without instantiating GitLogService
    // Receives the raw string from git log and returns a list of CommitEntry
    public static List<CommitEntry> ParseOutput(string rawOutput)
    {
        var commits = new List<CommitEntry>();

        foreach (var line in rawOutput.Split('\n',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            // Limiting the split to 5 parts preserves pipes inside the commit message
            // "abc|Joao|j@e.com|2024-01|feat: A | B" => 5 correct parts
            var parts = line.Split('|', 5);
            if (parts.Length < 5) continue;

            if (!DateTimeOffset.TryParse(parts[3], out var date))
                continue;

            commits.Add(new CommitEntry(
                Hash: parts[0],
                AuthorName: parts[1],
                AuthorEmail: parts[2],
                Date: date,
                Message: parts[4]
            ));
        }

        return commits;
    }
}