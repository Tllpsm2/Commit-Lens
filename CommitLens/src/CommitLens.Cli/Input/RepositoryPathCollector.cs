using CommitLens.Application.Abstractions;
using Spectre.Console;

namespace CommitLens.Cli.Input;

internal sealed class RepositoryPathCollector
{
    private readonly IRepositoryLocator _locator;

    public RepositoryPathCollector(IRepositoryLocator locator) => _locator = locator;

    public async Task<IReadOnlyList<string>> CollectAsync(IReadOnlyList<string> existing)
    {
        var paths = new List<string>(existing);

        if (paths.Count > 0)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Currently registered repositories:[/]");
            foreach (var p in paths)
                AnsiConsole.MarkupLine($"  [blue]\u2022[/] {Markup.Escape(p)}");
        }

        AnsiConsole.MarkupLine("\n[bold]Enter repository paths[/] (one per line, empty line to finish):");

        while (true)
        {
            var input = (await AnsiConsole.PromptAsync(
                new TextPrompt<string>("[blue]>[/]")
                    .AllowEmpty()))?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(input))
                break;

            if (!Directory.Exists(input))
            {
                AnsiConsole.MarkupLine($"[red]Directory not found:[/] {input}");
                continue;
            }

            IReadOnlyList<string> resolved;
            try
            {
                resolved = _locator.FindRepositories(input);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error searching for Git repositories:[/] {ex.Message}");
                continue;
            }

            if (resolved.Count == 0)
            {
                AnsiConsole.MarkupLine($"[red]No Git repositories found in or under:[/] {input}");
                continue;
            }

            foreach (var resolvedPath in resolved)
            {
                if (!paths.Contains(resolvedPath))
                {
                    paths.Add(resolvedPath);
                    AnsiConsole.MarkupLine($"[green]Added repository:[/] {resolvedPath}");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[yellow]Repository already added:[/] {resolvedPath}");
                }
            }
        }

        return paths;
    }
}
