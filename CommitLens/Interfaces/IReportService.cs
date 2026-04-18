using CommitLens.Models;

namespace CommitLens.Interfaces;

public interface IReportService
{
    CommitReport Build(List<CommitEntry> commits);
}
