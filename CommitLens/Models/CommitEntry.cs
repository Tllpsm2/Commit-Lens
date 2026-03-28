namespace CommitLens.Models;

public record CommitEntry(
    string Hash,
    string AuthorName,
    string AuthorEmail,
    DateTimeOffset Date,
    string Message
);