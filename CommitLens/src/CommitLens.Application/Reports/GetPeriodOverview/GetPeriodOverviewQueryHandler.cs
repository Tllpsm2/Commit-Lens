using CommitLens.Application.Abstractions;
using CommitLens.Application.Common;

namespace CommitLens.Application.Reports.GetPeriodOverview;

public sealed class GetPeriodOverviewQueryHandler
{
    private readonly IGitScanner _scanner;

    public GetPeriodOverviewQueryHandler(IGitScanner scanner)
    {
        _scanner = scanner;
    }

    public PeriodOverviewResponse Handle(GetPeriodOverviewQuery query)
    {
        var (from, to, label) = ResolvePeriod(query.Period);

        var repositories = query.RepositoryPaths
            .Select(_scanner.Scan)
            .Select(repo =>
            {
                var commits = repo.GetCommitsInPeriod(from, to);

                if (query.AuthorFilter is not null)
                    commits = commits.Where(c =>
                        c.Author.Name.Equals(query.AuthorFilter, StringComparison.OrdinalIgnoreCase));

                var commitDtos = commits
                    .OrderByDescending(c => c.Date)
                    .Select(c => new CommitDto(
                        c.Hash.Abbreviated,
                        c.Author.Name,
                        c.Author.Email,
                        c.Date,
                        ToRelativeDate(c.Date),
                        c.Subject,
                        repo.Name))
                    .ToList();

                return new RepositoryCommitsDto(repo.Name, commitDtos.Count, commitDtos);
            })
            .Where(r => r.CommitCount > 0)
            .OrderByDescending(r => r.CommitCount)
            .ToList();

        return new PeriodOverviewResponse(
            label, from, to,
            repositories.Sum(r => r.CommitCount),
            repositories);
    }

    private static (DateTimeOffset From, DateTimeOffset To, string Label) ResolvePeriod(ReportPeriod period)
    {
        var now = DateTimeOffset.Now;
        return period switch
        {
            ReportPeriod.Daily => (now.AddDays(-1), now, "Last 24 hours"),
            ReportPeriod.Weekly => (now.AddDays(-7), now, "Last 7 days"),
            ReportPeriod.Monthly => (now.AddDays(-30), now, "Last 30 days"),
            ReportPeriod.Yearly => (now.AddDays(-365), now, "Last 12 months"),
            _ => throw new ArgumentOutOfRangeException(nameof(period))
        };
    }

    private static string ToRelativeDate(DateTimeOffset date)
    {
        var diff = DateTimeOffset.Now - date;
        return diff.TotalMinutes < 1 ? "just now"
            : diff.TotalHours < 1 ? $"{(int)diff.TotalMinutes}m ago"
            : diff.TotalDays < 1 ? $"{(int)diff.TotalHours}h ago"
            : diff.TotalDays < 30 ? $"{(int)diff.TotalDays}d ago"
            : diff.TotalDays < 365 ? $"{(int)(diff.TotalDays / 30)}mo ago"
            : $"{(int)(diff.TotalDays / 365)}y ago";
    }
}
