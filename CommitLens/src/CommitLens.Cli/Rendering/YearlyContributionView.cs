using System.Globalization;
using System.Text;
using CommitLens.Application.Reports.GetActivityHeatMap;
using Spectre.Console;

namespace CommitLens.Cli.Rendering;

internal static class YearlyContributionView
{
    private static readonly string[] DayLabels = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

    private const char BlockLow = '\u2591';
    private const char BlockMid = '\u2592';
    private const char BlockHigh = '\u2593';
    private const char BlockPeak = '\u2588';

    private static readonly string[] TierColors = { "#0e4429", "#006d32", "#26a641", "#39d353" };

    public static void Render(ActivityHeatMapResponse response)
    {
        var counts = response.DailyCounts.ToDictionary(d => d.Date, d => d.Count);

        var from = DateOnly.FromDateTime(response.From.Date);
        var to = DateOnly.FromDateTime(response.To.Date);

        var firstWeek = StartOfWeek(from);
        var weeks = (to.DayNumber - firstWeek.DayNumber) / 7 + 1;

        RenderMonthLabels(firstWeek, weeks, to);

        for (var dayIndex = 0; dayIndex < 7; dayIndex++)
        {
            var row = new StringBuilder();
            row.Append(DayLabels[dayIndex]).Append("  ");

            for (var week = 0; week < weeks; week++)
            {
                var date = firstWeek.AddDays(week * 7 + dayIndex);
                row.Append(CellMarkup(date, from, to, counts.GetValueOrDefault(date)));
            }

            AnsiConsole.Markup(row.ToString());
            AnsiConsole.WriteLine();
        }

        AnsiConsole.WriteLine();
        RenderLegend();
    }

    private static string CellMarkup(DateOnly date, DateOnly from, DateOnly to, int count)
    {
        if (date < from || date > to)
            return " ";

        if (count <= 0)
            return "[grey]\u00b7[/]";

        return count switch
        {
            1 => $"[{TierColors[0]}]{BlockLow}[/]",
            2 => $"[{TierColors[1]}]{BlockMid}[/]",
            >= 3 and <= 4 => $"[{TierColors[2]}]{BlockHigh}[/]",
            _ => $"[{TierColors[3]}]{BlockPeak}[/]"
        };
    }

    private static void RenderMonthLabels(DateOnly firstWeek, int weeks, DateOnly to)
    {
        var labels = new char[weeks];
        Array.Fill(labels, ' ');

        var cursor = 0;
        for (var week = 0; week < weeks; week++)
        {
            var weekStart = firstWeek.AddDays(week * 7);

            var monthStart = FirstDayOfMonthInWeek(weekStart, to);
            if (monthStart is null)
                continue;

            var abbrev = monthStart.Value.ToString("MMM", CultureInfo.InvariantCulture);

            if (cursor > week)
                continue;

            var target = week;
            if (target < cursor)
                target = cursor;

            if (target + abbrev.Length > weeks)
                continue;

            for (var c = 0; c < abbrev.Length; c++)
                labels[target + c] = abbrev[c];

            cursor = target + abbrev.Length;
        }

        AnsiConsole.MarkupLine($"[grey]     {new string(labels)}[/]");
    }

    private static DateOnly? FirstDayOfMonthInWeek(DateOnly weekStart, DateOnly to)
    {
        for (var d = 0; d < 7; d++)
        {
            var date = weekStart.AddDays(d);
            if (date > to)
                return null;
            if (date.Day == 1)
                return date;
        }
        return null;
    }

    private static void RenderLegend()
    {
        AnsiConsole.MarkupLine(
            $"[grey]Legend:[/] [grey]\u00b7[/] 0  " +
            $"[{TierColors[0]}]{BlockLow}[/] 1  " +
            $"[{TierColors[1]}]{BlockMid}[/] 2  " +
            $"[{TierColors[2]}]{BlockHigh}[/] 3-4  " +
            $"[{TierColors[3]}]{BlockPeak}[/] 5+");
    }

    private static DateOnly StartOfWeek(DateOnly date) =>
        date.AddDays(-(((int)date.DayOfWeek + 6) % 7));
}
