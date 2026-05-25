using CommitLens.Domain.Commits;

namespace CommitLens.Domain.UnitTests.Commits;

public class CommitTests
{
    private static CommitHash ValidHash() => new("a1b2c3d");
    private static Author ValidAuthor() => new("João Alves", "joao@email.com");

    [Fact]
    public void Constructor_WithEmptySubject_FallsBackToNoMessage()
    {
        var commit = new Commit(ValidHash(), ValidAuthor(), DateTimeOffset.UtcNow, string.Empty);
        commit.Subject.Should().Be("<no message>");
    }

    [Fact]
    public void Constructor_TrimsSubject()
    {
        var commit = new Commit(ValidHash(), ValidAuthor(), DateTimeOffset.UtcNow, "  trimmed subject  ");
        commit.Subject.Should().Be("trimmed subject");
    }
}
