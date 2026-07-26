using CommitLens.Application.Abstractions;
using CommitLens.Application.Reports.GetActivityHeatMap;
using CommitLens.Domain.Commits;
using CommitLens.Domain.Repositories;
using NSubstitute;

namespace CommitLens.Application.UnitTests.Reports;

public class GetActivityHeatMapQueryHandlerTests
{
    private readonly IGitScanner _scanner = Substitute.For<IGitScanner>();
    private readonly GetActivityHeatMapQueryHandler _handler;

    public GetActivityHeatMapQueryHandlerTests()
    {
        _handler = new GetActivityHeatMapQueryHandler(_scanner);
    }

    private static Commit MakeCommit(string authorName, DateTimeOffset date, string subject = "feat: something") =>
        new(new CommitHash("a1b2c3d"), new Author(authorName, "a@b.com"), date, subject);

    private static Repo MakeRepo(string name, IEnumerable<Commit> commits) =>
        new(name, $"/repos/{name}", ["main"], commits.ToList());

    private static DateTimeOffset At(int year, int month, int day, int hour) =>
        new(year, month, day, hour, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(HeatMapPeriod.Weekly, "Last 7 days")]
    [InlineData(HeatMapPeriod.Monthly, "Last 30 days")]
    [InlineData(HeatMapPeriod.Yearly, "Last 12 months")]
    [InlineData(HeatMapPeriod.AllTime, "All time")]
    public void Handle_ReturnsPeriodLabel(HeatMapPeriod period, string expectedLabel)
    {
        _scanner.Scan(Arg.Any<string>()).Returns(MakeRepo("R", []));

        var response = _handler.Handle(new GetActivityHeatMapQuery(["/p"], period));

        response.Period.Should().Be(period);
        response.PeriodLabel.Should().Be(expectedLabel);
    }

    [Fact]
    public void Handle_ExcludesCommitsOutsideThePeriod()
    {
        var recent = MakeCommit("Alice", DateTimeOffset.UtcNow.AddDays(-2));
        var old = MakeCommit("Alice", DateTimeOffset.UtcNow.AddDays(-20));

        _scanner.Scan("/repo").Returns(MakeRepo("Repo", [recent, old]));

        var response = _handler.Handle(new GetActivityHeatMapQuery(["/repo"], HeatMapPeriod.Weekly));

        response.TotalCommits.Should().Be(1);
        response.DailyCounts.Should().ContainSingle();
    }

    [Fact]
    public void Handle_WithAllTime_IncludesOldestCommits()
    {
        var ancient = MakeCommit("Alice", DateTimeOffset.UtcNow.AddYears(-5));
        var recent = MakeCommit("Alice", DateTimeOffset.UtcNow.AddDays(-1));

        _scanner.Scan("/repo").Returns(MakeRepo("Repo", [ancient, recent]));

        var response = _handler.Handle(new GetActivityHeatMapQuery(["/repo"], HeatMapPeriod.AllTime));

        response.TotalCommits.Should().Be(2);
        response.DailyCounts.Should().HaveCount(2);
    }

    [Fact]
    public void Handle_BinsCommitsByDayOfWeekAndHour()
    {
        var date = At(2026, 7, 24, 14); // deterministic day/hour
        var day = date.DayOfWeek;
        var hour = date.Hour;

        _scanner.Scan("/repo").Returns(MakeRepo("Repo",
        [
            MakeCommit("Alice", date),
            MakeCommit("Alice", date),
            MakeCommit("Bob", date)
        ]));

        var response = _handler.Handle(new GetActivityHeatMapQuery(["/repo"], HeatMapPeriod.AllTime));

        response.TotalCommits.Should().Be(3);
        response.Aggregate[day, hour].Should().Be(3);
        response.Repositories[0].HeatMap[day, hour].Should().Be(3);
    }

    [Fact]
    public void Handle_KeepsDistinctDayHourBucketsSeparate()
    {
        var morning = At(2026, 7, 24, 9);
        var late = At(2026, 7, 25, 23);

        _scanner.Scan("/repo").Returns(MakeRepo("Repo",
        [
            MakeCommit("Alice", morning),
            MakeCommit("Alice", morning),
            MakeCommit("Alice", late)
        ]));

        var response = _handler.Handle(new GetActivityHeatMapQuery(["/repo"], HeatMapPeriod.AllTime));

        response.Aggregate[morning.DayOfWeek, morning.Hour].Should().Be(2);
        response.Aggregate[late.DayOfWeek, late.Hour].Should().Be(1);
        response.Aggregate.Total.Should().Be(3);
    }

    [Fact]
    public void Handle_GroupsCommitsByCalendarDay()
    {
        var first = At(2026, 7, 24, 9);
        var secondSameDay = At(2026, 7, 24, 18);
        var otherDay = At(2026, 7, 25, 10);

        _scanner.Scan("/repo").Returns(MakeRepo("Repo",
        [
            MakeCommit("Alice", first),
            MakeCommit("Alice", secondSameDay),
            MakeCommit("Alice", otherDay)
        ]));

        var response = _handler.Handle(new GetActivityHeatMapQuery(["/repo"], HeatMapPeriod.AllTime));

        response.DailyCounts.Should().HaveCount(2);
        response.DailyCounts[0].Should().Be(new DailyCommitCount(new DateOnly(2026, 7, 24), 2));
        response.DailyCounts[1].Should().Be(new DailyCommitCount(new DateOnly(2026, 7, 25), 1));
    }

    [Fact]
    public void Handle_AggregatesDailyCountsAcrossRepositories()
    {
        var date = At(2026, 7, 24, 10);
        _scanner.Scan("/repo1").Returns(MakeRepo("Repo1", [MakeCommit("A", date), MakeCommit("A", date)]));
        _scanner.Scan("/repo2").Returns(MakeRepo("Repo2", [MakeCommit("B", date)]));

        var response = _handler.Handle(new GetActivityHeatMapQuery(["/repo1", "/repo2"], HeatMapPeriod.AllTime));

        response.TotalCommits.Should().Be(3);
        response.DailyCounts.Should().ContainSingle()
            .Which.Should().Be(new DailyCommitCount(new DateOnly(2026, 7, 24), 3));
    }

    [Fact]
    public void Handle_OrdersRepositoriesByCommitCountDescending()
    {
        var date = At(2026, 7, 24, 10);
        _scanner.Scan("/repo1").Returns(MakeRepo("Repo1", [MakeCommit("A", date)]));
        _scanner.Scan("/repo2").Returns(MakeRepo("Repo2",
        [
            MakeCommit("B", date),
            MakeCommit("B", date.AddDays(1))
        ]));

        var response = _handler.Handle(new GetActivityHeatMapQuery(["/repo1", "/repo2"], HeatMapPeriod.AllTime));

        response.Repositories[0].RepositoryName.Should().Be("Repo2");
        response.Repositories[1].RepositoryName.Should().Be("Repo1");
    }

    [Fact]
    public void Handle_ExcludesRepositoriesWithNoCommits()
    {
        _scanner.Scan("/repo1").Returns(MakeRepo("Repo1", []));
        _scanner.Scan("/repo2").Returns(MakeRepo("Repo2", [MakeCommit("A", At(2026, 7, 24, 10))]));

        var response = _handler.Handle(new GetActivityHeatMapQuery(["/repo1", "/repo2"], HeatMapPeriod.AllTime));

        response.Repositories.Should().ContainSingle();
        response.Repositories[0].RepositoryName.Should().Be("Repo2");
        response.TotalCommits.Should().Be(1);
    }

    [Fact]
    public void Handle_WhenNoCommitsMatch_ReturnsEmptyAndZeroTotal()
    {
        _scanner.Scan("/repo").Returns(MakeRepo("Repo", []));

        var response = _handler.Handle(new GetActivityHeatMapQuery(["/repo"], HeatMapPeriod.AllTime));

        response.TotalCommits.Should().Be(0);
        response.Repositories.Should().BeEmpty();
        response.DailyCounts.Should().BeEmpty();
        response.Aggregate.Max.Should().Be(0);
    }

    [Fact]
    public void Handle_WithAuthorFilter_IncludesOnlyMatchingAuthor()
    {
        var date = At(2026, 7, 24, 11);
        _scanner.Scan("/repo").Returns(MakeRepo("Repo",
        [
            MakeCommit("Alice", date),
            MakeCommit("Bob", date)
        ]));

        var response = _handler.Handle(new GetActivityHeatMapQuery(["/repo"], HeatMapPeriod.AllTime, AuthorFilter: "Alice"));

        response.TotalCommits.Should().Be(1);
        response.Aggregate[date.DayOfWeek, date.Hour].Should().Be(1);
    }

    [Fact]
    public void Handle_WithAuthorFilter_IsCaseInsensitive()
    {
        var date = At(2026, 7, 24, 11);
        _scanner.Scan("/repo").Returns(MakeRepo("Repo", [MakeCommit("alice", date)]));

        var response = _handler.Handle(new GetActivityHeatMapQuery(["/repo"], HeatMapPeriod.AllTime, AuthorFilter: "ALICE"));

        response.TotalCommits.Should().Be(1);
    }

    [Fact]
    public void Handle_AggregatePeakReturnsBusiestSlot()
    {
        var busy = At(2026, 7, 24, 14);
        var quiet = At(2026, 7, 25, 8);
        _scanner.Scan("/repo").Returns(MakeRepo("Repo",
        [
            MakeCommit("A", busy),
            MakeCommit("A", busy),
            MakeCommit("A", busy),
            MakeCommit("A", quiet)
        ]));

        var response = _handler.Handle(new GetActivityHeatMapQuery(["/repo"], HeatMapPeriod.AllTime));

        var (day, hour, count) = response.Aggregate.Peak();
        day.Should().Be(busy.DayOfWeek);
        hour.Should().Be(busy.Hour);
        count.Should().Be(3);
    }
}