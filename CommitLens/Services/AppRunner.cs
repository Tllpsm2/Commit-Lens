using CommitLens.Interfaces;

namespace CommitLens.Services;

public class AppRunner
{
    private readonly IRepositoryLocator _locator;
    private readonly IAppUseCase _useCase;

    public AppRunner(IRepositoryLocator locator, IAppUseCase useCase)
    {
        _locator = locator;
        _useCase = useCase;
    }

    public async Task RunAsync()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var repoPath = _locator.FindGitRoot(currentDir);
        await _useCase.RunAsync(repoPath, 30, 100); // repoPath, daysToLookBack, maxCommitsToShow
    }
}
