using CommitLens.Infrastructure;
using CommitLens.Interfaces;
using CommitLens.Services;
using Microsoft.Extensions.DependencyInjection;

// Setup dependency injection
var services = new ServiceCollection();
services.AddTransient<ICommitParser, CommitParser>();
services.AddTransient<IGitLogProvider, CliGitLogProvider>();
services.AddTransient<IRepositoryLocator, FileSystemLocator>();
services.AddTransient<IReportService, ReportService>();
services.AddTransient<IViewRenderer, ConsoleRenderer>();
services.AddTransient<AppRunner>();

// Build the service provider and run the application
using var serviceProvider = services.BuildServiceProvider();
var app = serviceProvider.GetRequiredService<AppRunner>();

await app.RunAsync();