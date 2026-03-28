namespace CommitLens.Models;

public class RepositoryInfo
{
    public required string Name { get; init; }
    public required string Path { get; init; }
    public List<CommitEntry> Commits { get; set;  } = [];

    public bool HasCommits => Commits.Count > 0;
}