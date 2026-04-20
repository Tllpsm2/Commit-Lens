# CommitLens

CommitLens is a C# CLI for quick visibility into recent Git activity.
It scans a repository, shows a commit table in the terminal, and summarizes activity by day and author.

## Current Status

Early stage MVP.

What already works:
- Automatically detects the Git repository root from the current directory.
- Reads commit history from the last X days.
- Renders recent commits in a terminal table.
- Shows summary metrics (total commits, active days, authors).

## Tech Stack

- .NET 10
- Spectre.Console
- xUnit

## Quick Start

Requirements:
- .NET 10 SDK installed
- Git installed and available in `PATH`

Run from repository root:

```bash
dotnet run --project CommitLens/CommitLens.csproj
```

Run tests:

```bash
dotnet test CommitLens.Tests/CommitLens.Tests.csproj
```

## Project Structure

```text
CommitLens/
	Infrastructure/   # Git command execution, parsing, repository discovery
	Interfaces/       # Contracts between layers
	Models/           # Domain models and report records
	Services/         # Application orchestration and report generation
	UI/               # Console rendering (Spectre.Console)
```

## Roadmap

Primary objective: make CommitLens globally installable as a versioned .NET tool via NuGet.

- [ ] Package CommitLens as a .NET Global Tool (`PackAsTool`, `ToolCommandName`, `PackageId`, and package metadata).
- [ ] Define semantic versioning and release tags.
- [ ] Add CD to pack and publish to NuGet.org on version tags.
- [ ] Publish the first stable release and document global install (`dotnet tool install -g CommitLens.Tool`).
- [ ] Evolve product features after distribution baseline (CLI args, export formats, streak/heatmap analytics).