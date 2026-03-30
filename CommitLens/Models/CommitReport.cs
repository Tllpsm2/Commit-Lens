
namespace CommitLens.Models;

public record CommitReport(
    int TotalCommits,
    Dictionary<string, int> CommitsPerDay,
    Dictionary<string, int> CommitsPerAuthor
);