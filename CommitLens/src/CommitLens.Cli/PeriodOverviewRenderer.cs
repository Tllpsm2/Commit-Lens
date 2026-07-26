using CommitLens.Application.Reports.GetPeriodOverview;
using Spectre.Console;

namespace CommitLens.Cli;

internal sealed class PeriodOverviewRenderer
{
    public void Render(PeriodOverviewResponse response)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold cornflowerblue]{response.PeriodLabel}[/]  " +
                               $"[grey]{response.From:dd MMM yyyy} \u2192 {response.To:dd MMM yyyy HH:mm}[/]");
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
    }
}
