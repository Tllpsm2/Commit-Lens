using CommitLens.Application.Abstractions;
using CommitLens.Infrastructure.FileSystem;
using CommitLens.Infrastructure.Git;
using Microsoft.Extensions.DependencyInjection;

namespace CommitLens.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IGitScanner, CliGitScanner>();
        services.AddSingleton<IRepositoryLocator, FileSystemLocator>();
        return services;
    }
}
