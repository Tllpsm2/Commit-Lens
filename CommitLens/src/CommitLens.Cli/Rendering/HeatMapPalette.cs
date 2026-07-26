using Spectre.Console;

namespace CommitLens.Cli.Rendering;

internal static class HeatMapPalette
{
    public const string EmptyColor = "#21262d";
    public const string DotCell = "[grey]\u00b7[/]";

    private static readonly string[] TierColors = { "#0e4429", "#006d32", "#26a641", "#39d353" };

    public static int TierFor(int count, int max)
    {
        if (count <= 0 || max <= 0)
            return -1;

        var ratio = (double)count / max;
        return ratio switch
        {
            <= 0.25 => 0,
            <= 0.50 => 1,
            <= 0.75 => 2,
            _ => 3
        };
    }

    public static string ColorFor(int count, int max)
    {
        var tier = TierFor(count, max);
        return tier < 0 ? EmptyColor : TierColors[tier];
    }

    public static string BlockFor(int count, int max) => $"[{ColorFor(count, max)}]\u2588[/]";

    public static void RenderLegend()
    {
        AnsiConsole.MarkupLine(
            $"[grey]Less[/] [{EmptyColor}]\u2588[/]" +
            $"[{TierColors[0]}]\u2588[/][{TierColors[1]}]\u2588[/][{TierColors[2]}]\u2588[/][{TierColors[3]}]\u2588[/]" +
            " [grey]More[/]");
    }
}