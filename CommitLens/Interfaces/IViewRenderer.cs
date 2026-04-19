using System;

namespace CommitLens.Interfaces;

public interface IViewRenderer
{
    
    void RenderNoGitRepoMessage(string repoPath);
    void RenderNoCommitsMessage(int days);
    void RenderCommitsTable(List<Models.CommitEntry> commits, int days, int maxCommitsToShow);
    void RenderCommitStatistics(Models.CommitReport report);
}
