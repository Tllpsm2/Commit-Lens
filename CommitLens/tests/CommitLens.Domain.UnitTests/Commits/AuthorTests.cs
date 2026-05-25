using CommitLens.Domain.Commits;

namespace CommitLens.Domain.UnitTests.Commits;

public class AuthorTests
{
    [Fact]
    public void Constructor_WithValidData_SetsProperties()
    {
        var author = new Author("João Alves", "joao@example.com");

        author.Name.Should().Be("João Alves");
        author.Email.Should().Be("joao@example.com");
    }

    [Fact]
    public void Constructor_WithEmptyName_FallsBackToUnknown()
    {
        var author = new Author(string.Empty, "joao@example.com");
        author.Name.Should().Be("Unknown");
    }

    [Fact]
    public void Constructor_WithEmptyEmail_SetsEmptyString()
    {
        var author = new Author("João", string.Empty);
        author.Email.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_TrimsNameAndEmail()
    {
        var author = new Author("  João  ", "  joao@example.com  ");

        author.Name.Should().Be("João");
        author.Email.Should().Be("joao@example.com");
    }
}
