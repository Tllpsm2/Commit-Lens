using CommitLens.Services;

Console.Clear();
Console.WriteLine("=== CommitLens ===");

var service = new GitLogService();

// Find the Git repository root (search from current directory upwards) [PLACE HOLDER]
var currentDir = Directory.GetCurrentDirectory();
var repoPath = FindGitRoot(currentDir);

if (repoPath == null)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Error: No Git repository found starting from '{currentDir}' and searching upwards.");
    Console.ResetColor();
    return;
}

Console.WriteLine($"Scanning repository: {repoPath}");

try
{
    var commits = await service.GetCommitsAsync(repoPath, days: 30);

    if (commits.Count == 0)
    {
        Console.WriteLine("No commits found in the last 30 days.");
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Found {commits.Count} commits:");
        Console.ResetColor();
        Console.WriteLine(new string('-', 40));

        foreach (var c in commits.Take(15))
            Console.WriteLine($"[{c.Hash}] {c.Message} ({c.AuthorName})");
        
        if (commits.Count > 15)
            Console.WriteLine($"... and {commits.Count - 15} more.");
    }
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Failed to capture commits: {ex.Message}");
    Console.ResetColor();
}

static string? FindGitRoot(string startDir)
{
    var dir = new DirectoryInfo(startDir);
    while (dir != null)
    {
        if (Directory.Exists(Path.Combine(dir.FullName, ".git")))
            return dir.FullName;
        
        dir = dir.Parent;
    }
    return null;
}