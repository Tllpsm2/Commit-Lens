using CommitLens.Application.Abstractions;
using CommitLens.Application.Reports.GetPeriodOverview;
using CommitLens.Domain.Commits;
using CommitLens.Domain.Repositories;
using NSubstitute;

namespace CommitLens.Application.UnitTests.Reports;

public class GetPeriodOverviewQueryHandlerTests
{
    private readonly IGitScanner _scanner = Substitute.For<IGitScanner>();
    private readonly GetPeriodOverviewQueryHandler _handler;

    public GetPeriodOverviewQueryHandlerTests()
    {
        _handler = new GetPeriodOverviewQueryHandler(_scanner);
    }

    private static Commit MakeCommit(
        string authorName,
        DateTimeOffset date,
        string subject = "feat: something") =>
        new(new CommitHash("a1b2c3d"), new Author(authorName, "a@b.com"), date, subject);

    private static Repository MakeRepo(string name, IEnumerable<Commit> commits) =>
        new(name, $"/repos/{name}", ["main"], commits.ToList());

    [Theory]
    [InlineData(ReportPeriod.Daily, "Last 24 hours")]
    [InlineData(ReportPeriod.Weekly, "Last 7 days")]
    [InlineData(ReportPeriod.Monthly, "Last 30 days")]
    [InlineData(ReportPeriod.Yearly, "Last 12 months")]
    public void Handle_ReturnsPeriodLabel(ReportPeriod period, string expectedLabel)
    {
        _scanner.Scan(Arg.Any<string>()).Returns(MakeRepo("R", []));

        var response = _handler.Handle(new GetPeriodOverviewQuery(["/p"], period));

        response.PeriodLabel.Should().Be(expectedLabel);
    }

    [Fact]
    public void Handle_ExcludesCommitsOutsideThePeriod()
    {
        var recentCommit = MakeCommit("Alice", DateTimeOffset.UtcNow.AddDays(-5));
        var oldCommit = MakeCommit("Alice", DateTimeOffset.UtcNow.AddDays(-60));

        _scanner.Scan("/repo").Returns(MakeRepo("Repo", [recentCommit, oldCommit]));

        var response = _handler.Handle(new GetPeriodOverviewQuery(["/repo"], ReportPeriod.Monthly));

        response.TotalCommits.Should().Be(1);
    }

    [Fact]
    public void Handle_WithNoCommitsInPeriod_ReturnsEmptyRepositories()
    {
        _scanner.Scan("/repo").Returns(MakeRepo("Repo", [MakeCommit("Bob", DateTimeOffset.UtcNow.AddDays(-40))]));

        var response = _handler.Handle(new GetPeriodOverviewQuery(["/repo"], ReportPeriod.Monthly));

        response.TotalCommits.Should().Be(0);
        response.Repositories.Should().BeEmpty();
    }

    [Fact]
    public void Handle_WithAuthorFilter_IncludesOnlyMatchingAuthor()
    {
        var commits = new[]
        {
            MakeCommit("Alice", DateTimeOffset.UtcNow.AddDays(-1)),
            MakeCommit("Bob", DateTimeOffset.UtcNow.AddDays(-1)),
        };
        _scanner.Scan("/repo").Returns(MakeRepo("Repo", commits));

        var response = _handler.Handle(new GetPeriodOverviewQuery(["/repo"], ReportPeriod.Monthly, AuthorFilter: "Alice"));

        response.TotalCommits.Should().Be(1);
        response.Repositories[0].Commits[0].AuthorName.Should().Be("Alice");
    }

    [Fact]
    public void Handle_WithAuthorFilter_IsCaseInsensitive()
    {
        _scanner.Scan("/repo").Returns(
            MakeRepo("Repo", [MakeCommit("alice", DateTimeOffset.UtcNow.AddDays(-1))]));

        var response = _handler.Handle(new GetPeriodOverviewQuery(["/repo"], ReportPeriod.Monthly, AuthorFilter: "ALICE"));

        response.TotalCommits.Should().Be(1);
    }

    [Fact]
    public void Handle_WithMultipleRepositories_AggregatesTotalCommits()
    {
        var now = DateTimeOffset.UtcNow;
        _scanner.Scan("/repo1").Returns(MakeRepo("Repo1", [MakeCommit("Alice", now.AddDays(-1))]));
        _scanner.Scan("/repo2").Returns(MakeRepo("Repo2", [MakeCommit("Bob", now.AddDays(-2)), MakeCommit("Bob", now.AddDays(-3))]));

        var response = _handler.Handle(new GetPeriodOverviewQuery(["/repo1", "/repo2"], ReportPeriod.Monthly));

        response.TotalCommits.Should().Be(3);
        response.Repositories.Should().HaveCount(2);
    }

    [Fact]
    public void Handle_OrdersRepositoriesByCommitCountDescending()
    {
        var now = DateTimeOffset.UtcNow;
        _scanner.Scan("/repo1").Returns(MakeRepo("Repo1", [MakeCommit("A", now.AddDays(-1))]));
        _scanner.Scan("/repo2").Returns(MakeRepo("Repo2", [
            MakeCommit("B", now.AddDays(-1)),
            MakeCommit("B", now.AddDays(-2))
        ]));

        var response = _handler.Handle(new GetPeriodOverviewQuery(["/repo1", "/repo2"], ReportPeriod.Monthly));

        response.Repositories[0].RepositoryName.Should().Be("Repo2");
        response.Repositories[1].RepositoryName.Should().Be("Repo1");
    }

    [Fact]
    public void Handle_OrdersCommitsByDateDescending()
    {
        var now = DateTimeOffset.UtcNow;
        var commits = new[]
        {
            MakeCommit("Alice", now.AddDays(-3), "old"),
            MakeCommit("Alice", now.AddDays(-1), "new"),
            MakeCommit("Alice", now.AddDays(-2), "mid"),
        };
        _scanner.Scan("/repo").Returns(MakeRepo("Repo", commits));

        var response = _handler.Handle(new GetPeriodOverviewQuery(["/repo"], ReportPeriod.Monthly));

        var subjects = response.Repositories[0].Commits.Select(c => c.Subject).ToList();
        subjects.Should().ContainInOrder("new", "mid", "old");
    }
}
