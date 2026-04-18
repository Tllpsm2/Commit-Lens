
using CommitLens.Interfaces;
using CommitLens.Models;
using Spectre.Console;

namespace CommitLens.Services;
public class ConsoleRenderer : IViewRenderer
{
    public void ShowRecentCommits(List<CommitEntry> commits, int days, int maxCommitsToShow)
    {
        if (!HasCommits(commits, days))
            return;

        // Complete table setup
        var commitsToShow = commits.Take(maxCommitsToShow).ToList();
        TitleRule($"CommitLens - Last {commitsToShow.Count} Commits");
        var fullTable = new Table()
            .BorderColor(Color.Grey84)
            .ShowRowSeparators()
            .AsciiBorder()
            .AddColumn(new TableColumn("[bold]Hash[/]").Centered())
            .AddColumn(new TableColumn("[bold]Author[/]").Centered())
            .AddColumn(new TableColumn("[bold]Date[/]").Centered())
            .AddColumn(new TableColumn("[bold]Message[/]").LeftAligned());
        foreach (var c in commitsToShow)
        {
            fullTable.AddRow(
                $"[green]{Markup.Escape(c.Hash)}[/]",
                $"[blue]{Markup.Escape(c.AuthorName)}[/]",
                $"[dim]{c.Date:yyyy-MM-dd HH:mm}[/]",
                Markup.Escape(c.Message)
            );
        }
        AnsiConsole.Write(fullTable);
    }

    private bool HasCommits(List<Models.CommitEntry> commits, int days)
    {
        if (commits?.Count > 0)
            return true;

        AnsiConsole.MarkupLine($"[yellow]No commits found in the last {days} days.[/]");
        return false;
    }

    public void ShowCommitReport(CommitReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        TitleRule("CommitLens - Commit Report");

        var summary = new Table()
            .BorderColor(Color.Grey84)
            .AsciiBorder()
            .AddColumn(new TableColumn("[bold]Metric[/]").LeftAligned())
            .AddColumn(new TableColumn("[bold]Value[/]").RightAligned());

        summary.AddRow("[bold]Total commits[/]", report.TotalCommits.ToString());
        summary.AddRow("Days with activity", report.CommitsPerDay.Count.ToString());
        summary.AddRow("Authors", report.CommitsPerAuthor.Count.ToString());

        AnsiConsole.Write(summary);
    }

    void IViewRenderer.ShowCommitReport(CommitReport report)
    {
        ShowCommitReport(report);
    }

    void IViewRenderer.ShowRecentCommits(List<CommitEntry> commits, int days, int maxCommitsToShow)
    {
        ShowRecentCommits(commits, days, maxCommitsToShow);
    }

    private void TitleRule(string title)
    {
        var rule = new Rule()
            .RuleTitle($"[bold]{Markup.Escape(title)}[/]")
            .RuleStyle(Style.Parse("bold"));
        AnsiConsole.Write(rule);
    }

/* fazer o service de relatório de commits por dia e por autor, e depois usar o console renderer para mostrar os dados. 
    public static void ShowCommitReport(List<Models.CommitEntry> commits)
    {
        if (!HasCommits(commits))
            return;

        TitleRule("CommitLens - Commits per day and per author");
        // commits PerAuthor
        

        // TBA 
        /// acho melhor separar em dois métodos um, PerDay outro PerAuthor
    }
    */
}