using CommitLens.Domain.Repositories;

namespace CommitLens.Application.Abstractions;

public interface IGitScanner
{
    Repo Scan(string directoryPath);
}
