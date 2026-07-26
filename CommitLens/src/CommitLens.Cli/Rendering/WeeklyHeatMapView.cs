using System.Text;
using CommitLens.Application.Reports.GetActivityHeatMap;
using Spectre.Console;

namespace CommitLens.Cli.Rendering;

internal static class WeeklyHeatMapView
{
    private static readonly DayOfWeek[] DayOrder =
    {
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
        DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
    };

    private static readonly string[] DayLabels = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

    // Block characters progress from low to high density, paired with the
    // GitHub-style green gradient (darkest -> lightest) so each tier is
    // visible as "low activity" rather than a hole, even at the lowest tier.
    private const char BlockLow = '\u2591'; // ░
    private const char BlockMid = '\u2592'; // ▒
    private const char BlockHigh = '\u2593'; // ▓
    private const char BlockPeak = '\u2588'; // █

    private static readonly string[] TierColors = { "#0e4429", "#006d32", "#26a641", "#39d353" };

    private const string HourAxis = "     00    06    12    18    ";

    public static void Render(ActivityHeatMapResponse response)
    {
        var grids = new List<(string Title, HeatMapGrid Grid)>();

        if (response.Repositories.Count > 1)
            grids.Add(("Aggregate", response.Aggregate));

        grids.AddRange(response.Repositories.Select(r => (r.RepositoryName, r.HeatMap)));

        for (var i = 0; i < grids.Count; i++)
            RenderGrid(grids[i].Title, grids[i].Grid, showLegend: i == 0);
    }

    private static void RenderGrid(string title, HeatMapGrid grid, bool showLegend)
    {
        AnsiConsole.MarkupLine($"[bold]{Markup.Escape(title)}[/]  [grey]({grid.Total} commits)[/]");

        for (var i = 0; i < DayOrder.Length; i++)
        {
            var day = DayOrder[i];
            var row = new StringBuilder();
            row.Append(DayLabels[i]).Append("  ");

            for (var h = 0; h < 24; h++)
            {
                var count = grid[day, h];
                row.Append(CellMarkup(count));
            }

            AnsiConsole.Markup(row.ToString());
            AnsiConsole.WriteLine();
        }

        AnsiConsole.MarkupLine($"[grey]{HourAxis}[/]");

        if (showLegend)
            RenderLegend();

        var (peakDay, peakHour, peakCount) = grid.Peak();
        AnsiConsole.MarkupLine(
            $"[grey]Busiest slot:[/] {peakDay} {peakHour:D2}:00 " +
            $"([bold]{peakCount}[/] commit{(peakCount == 1 ? "" : "s")})");
        AnsiConsole.WriteLine();
    }

    private static string CellMarkup(int count)
    {
        if (count <= 0)
            return ".";

        var tier = TierFor(count);
        return $"[{TierColors[tier]}]{TierChar(tier)}[/]";
    }

    private static int TierFor(int count) => count switch
    {
        1 => 0,
        2 => 1,
        >= 3 and <= 4 => 2,
        _ => 3
    };

    private static char TierChar(int tier) => tier switch
    {
        0 => BlockLow,
        1 => BlockMid,
        2 => BlockHigh,
        _ => BlockPeak
    };

    private static void RenderLegend()
    {
        AnsiConsole.MarkupLine(
            $"[grey]Legend:[/] [grey]\u00b7[/] 0  " +
            $"[{TierColors[0]}]{BlockLow}[/] 1  " +
            $"[{TierColors[1]}]{BlockMid}[/] 2  " +
            $"[{TierColors[2]}]{BlockHigh}[/] 3-4  " +
            $"[{TierColors[3]}]{BlockPeak}[/] 5+");
    }
}
