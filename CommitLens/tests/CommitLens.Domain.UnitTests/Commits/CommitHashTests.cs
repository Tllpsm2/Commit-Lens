using CommitLens.Domain.Commits;

namespace CommitLens.Domain.UnitTests.Commits;

public class CommitHashTests
{
    [Fact]
    public void Constructor_WithEmptyHash_ThrowsArgumentException()
    {
        var act = () => new CommitHash(string.Empty);
        act.Should().Throw<ArgumentException>().WithMessage("*cannot be empty*");
    }

    [Theory]
    [InlineData("abc123")]
    [InlineData("xyz!@#$1234567")]
    public void Constructor_WithInvalidFormat_ThrowsArgumentException(string hash)
    {
        var act = () => new CommitHash(hash);
        act.Should().Throw<ArgumentException>().WithMessage("*Invalid hash format*");
    }

    [Fact]
    public void Abbreviated_ReturnsFirst7Characters()
    {
        var hash = new CommitHash("a1b2c3d4e5f6789");
        hash.Abbreviated.Should().Be("a1b2c3d");
    }
}
