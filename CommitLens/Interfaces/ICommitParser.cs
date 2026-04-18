using CommitLens.Models;

namespace CommitLens.Interfaces;

public interface ICommitParser
{
    List<CommitEntry> ParseOutput(string rawOutput);
}
