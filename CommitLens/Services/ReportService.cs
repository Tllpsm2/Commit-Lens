using CommitLens.Interfaces;
using CommitLens.Models;

namespace CommitLens.Services;

public class ReportService : IReportService
{
    // Check if there are any commits to report on
    public bool HasCommits(List<CommitEntry> commits)
    {
        return commits?.Count > 0;
    }

    // Takes a list of commits and generates a report
    public CommitReport Build(List<CommitEntry> commits)
    {
        ArgumentNullException.ThrowIfNull(commits);

        var dailyActivity = commits
            .GroupBy(c => c.Date.ToString("yyyy-MM-dd"))
            .ToDictionary(g => g.Key, g => g.Count());
        var authorActivity = commits
            .GroupBy(c => c.AuthorEmail)
            .ToDictionary(g => g.Key, g => g.Count());
        
        return new CommitReport(
            TotalCommits: commits.Count,
            CommitsPerDay: dailyActivity,
            CommitsPerAuthor: authorActivity
        );

    }
}