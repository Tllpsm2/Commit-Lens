# CommitLens (Main Project)

A C# command-line application designed to analyze and display the commit history of a Git repository.

## Estrutura

```
CommitLens/
├── Commands/          # CLI command implementations
├── Config/            # Application configurations and settings
├── Models/            # Data models (CommitEntry, DailyActivity, RepositoryInfo)
├── Reporters/         # Report generation logic
├── Services/          # Business logic and core services (GitLogService)
└── Program.cs         # Application entry point
```

## How to Run

```bash
dotnet run
```

## Dependencies

- .NET 10.0
