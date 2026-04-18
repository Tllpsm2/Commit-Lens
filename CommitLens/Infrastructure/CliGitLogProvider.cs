using System.Diagnostics;
using CommitLens.Interfaces;

namespace CommitLens.Infrastructure;

public class CliGitLogProvider : IGitLogProvider
{
    private const string LogFormat = "%H|%an|%ae|%ad|%s";
    private const string DateFormat = "iso-strict";

    public async Task<string> GetRawLogAsync(
        string repoPath,
        int days,
        string? authorEmail = null)
    {
        var since = DateTime.UtcNow.AddDays(-days).ToString("yyyy-MM-dd");

        var psi = new ProcessStartInfo
        {
            FileName = "git",
            WorkingDirectory = repoPath,
            RedirectStandardOutput = true,  // capture output
            RedirectStandardError = true,   // capture errors
            UseShellExecute = false,        // required for redirection
            CreateNoWindow = true           // don't show a console window
        };

        // Build the git log command
        psi.ArgumentList.Add("log");
        psi.ArgumentList.Add($"--pretty=format:{LogFormat}");
        psi.ArgumentList.Add($"--date={DateFormat}");
        psi.ArgumentList.Add($"--since={since}");
        if (!string.IsNullOrWhiteSpace(authorEmail))
            psi.ArgumentList.Add($"--author={authorEmail}");

        // capture output and error
        using var process = new Process { StartInfo = psi };
        process.Start();

        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        await Task.WhenAll(outputTask, errorTask);

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new Exception($"Git command failed with exit code {process.ExitCode}: {await errorTask}");
        }

        return await outputTask;
    }
}