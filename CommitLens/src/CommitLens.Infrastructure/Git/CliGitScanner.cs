using System.Diagnostics;
using CommitLens.Application.Abstractions;
using CommitLens.Domain.Repositories;

namespace CommitLens.Infrastructure.Git;

public sealed class CliGitScanner : IGitScanner
{
    public Repository Scan(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

        var name = new DirectoryInfo(directoryPath).Name;
        var branches = GetBranches(directoryPath);
        var commits = GetCommits(directoryPath);

        return new Repository(name, directoryPath, branches, commits);
    }

    private static IReadOnlyList<string> GetBranches(string path)
    {
        var output = RunGit("branch --format=%(refname:short)", path);
        return output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(b => b.Trim())
            .ToList();
    }

    private static IReadOnlyList<Domain.Commits.Commit> GetCommits(string path)
    {
        var output = RunGit($"log --format={CommitParser.LogFormat}", path);
        return output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(CommitParser.Parse)
            .Where(c => c is not null)
            .ToList()!;
    }

    private static string RunGit(string arguments, string workingDirectory)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true, // Capture output for processing
                RedirectStandardError = true, // Capture error output for diagnostics
                UseShellExecute = false, // leave false
                CreateNoWindow = true
            }
        };

        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            var error = process.StandardError.ReadToEnd();
            throw new InvalidOperationException(
                $"Git command failed: 'git {arguments}' in '{workingDirectory}'. {error}");
        }

        return output;
    }
}
