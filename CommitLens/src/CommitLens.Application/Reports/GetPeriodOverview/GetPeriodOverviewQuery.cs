namespace CommitLens.Application.Reports.GetPeriodOverview;

public enum ReportPeriod
{
    Daily,
    Weekly,
    Monthly,
    Yearly
}

public record GetPeriodOverviewQuery(
    IReadOnlyList<string> RepositoryPaths,
    ReportPeriod Period,
    string? AuthorFilter = null
    );
