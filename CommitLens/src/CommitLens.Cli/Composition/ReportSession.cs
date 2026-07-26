using CommitLens.Application.Abstractions;
using CommitLens.Application.Reports.GetActivityHeatMap;
using CommitLens.Application.Reports.GetPeriodOverview;
using CommitLens.Cli.Input;
using CommitLens.Cli.Rendering;
using Spectre.Console;

namespace CommitLens.Cli.Composition;

internal sealed class ReportSession
{
    private readonly RepositoryPathCollector _pathCollector;
    private readonly PeriodOverviewRenderer _periodRenderer;
    private readonly ActivityHeatMapRenderer _heatMapRenderer;
    private readonly GetPeriodOverviewQueryHandler _periodHandler;
    private readonly GetActivityHeatMapQueryHandler _heatMapHandler;

    public ReportSession(
        IRepositoryLocator locator,
        GetPeriodOverviewQueryHandler periodHandler,
        GetActivityHeatMapQueryHandler heatMapHandler)
    {
        _pathCollector = new RepositoryPathCollector(locator);
        _periodRenderer = new PeriodOverviewRenderer();
        _heatMapRenderer = new ActivityHeatMapRenderer();
        _periodHandler = periodHandler;
        _heatMapHandler = heatMapHandler;
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

            var reportType = await PromptReportTypeAsync();

            switch (reportType)
            {
                case ReportType.PeriodOverview:
                {
                    var period = await PromptPeriodAsync();
                    var authorFilter = await PromptAuthorFilterAsync();
                    var response = RunPeriodReport(paths, period, authorFilter);
                    _periodRenderer.Render(response);
                    break;
                }
                case ReportType.ActivityHeatMap:
                {
                    var heatMapPeriod = await PromptHeatMapPeriodAsync();
                    var authorFilter = await PromptAuthorFilterAsync();
                    var response = RunHeatMapReport(paths, heatMapPeriod, authorFilter);
                    _heatMapRenderer.Render(response);
                    break;
                }
            }

            if (!await PromptContinueAsync())
                return;
        }
    }

    private static void RenderBanner()
    {
        AnsiConsole.Write(new FigletText("CommitLens").Color(Color.CornflowerBlue));
        AnsiConsole.MarkupLine("[grey]Git repository commit analyser[/]\n");
    }

    private static Task<ReportType> PromptReportTypeAsync() =>
        AnsiConsole.PromptAsync(
            new SelectionPrompt<ReportType>()
                .Title("\nSelect [bold]report type[/]:")
                .AddChoices(ReportType.PeriodOverview, ReportType.ActivityHeatMap)
                .UseConverter(t => t switch
                {
                    ReportType.PeriodOverview => "Period Overview",
                    ReportType.ActivityHeatMap => "Activity Heat Map",
                    _ => t.ToString()
                }));

    private static Task<ReportPeriod> PromptPeriodAsync() =>
        AnsiConsole.PromptAsync(
            new SelectionPrompt<ReportPeriod>()
                .Title("\nSelect [bold]report period[/]:")
                .AddChoices(ReportPeriod.Daily, ReportPeriod.Weekly, ReportPeriod.Monthly, ReportPeriod.Yearly));

    private static Task<HeatMapPeriod> PromptHeatMapPeriodAsync() =>
        AnsiConsole.PromptAsync(
            new SelectionPrompt<HeatMapPeriod>()
                .Title("\nSelect [bold]heat map range[/]:")
                .AddChoices(HeatMapPeriod.Weekly, HeatMapPeriod.Monthly, HeatMapPeriod.Yearly, HeatMapPeriod.AllTime)
                .UseConverter(p => p switch
                {
                    HeatMapPeriod.Weekly => "Weekly (last 7 days)",
                    HeatMapPeriod.Monthly => "Monthly (last 30 days)",
                    HeatMapPeriod.Yearly => "Yearly (last 12 months)",
                    HeatMapPeriod.AllTime => "All time (since first commit)",
                    _ => p.ToString()
                }));

    private static async Task<string?> PromptAuthorFilterAsync()
    {
        var input = (await AnsiConsole.PromptAsync(
            new TextPrompt<string>("\nFilter by author name? [grey](leave empty to skip)[/]:")
                .AllowEmpty()))?.Trim() ?? string.Empty;
        return string.IsNullOrWhiteSpace(input) ? null : input;
    }

    private PeriodOverviewResponse RunPeriodReport(IReadOnlyList<string> paths, ReportPeriod period, string? authorFilter)
    {
        PeriodOverviewResponse response = null!;
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("cornflowerblue"))
            .Start("Scanning repositories...", _ =>
            {
                response = _periodHandler.Handle(new GetPeriodOverviewQuery(paths, period, authorFilter));
            });
        return response;
    }

    private ActivityHeatMapResponse RunHeatMapReport(IReadOnlyList<string> paths, HeatMapPeriod period, string? authorFilter)
    {
        ActivityHeatMapResponse response = null!;
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("cornflowerblue"))
            .Start("Scanning repositories...", _ =>
            {
                response = _heatMapHandler.Handle(new GetActivityHeatMapQuery(paths, period, authorFilter));
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