using Spectre.Console;

namespace CommitLens.Cli.Rendering;

internal static class ReportTitle
{
    public const string OpenRule = "\u2500\u2500";
    public const string CloseRule = "\u2500\u2500";

    public static void Write(string reportName, string qualifier)
    {
        var title = string.IsNullOrEmpty(qualifier)
            ? $" {reportName} "
            : $" ({qualifier}) {reportName} ";

        AnsiConsole.Write(
            new Rule($"[bold cornflowerblue]{Markup.Escape(title)}[/]")
                .LeftJustified()
                .RuleStyle("grey"));
    }
}
