using CommitLens.Domain.Commits;
using CommitLens.Domain.Repositories;

namespace CommitLens.Domain.UnitTests.Repositories;

public class RepositoryTests
{
    private static Commit MakeCommit(string authorName, DateTimeOffset date) =>
        new(new CommitHash("a1b2c3d"), new Author(authorName, "e@e.com"), date, "subject");

    [Fact]
    public void Constructor_WithEmptyName_ThrowsArgumentException()
    {
        var act = () => new Repository(string.Empty, "/path", [], []);
        act.Should().Throw<ArgumentException>().WithMessage("*name is required*");
    }

    [Fact]
    public void Constructor_WithEmptyPath_ThrowsArgumentException()
    {
        var act = () => new Repository("MyRepo", string.Empty, [], []);
        act.Should().Throw<ArgumentException>().WithMessage("*path is required*");
    }

    [Fact]
    public void GetCommitsByAuthor_ReturnsOnlyMatchingCommits()
    {
        var date = DateTimeOffset.UtcNow;
        var commits = new[]
        {
            MakeCommit("Ana", date),
            MakeCommit("Caio", date),
            MakeCommit("ana", date)
        };
        var repo = new Repository("R", "/p", [], commits);

        var result = repo.GetCommitsByAuthor("Ana").ToList();

        result.Should().HaveCount(2);
    }

    [Fact]
    public void GetCommitsByAuthor_IsCaseInsensitive()
    {
        var repo = new Repository("R", "/p", [], [MakeCommit("Ana", DateTimeOffset.UtcNow)]);

        repo.GetCommitsByAuthor("ANA").Should().HaveCount(1);
    }

    [Fact]
    public void GetCommitsInPeriod_ReturnsOnlyCommitsWithinRange()
    {
        var start = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2025, 1, 31, 23, 59, 59, TimeSpan.Zero);

        var commits = new[]
        {
            MakeCommit("Ana", new DateTimeOffset(2025, 1, 10, 0, 0, 0, TimeSpan.Zero)),
            MakeCommit("Ana", new DateTimeOffset(2024, 12, 31, 0, 0, 0, TimeSpan.Zero)),
            MakeCommit("Ana", new DateTimeOffset(2025, 2, 1, 0, 0, 0, TimeSpan.Zero)),
        };
        var repo = new Repository("R", "/p", [], commits);

        repo.GetCommitsInPeriod(start, end).Should().HaveCount(1);
    }

    [Fact]
    public void GetCommitsByAuthorInPeriod_AppliesBothFilters()
    {
        var start = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2025, 1, 31, 0, 0, 0, TimeSpan.Zero);

        var commits = new[]
        {
            MakeCommit("Ana", new DateTimeOffset(2025, 1, 10, 0, 0, 0, TimeSpan.Zero)),
            MakeCommit("Bob",   new DateTimeOffset(2025, 1, 10, 0, 0, 0, TimeSpan.Zero)),
            MakeCommit("Ana", new DateTimeOffset(2024, 12, 1, 0, 0, 0, TimeSpan.Zero)),
        };
        var repo = new Repository("R", "/p", [], commits);

        var result = repo.GetCommitsByAuthorInPeriod("Ana", start, end).ToList();

        result.Should().HaveCount(1);
        result[0].Author.Name.Should().Be("Ana");
    }
}
