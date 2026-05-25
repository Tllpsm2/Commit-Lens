using CommitLens.Application.Abstractions;
using CommitLens.Application.Reports.GetPeriodOverview;
using CommitLens.Cli;
using CommitLens.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

var services = new ServiceCollection()
    .AddInfrastructure()
    .AddCliServices()
    .BuildServiceProvider();

var handler = services.GetRequiredService<GetPeriodOverviewQueryHandler>();
var repositoryLocator = services.GetRequiredService<IRepositoryLocator>();

AnsiConsole.Write(new FigletText("CommitLens").Color(Color.CornflowerBlue));
AnsiConsole.MarkupLine("[grey]Git repository commit analyser[/]\n");

AnsiConsole.MarkupLine("[bold]Enter repository paths[/] (one per line, empty line to finish):");

var paths = new List<string>();

while (true)
{
    var input = (await AnsiConsole.PromptAsync(
        new TextPrompt<string>("[blue]>[/]")
            .AllowEmpty()))?.Trim() ?? string.Empty;

    if (string.IsNullOrWhiteSpace(input))
        break;

    if (!Directory.Exists(input))
    {
        AnsiConsole.MarkupLine($"[red]Directory not found:[/] {input}");
        continue;
    }

    IReadOnlyList<string> resolvedPaths;
    try
    {
        resolvedPaths = repositoryLocator.FindRepositories(input);
    }
    catch (Exception ex)
    {
        AnsiConsole.MarkupLine($"[red]Error searching for Git repositories:[/] {ex.Message}");
        continue;
    }

    if (resolvedPaths.Count == 0)
    {
        AnsiConsole.MarkupLine($"[red]No Git repositories found in or under:[/] {input}");
        continue;
    }

    foreach (var resolvedPath in resolvedPaths)
    {
        if (!paths.Contains(resolvedPath))
        {
            paths.Add(resolvedPath);
            AnsiConsole.MarkupLine($"[green]Added repository:[/] {resolvedPath}");
        }
        else
        {
            AnsiConsole.MarkupLine($"[yellow]Repository already added:[/] {resolvedPath}");
        }
    }
}

if (paths.Count == 0)
{
    AnsiConsole.MarkupLine("[red]No valid repositories provided. Exiting.[/]");
    return;
}

var period = await AnsiConsole.PromptAsync(
    new SelectionPrompt<ReportPeriod>()
        .Title("\nSelect [bold]report period[/]:")
        .AddChoices(ReportPeriod.Daily, ReportPeriod.Weekly, ReportPeriod.Monthly, ReportPeriod.Yearly));

var filterInput = (await AnsiConsole.PromptAsync(
    new TextPrompt<string>("\nFilter by author name? [grey](leave empty to skip)[/]:")
        .AllowEmpty()))?.Trim() ?? string.Empty;
var authorFilter = string.IsNullOrWhiteSpace(filterInput) ? null : filterInput;

PeriodOverviewResponse response = null!;

AnsiConsole.Status()
    .Spinner(Spinner.Known.Dots)
    .SpinnerStyle(Style.Parse("cornflowerblue"))
    .Start("Scanning repositories...", _ =>
    {
        response = handler.Handle(new GetPeriodOverviewQuery(paths, period, authorFilter));
    });

AnsiConsole.WriteLine();
AnsiConsole.MarkupLine($"[bold cornflowerblue]{response.PeriodLabel}[/]  " +
                       $"[grey]{response.From:dd MMM yyyy} → {response.To:dd MMM yyyy HH:mm}[/]");
AnsiConsole.MarkupLine($"[bold]Total commits:[/] {response.TotalCommits}\n");

if (response.TotalCommits == 0)
{
    AnsiConsole.MarkupLine("[grey]No commits found for the selected period.[/]");
    return;
}

foreach (var repo in response.Repositories)
{
    var table = new Table()
        .BorderColor(Color.Grey)
        .Title($"[bold]{Markup.Escape(repo.RepositoryName)}[/]  [grey]({repo.CommitCount} commits)[/]")
        .AddColumn(new TableColumn("[grey]Hash[/]").Width(9))
        .AddColumn(new TableColumn("[grey]Author[/]").Width(22))
        .AddColumn(new TableColumn("[grey]When[/]").Width(14))
        .AddColumn(new TableColumn("[grey]Subject[/]"));

    foreach (var commit in repo.Commits)
    {
        table.AddRow(
            $"[yellow]{Markup.Escape(commit.Hash)}[/]",
            Markup.Escape(commit.AuthorName),
            $"[grey]{Markup.Escape(commit.RelativeDate)}[/]",
            Markup.Escape(commit.Subject));
    }

    AnsiConsole.Write(table);
    AnsiConsole.WriteLine();
}