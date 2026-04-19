using System;
using CommitLens.Interfaces;

namespace CommitLens.Services;

public interface IAppUseCase
{
    Task RunAsync(string repoPath, int daysBack, int maxCommitsToShow);
}

public class AppUseCase : IAppUseCase
{
    private readonly IGitLogProvider _gitLogProvider;
    private readonly ICommitParser _commitParser;
    private readonly IReportService _reportService;
    private readonly IViewRenderer _viewRenderer;

    // Constructor with dependency injection
    public AppUseCase(
        IGitLogProvider gitLogProvider,
        ICommitParser commitParser,
        IReportService reportService,
        IViewRenderer viewRenderer)
    {
        _gitLogProvider = gitLogProvider;
        _commitParser = commitParser;
        _reportService = reportService;
        _viewRenderer = viewRenderer;
    }

    public async Task RunAsync(string repoPath, int daysBack, int maxCommitsToShow)
    {   
        if (string.IsNullOrWhiteSpace(repoPath))
        {
            _viewRenderer.RenderNoGitRepoMessage(repoPath);
            return;
        }
        var rawOutput = await _gitLogProvider.GetRawLogAsync(repoPath, daysBack);
        
        var parsedCommits = _commitParser.ParseOutput(rawOutput);
        if (!_reportService.HasCommits(parsedCommits))
        {
            _viewRenderer.RenderNoCommitsMessage(daysBack);
            return;
        }
        
        var report = _reportService.Build(parsedCommits);

        _viewRenderer.RenderCommitsTable(parsedCommits, daysBack, maxCommitsToShow);
        _viewRenderer.RenderCommitStatistics(report);
    }
}