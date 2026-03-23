namespace CommitLens.Models;

public record DailyActivity(
    DateOnly Date,
    int CommitCount,
    List<CommitEntry> Commits
);