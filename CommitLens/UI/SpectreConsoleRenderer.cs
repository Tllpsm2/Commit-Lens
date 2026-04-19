
using CommitLens.Interfaces;
using CommitLens.Models;
using Spectre.Console;

namespace CommitLens.Services;
public class ConsoleRenderer : IViewRenderer
{
    public void RenderNoGitRepoMessage(string repoPath)
    {
        AnsiConsole.MarkupLine($"[red]Error: No Git repository found in: \n{repoPath}[/]");
    }
    public void RenderNoCommitsMessage(int days)
    {
        AnsiConsole.MarkupLine($"[yellow]No commits found in the last {days} days.[/]");
    }

    public void RenderCommitsTable(List<CommitEntry> commits, int days, int maxCommitsToShow)
    {

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

    public void RenderCommitStatistics(CommitReport report)
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

    void IViewRenderer.RenderCommitStatistics(CommitReport report)
    {
        RenderCommitStatistics(report);
    }

    void IViewRenderer.RenderCommitsTable(List<CommitEntry> commits, int days, int maxCommitsToShow)
    {
        RenderCommitsTable(commits, days, maxCommitsToShow);
    }

    private void TitleRule(string title)
    {
        var rule = new Rule()
            .RuleTitle($"[bold]{Markup.Escape(title)}[/]")
            .RuleStyle(Style.Parse("bold"));
        AnsiConsole.Write(rule);
    }
}