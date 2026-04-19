using CommitLens.Models;

namespace CommitLens.Interfaces;

public interface IReportService
{
    bool HasCommits(List<CommitEntry> commits);
    CommitReport Build(List<CommitEntry> commits);
}
