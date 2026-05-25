using CommitLens.Application.Abstractions;

namespace CommitLens.Infrastructure.FileSystem;

public sealed class FileSystemLocator : IRepositoryLocator
{
    public IReadOnlyList<string> FindRepositories(string rootPath)
    {
        if (!Directory.Exists(rootPath))
            throw new DirectoryNotFoundException($"Root directory not found: {rootPath}");

        return Directory
            .GetDirectories(rootPath, ".git", SearchOption.AllDirectories)
            .Select(gitDir => Directory.GetParent(gitDir)!.FullName)
            .ToList();
    }
}
