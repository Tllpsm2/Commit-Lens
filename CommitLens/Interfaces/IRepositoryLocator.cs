namespace CommitLens.Interfaces;

public interface IRepositoryLocator
{
    string? FindGitRoot(string startDir);
}
