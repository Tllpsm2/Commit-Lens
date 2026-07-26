using CommitLens.Domain.Commits;

namespace CommitLens.Domain.Repositories;

public record Repo
{
    public string Name { get; init; }
    public string FullPath { get; init; }
    public IReadOnlyList<string> Branches { get; init; }
    public IReadOnlyList<Commit> Commits { get; init; }

    public Repo(
        string name,
        string fullPath,
        IReadOnlyList<string> branches,
        IReadOnlyList<Commit> commits)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Repository name is required.", nameof(name));

        if (string.IsNullOrWhiteSpace(fullPath))
            throw new ArgumentException("Repository path is required.", nameof(fullPath));

        Name = name;
        FullPath = fullPath;
        Branches = branches ?? [];
        Commits = commits ?? [];
    }

    public IEnumerable<Commit> GetCommitsByAuthor(string authorName) =>
        Commits.Where(c => c.Author.Name.Equals(authorName, StringComparison.OrdinalIgnoreCase));

    public IEnumerable<Commit> GetCommitsInPeriod(DateTimeOffset start, DateTimeOffset end) =>
        Commits.Where(c => c.Date >= start && c.Date <= end);

    public IEnumerable<Commit> GetCommitsByAuthorInPeriod(
        string authorName, DateTimeOffset start, DateTimeOffset end) =>
        GetCommitsByAuthor(authorName).Where(c => c.Date >= start && c.Date <= end);
}
