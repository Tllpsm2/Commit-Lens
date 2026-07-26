using CommitLens.Application.Abstractions;

namespace CommitLens.Application.Reports.GetActivityHeatMap;

public sealed class GetActivityHeatMapQueryHandler
{
    private readonly IGitScanner _scanner;

    public GetActivityHeatMapQueryHandler(IGitScanner scanner)
    {
        _scanner = scanner;
    }

    public ActivityHeatMapResponse Handle(GetActivityHeatMapQuery query)
    {
        var (from, to, label) = ResolvePeriod(query.Period);

        var repositories = query.RepositoryPaths
            .Select(_scanner.Scan)
            .Select(repo =>
            {
                IEnumerable<Domain.Commits.Commit> commits = repo.GetCommitsInPeriod(from, to);

                if (query.AuthorFilter is not null)
                    commits = commits.Where(c =>
                        c.Author.Name.Equals(query.AuthorFilter, StringComparison.OrdinalIgnoreCase));

                var grid = new HeatMapGrid();
                var daily = new Dictionary<DateOnly, int>();

                foreach (var commit in commits)
                {
                    grid.Increment(commit.Date.DayOfWeek, commit.Date.Hour);
                    var date = DateOnly.FromDateTime(commit.Date.Date);
                    daily[date] = daily.GetValueOrDefault(date) + 1;
                }

                var dailyCounts = daily
                    .Select(kv => new DailyCommitCount(kv.Key, kv.Value))
                    .OrderBy(d => d.Date)
                    .ToList();

                return new RepositoryHeatMapDto(repo.Name, grid.Total, grid, dailyCounts);
            })
            .Where(r => r.CommitCount > 0)
            .OrderByDescending(r => r.CommitCount)
            .ToList();

        var aggregate = new HeatMapGrid();
        var aggregateDaily = new Dictionary<DateOnly, int>();

        foreach (var repo in repositories)
        {
            aggregate.Merge(repo.HeatMap);
            foreach (var day in repo.DailyCounts)
                aggregateDaily[day.Date] = aggregateDaily.GetValueOrDefault(day.Date) + day.Count;
        }

        var aggregateDailyCounts = aggregateDaily
            .Select(kv => new DailyCommitCount(kv.Key, kv.Value))
            .OrderBy(d => d.Date)
            .ToList();

        return new ActivityHeatMapResponse(
            query.Period,
            label,
            from,
            to,
            aggregate.Total,
            aggregate,
            aggregateDailyCounts,
            repositories);
    }

    private static (DateTimeOffset From, DateTimeOffset To, string Label) ResolvePeriod(HeatMapPeriod period)
    {
        var now = DateTimeOffset.Now;
        return period switch
        {
            HeatMapPeriod.Weekly => (now.AddDays(-7), now, "Last 7 days"),
            HeatMapPeriod.Monthly => (now.AddDays(-30), now, "Last 30 days"),
            HeatMapPeriod.Yearly => (now.AddDays(-365), now, "Last 12 months"),
            HeatMapPeriod.AllTime => (DateTimeOffset.MinValue, now, "All time"),
            _ => throw new ArgumentOutOfRangeException(nameof(period))
        };
    }
}