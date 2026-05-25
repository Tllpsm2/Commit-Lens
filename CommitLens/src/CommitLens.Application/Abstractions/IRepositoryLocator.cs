namespace CommitLens.Application.Abstractions;

public interface IRepositoryLocator
{
    IReadOnlyList<string> FindRepositories(string rootPath);
}
