using CommitLens.Domain.Commits;

namespace CommitLens.Infrastructure.Git;

internal static class CommitParser
{
    // Log format: Fields are delimited by U+001F (ASCII Unit Separator) to prevent accidental collision with special characters in commit messages.
    internal const string LogFormat = "%H%x1f%an%x1f%ae%x1f%aI%x1f%s";
    private const char Separator = '\x1f';

    internal static Commit? Parse(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return null;

        var parts = line.Split(Separator);

        if (parts.Length < 5)
            return null;

        var hash = parts[0].Trim();
        var name = parts[1].Trim();
        var email = parts[2].Trim();
        var date = parts[3].Trim();
        var subject = parts[4].Trim();

        if (!DateTimeOffset.TryParse(date, out var parsedDate))
            parsedDate = DateTimeOffset.MinValue;

        try
        {
            return new Commit(
                new CommitHash(hash),
                new Author(name, email),
                parsedDate,
                subject);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
}
