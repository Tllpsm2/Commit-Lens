using CommitLens.Interfaces;

namespace CommitLens.Infrastructure;

public class FileSystemLocator : IRepositoryLocator
{
    public string? FindGitRoot(string startDir)
    {
        // Search upwards from the starting directory for a .git folder
        var dir = new DirectoryInfo(startDir);
        while (dir != null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, ".git")))
                return dir.FullName;

            dir = dir.Parent;
        }
        return null;
    }
}
