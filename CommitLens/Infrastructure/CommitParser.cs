using CommitLens.Interfaces;
using CommitLens.Models;

namespace CommitLens.Infrastructure;

public class CommitParser : ICommitParser
{
    public List<CommitEntry> ParseOutput(string rawOutput)
    {
        if (string.IsNullOrWhiteSpace(rawOutput))
            return [];

        // Expected format per line (5parts): Hash|AuthorName|AuthorEmail|Date|Message
        var commits = new List<CommitEntry>();

        // Split output into lines and parse each line
        foreach (var line in rawOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = line.Split('|', 5);
            if (parts.Length < 5)
                continue;

            if (!DateTimeOffset.TryParse(parts[3], out var date))
                continue;
            // map parts to CommitEntry
            commits.Add(new CommitEntry(
                Hash: parts[0][..7],
                AuthorName: parts[1],
                AuthorEmail: parts[2],
                Date: date,
                Message: parts[4]
            ));
        }

        return commits;
    }
}
