using CommitLens.Application.Common;

namespace CommitLens.Application.Reports.GetPeriodOverview;

public record PeriodOverviewResponse(
    string PeriodLabel,
    DateTimeOffset From,
    DateTimeOffset To,
    int TotalCommits,
    IReadOnlyList<RepositoryCommitsDto> Repositories
);

public record RepositoryCommitsDto(
    string RepositoryName,
    int CommitCount,
    IReadOnlyList<CommitDto> Commits
);
