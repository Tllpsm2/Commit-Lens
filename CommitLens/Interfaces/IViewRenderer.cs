using System;

namespace CommitLens.Interfaces;

public interface IViewRenderer
{
    void ShowRecentCommits(List<Models.CommitEntry> commits, int days, int maxCommitsToShow);
    void ShowCommitReport(Models.CommitReport report);
}
