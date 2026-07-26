using CommitLens.Application.Reports.GetActivityHeatMap;
using CommitLens.Application.Reports.GetPeriodOverview;
using Microsoft.Extensions.DependencyInjection;

namespace CommitLens.Cli.Composition;

public static class DependencyInjection
{
    public static IServiceCollection AddCliServices(this IServiceCollection services)
    {
        services
            .AddSingleton<GetPeriodOverviewQueryHandler>()
            .AddSingleton<GetActivityHeatMapQueryHandler>()
            .AddSingleton<ReportSession>();
        return services;
    }
}
