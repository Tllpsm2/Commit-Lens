using CommitLens.Application.Reports.GetPeriodOverview;
using Spectre.Console;

namespace CommitLens.Cli.Rendering;

internal sealed class PeriodOverviewRenderer
{
    public void Render(PeriodOverviewResponse response)
    {
        ReportTitle.Write("Period Overview", response.PeriodLabel);
        AnsiConsole.MarkupLine(
            $"[grey]{response.From.ToString("dd MMM yyyy", System.Globalization.CultureInfo.InvariantCulture)} \u2192 " +
            $"{response.To.ToString("dd MMM yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture)}[/]");
        AnsiConsole.MarkupLine($"[bold]Total commits:[/] {response.TotalCommits}");
        AnsiConsole.WriteLine();

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
