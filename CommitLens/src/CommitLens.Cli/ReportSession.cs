using CommitLens.Application.Abstractions;
using CommitLens.Application.Reports.GetPeriodOverview;
using Spectre.Console;

namespace CommitLens.Cli;

internal sealed class ReportSession
{
    private readonly RepositoryPathCollector _pathCollector;
    private readonly PeriodOverviewRenderer _renderer;
    private readonly GetPeriodOverviewQueryHandler _handler;

    public ReportSession(IRepositoryLocator locator, GetPeriodOverviewQueryHandler handler)
    {
        _pathCollector = new RepositoryPathCollector(locator);
        _renderer = new PeriodOverviewRenderer();
        _handler = handler;
    }

    public async Task RunAsync()
    {
        RenderBanner();

        IReadOnlyList<string> paths = Array.Empty<string>();

        while (true)
        {
            paths = await _pathCollector.CollectAsync(paths);

            if (paths.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No valid repositories provided. Exiting.[/]");
                return;
            }

            var period = await PromptPeriodAsync();
            var authorFilter = await PromptAuthorFilterAsync();

            var response = RunReport(paths, period, authorFilter);
            _renderer.Render(response);

            if (!await PromptContinueAsync())
                return;
        }
    }

    private static void RenderBanner()
    {
        AnsiConsole.Write(new FigletText("CommitLens").Color(Color.CornflowerBlue));
        AnsiConsole.MarkupLine("[grey]Git repository commit analyser[/]\n");
    }

    private static Task<ReportPeriod> PromptPeriodAsync() =>
        AnsiConsole.PromptAsync(
            new SelectionPrompt<ReportPeriod>()
                .Title("\nSelect [bold]report period[/]:")
                .AddChoices(ReportPeriod.Daily, ReportPeriod.Weekly, ReportPeriod.Monthly, ReportPeriod.Yearly));

    private static async Task<string?> PromptAuthorFilterAsync()
    {
        var input = (await AnsiConsole.PromptAsync(
            new TextPrompt<string>("\nFilter by author name? [grey](leave empty to skip)[/]:")
                .AllowEmpty()))?.Trim() ?? string.Empty;
        return string.IsNullOrWhiteSpace(input) ? null : input;
    }

    private PeriodOverviewResponse RunReport(IReadOnlyList<string> paths, ReportPeriod period, string? authorFilter)
    {
        PeriodOverviewResponse response = null!;
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("cornflowerblue"))
            .Start("Scanning repositories...", _ =>
            {
                response = _handler.Handle(new GetPeriodOverviewQuery(paths, period, authorFilter));
            });
        return response;
    }

    private static Task<bool> PromptContinueAsync() =>
        AnsiConsole.PromptAsync(
            new TextPrompt<string>("Generate another report? [green]y[/]/[red]n[/]:")
                .PromptStyle("cyan")
                .ValidationErrorMessage("[red]Please answer y or n[/]")
                .Validate(input =>
                {
                    var value = (input ?? string.Empty).Trim().ToLowerInvariant();
                    return value is "y" or "n"
                        ? ValidationResult.Success()
                        : ValidationResult.Error();
                }))
            .ContinueWith(t => string.Equals(t.Result.Trim(), "y", StringComparison.OrdinalIgnoreCase));
}
