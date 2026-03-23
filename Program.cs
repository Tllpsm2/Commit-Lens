using CommitLens.Services;

Console.Clear();

var service = new GitLogService();

// set a repo path or use the current directory (".") if running inside a git repo
var repoPath = Directory.GetCurrentDirectory();

var commits = await service.GetCommitsAsync(repoPath, days: 30);

Console.WriteLine($"Found {commits.Count} commits");

// limit to {x} commits for display
foreach (var c in commits.Take(20))
    Console.WriteLine($"{c.Message} — {c.AuthorEmail}");