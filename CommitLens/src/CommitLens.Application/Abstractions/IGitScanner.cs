using CommitLens.Domain.Repositories;

namespace CommitLens.Application.Abstractions;

public interface IGitScanner
{
    Repository Scan(string directoryPath);
}
