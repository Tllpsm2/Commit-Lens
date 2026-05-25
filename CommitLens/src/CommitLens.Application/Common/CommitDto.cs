namespace CommitLens.Application.Common;

public record CommitDto(
    string Hash,
    string AuthorName,
    string AuthorEmail,
    DateTimeOffset Date,
    string RelativeDate,
    string Subject,
    string RepositoryName
    );
