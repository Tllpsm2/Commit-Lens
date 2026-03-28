using System.Diagnostics;
using CommitLens.Models;

namespace CommitLens.Services;

public class GitLogService
{
    // Git log variables for parsing output
    private const string LogFormat = "%H|%an|%ae|%ad|%s"; // hash|author name|author email|date|subject
    private const string DateFormat = "iso-strict"; // ISO 8601 format

    public async Task<List<CommitEntry>> GetCommitsAsync(
        string repoPath,
        string? authorEmail = null,
        int days = 30)
    {
        // check repoPath exists and contains a .git folder
        if (!Directory.Exists(repoPath) ||
        !Directory.Exists(Path.Combine(repoPath, ".git")))
            return [];

        // build git log command
        var since = DateTime.UtcNow.AddDays(-days).ToString("yyyy-MM-dd");

        var args = string.Join(" ", new[]
        {
            "log",
            $"--pretty=format:\"{LogFormat}\"",
            $"--date={DateFormat}",
            $"--since={since}",
            !string.IsNullOrWhiteSpace(authorEmail) ? $"--author=\"{authorEmail}\"" : null
        }
        .Where(s => s != null));

        // Process.Start runs the 'git' program as a child process
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = args,
                WorkingDirectory = repoPath,
                RedirectStandardOutput = true, // captures stdout
                RedirectStandardError = true,
                UseShellExecute = false,    // false required for stdout redirection
                CreateNoWindow = true       // don't show a console window
            }
        };

        /// [place holder - exception handling]
        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        return ParseOutput(output);
    }

    public static List<CommitEntry> ParseOutput(string rawOutput)
    {
        if (string.IsNullOrWhiteSpace(rawOutput)) return [];

        var commits = new List<CommitEntry>();

        // split output into lines and parse each line
        foreach (var line in rawOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = line.Split('|', 5);
            if (parts.Length < 5) continue;

            if (!DateTimeOffset.TryParse(parts[3], out var date))
                continue;
            
            // Positional records require constructor initialization; named arguments are used here for better readability.
            commits.Add(new CommitEntry(
                Hash: parts[0][..7], // short hash
                AuthorName: parts[1],
                AuthorEmail: parts[2],
                Date: date,
                Message: parts[4]
            ));
        }
        
        return commits;
    }
}