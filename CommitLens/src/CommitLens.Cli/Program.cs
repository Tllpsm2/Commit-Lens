using CommitLens.Cli;
using CommitLens.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection()
    .AddInfrastructure()
    .AddCliServices()
    .BuildServiceProvider();

await services.GetRequiredService<ReportSession>().RunAsync();
