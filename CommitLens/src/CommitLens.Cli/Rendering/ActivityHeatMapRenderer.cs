using CommitLens.Application.Reports.GetActivityHeatMap;
using Spectre.Console;

namespace CommitLens.Cli.Rendering;

internal sealed class ActivityHeatMapRenderer
{
    public void Render(ActivityHeatMapResponse response)
    {
        ReportTitle.Write("Activity Heat Map", response.PeriodLabel);
        RenderSummary(response);

        if (response.TotalCommits == 0)
        {
            AnsiConsole.MarkupLine("[grey]No commits found for the selected period.[/]");
            return;
        }

        switch (response.Period)
        {
            case HeatMapPeriod.Weekly:
                WeeklyHeatMapView.Render(response);
                break;
            case HeatMapPeriod.Monthly:
                MonthlyCalendarView.Render(response);
                RenderRepositoryBreakdown(response);
                break;
            case HeatMapPeriod.Yearly:
                YearlyContributionView.Render(response);
                RenderRepositoryBreakdown(response);
                break;
            case HeatMapPeriod.AllTime:
                AllTimeTimelineView.Render(response);
                RenderRepositoryBreakdown(response);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(response.Period));
        }
    }

    private static void RenderSummary(ActivityHeatMapResponse response)
    {
        var repoWord = response.Repositories.Count == 1 ? "repository" : "repositories";
        AnsiConsole.MarkupLine(
            $"[bold]Total commits:[/] {response.TotalCommits} " +
            $"[grey]across {response.Repositories.Count} {repoWord}[/]");
        AnsiConsole.WriteLine();
    }

    private static void RenderRepositoryBreakdown(ActivityHeatMapResponse response)
    {
        if (response.Repositories.Count < 2)
            return;

        AnsiConsole.WriteLine();

        var chart = new BarChart()
            .Width(60)
            .Label("[bold]Commits per repository[/]");

        foreach (var repo in response.Repositories)
            chart.AddItem(Markup.Escape(repo.RepositoryName), repo.CommitCount, Color.Green);

        AnsiConsole.Write(chart);
    }
}