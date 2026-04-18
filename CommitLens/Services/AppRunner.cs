using System;
using CommitLens.Interfaces;

namespace CommitLens.Services;

public class AppRunner
{
    private readonly IRepositoryLocator _locator;
    private readonly IGitLogProvider _gitLogProvider;
    private readonly ICommitParser _commitParser;
    private readonly IViewRenderer _viewRenderer;

    // Constructor with dependency injection
    public AppRunner(
        IRepositoryLocator locator,
        IGitLogProvider gitLogProvider,
        ICommitParser commitParser,
        IViewRenderer viewRenderer)
    {
        _locator = locator;
        _gitLogProvider = gitLogProvider;
        _commitParser = commitParser;
        _viewRenderer = viewRenderer;
    }

    public async Task RunAsync()
    {   // Find the Git repository root (search from current directory upwards)
        var currentDir = Directory.GetCurrentDirectory();
        var repoPath = _locator.FindGitRoot(currentDir);
        
        if (string.IsNullOrWhiteSpace(repoPath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: No Git repository found starting from '{currentDir}' and searching upwards.");
            Console.ResetColor();
            return;
        }

        var rawOutput = await _gitLogProvider.GetRawLogAsync(repoPath, 30);
        var commits = _commitParser.ParseOutput(rawOutput);
        _viewRenderer.ShowRecentCommits(commits, 30, 15); // commits, days, max to show
    }
}
