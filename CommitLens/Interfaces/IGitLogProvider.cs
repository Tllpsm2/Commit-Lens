namespace CommitLens.Interfaces;

public interface IGitLogProvider
{
    Task<string> GetRawLogAsync(string repoPath, int days, string? authorEmail = null);
}