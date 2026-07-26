namespace CommitLens.Application.Reports.GetActivityHeatMap;

public enum HeatMapPeriod
{
    Weekly,
    Monthly,
    Yearly,
    AllTime
}

public record GetActivityHeatMapQuery(
    IReadOnlyList<string> RepositoryPaths,
    HeatMapPeriod Period,
    string? AuthorFilter = null
);