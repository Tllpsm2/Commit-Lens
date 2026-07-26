using System.Globalization;
using CommitLens.Application.Reports.GetActivityHeatMap;
using Spectre.Console;

namespace CommitLens.Cli.Rendering;

internal static class AllTimeTimelineView
{
    private static readonly string[] MonthNames =
        { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

    private const string MonthHeaders = "JFMAMJJASOND";

    public static void Render(ActivityHeatMapResponse response)
    {
        var monthly = new Dictionary<(int Year, int Month), int>();
        foreach (var day in response.DailyCounts)
        {
            var key = (day.Date.Year, day.Date.Month);
            monthly[key] = monthly.GetValueOrDefault(key) + day.Count;
        }

        var first = response.DailyCounts.Min(d => d.Date);
        var last = DateOnly.FromDateTime(response.To.Date);
        var max = monthly.Values.Max();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .Title($"[bold]All time[/]  [grey]since {first.ToString("dd MMM yyyy", CultureInfo.InvariantCulture)} " +
                   $"\u00b7 {response.TotalCommits} commits[/]");

        table.AddColumn(new TableColumn("[grey]Year[/]").Width(4));

        for (var month = 0; month < 12; month++)
            table.AddColumn(new TableColumn($"[grey]{MonthHeaders[month]}[/]").Width(1).Centered());

        table.AddColumn(new TableColumn("[grey]Total[/]").Width(5).RightAligned());

        for (var year = first.Year; year <= last.Year; year++)
        {
            var cells = new string[14];
            cells[0] = $"[bold]{year}[/]";
            var yearTotal = 0;

            for (var month = 1; month <= 12; month++)
            {
                var beforeFirst = year == first.Year && month < first.Month;
                var afterLast = year == last.Year && month > last.Month;

                if (beforeFirst || afterLast)
                {
                    cells[month] = " ";
                    continue;
                }

                var count = monthly.GetValueOrDefault((year, month));
                yearTotal += count;

                cells[month] = count == 0
                    ? HeatMapPalette.DotCell
                    : HeatMapPalette.BlockFor(count, max);
            }

            cells[13] = $"[grey]{yearTotal}[/]";
            table.AddRow(cells);
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
        HeatMapPalette.RenderLegend();

        var busiest = monthly.MaxBy(kv => kv.Value);
        AnsiConsole.MarkupLine(
            $"[grey]Busiest month:[/] {MonthNames[busiest.Key.Month - 1]} {busiest.Key.Year} " +
            $"([bold]{busiest.Value}[/] commits)");
    }
}