using System.Globalization;
using CommitLens.Application.Reports.GetActivityHeatMap;
using Spectre.Console;

namespace CommitLens.Cli.Rendering;

internal static class MonthlyCalendarView
{
    private static readonly string[] DayNames = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

    public static void Render(ActivityHeatMapResponse response)
    {
        var counts = response.DailyCounts.ToDictionary(d => d.Date, d => d.Count);
        var max = response.DailyCounts.Count > 0 ? response.DailyCounts.Max(d => d.Count) : 0;

        var from = DateOnly.FromDateTime(response.From.Date);
        var to = DateOnly.FromDateTime(response.To.Date);
        var today = DateOnly.FromDateTime(DateTime.Today);

        var firstWeek = StartOfWeek(from);
        var lastWeek = StartOfWeek(to);

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .Title($"[bold]{Markup.Escape(response.PeriodLabel)}[/]  " +
                   $"[grey]{from.ToString("dd MMM", CultureInfo.InvariantCulture)} \u2192 " +
                   $"{to.ToString("dd MMM", CultureInfo.InvariantCulture)} \u00b7 {response.TotalCommits} commits[/]");

        foreach (var dayName in DayNames)
            table.AddColumn(new TableColumn($"[grey]{dayName}[/]").Width(4).Centered());

        for (var week = firstWeek; week <= lastWeek; week = week.AddDays(7))
        {
            var cells = new string[7];

            for (var i = 0; i < 7; i++)
            {
                var date = week.AddDays(i);

                if (date < from || date > to)
                {
                    cells[i] = "    ";
                    continue;
                }

                var dayText = $" {date.Day,2} ";
                var count = counts.GetValueOrDefault(date);
                var isToday = date == today;

                if (count == 0)
                {
                    cells[i] = isToday ? $"[bold grey]{dayText}[/]" : $"[grey]{dayText}[/]";
                    continue;
                }

                var tier = HeatMapPalette.TierFor(count, max);
                var foreground = tier <= 1 ? "white" : "black";
                var style = isToday ? $"bold {foreground}" : foreground;
                cells[i] = $"[{style} on {HeatMapPalette.ColorFor(count, max)}]{dayText}[/]";
            }

            table.AddRow(cells);
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
        HeatMapPalette.RenderLegend();
        AnsiConsole.MarkupLine("[grey]Bold = today[/]");
    }

    private static DateOnly StartOfWeek(DateOnly date) =>
        date.AddDays(-(((int)date.DayOfWeek + 6) % 7));
}