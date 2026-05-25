# CommitLens

CommitLens is a Git repository commit analyser built in .NET 10. It parses Git logs and repository details to generate deep, clean reports on activities, bus factors, top contributors, and more.

---

## Current Status

- Working: period overview report (daily, weekly, monthly, yearly)
- Working: multi-repository scanning in a single run
- Working: automatic Git repository discovery inside a given directory
- Working: author filter
- Working: interactive terminal UI

> Currently only local paths are supported. Remote repository scanning (directly from a URL) is in progress.

---

## Stack

- .NET 10
- Spectre.Console
- xUnit + FluentAssertions

---

## Requirements

- .NET 10 SDK
- Git available on PATH

---

## Quick Start

```bash
git clone https://github.com/Tllpsm2/CommitLens.git
cd CommitLens
dotnet run --project CommitLens/src/CommitLens.Cli
```

---

## Clean Architecture

```
src/
├── CommitLens.Domain          # Entities and business rules
├── CommitLens.Application     # Use cases and query handlers
├── CommitLens.Infrastructure  # Git CLI and file system integrations
└── CommitLens.Cli             # Entry point and user interface

tests/
├── CommitLens.Domain.UnitTests
└── CommitLens.Application.UnitTests
```

---

## Roadmap

Primary objective: make CommitLens globally installable as a versioned .NET tool via NuGet.

### Pending reports

- [ ] **Top Contributors** — ranking of authors by commit count and participation percentage
- [ ] **Bus Factor** — knowledge concentration risk across contributors
- [ ] **Activity Heat Map** — commit frequency by day of week and time of day
- [ ] **Ghost Authors** — authors who contributed in the past but have gone inactive
- [ ] **Author Activity** — individual developer activity over time

### Other planned improvements

- [ ] Publish as a `dotnet tool` on NuGet
- [ ] Export reports to Markdown and JSON
- [ ] Configuration file support (`.commitlens.json`)
- [ ] Branch filtering
- [ ] Remote repository support (clone and analyse from URL)

---

## License

MIT © [João Vitor Oliveira Alves](https://github.com/Tllpsm2)
