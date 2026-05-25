using CommitLens.Application.Reports.GetPeriodOverview;
using Microsoft.Extensions.DependencyInjection;

namespace CommitLens.Cli;

public static class DependencyInjection
{
    public static IServiceCollection AddCliServices(this IServiceCollection services)
    {
        services
            .AddSingleton<GetPeriodOverviewQueryHandler>();
        return services;
    }
}
