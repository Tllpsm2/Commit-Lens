using CommitLens.Models;

namespace CommitLens.Services;

public static class ReportService
{
    public static CommitReport Build(List<CommitEntry> commits)
    {
        ArgumentNullException.ThrowIfNull(commits);

        var report = new CommitReport(
            commits.Count,
            commits
                .GroupBy(c => c.Date.ToString("yyyy-MM-dd"))
                .ToDictionary(g => g.Key, g => g.Count()),
            commits
                .GroupBy(c => c.AuthorEmail)
                .ToDictionary(g => g.Key, g => g.Count())
        );
        
        return report;
    }
}