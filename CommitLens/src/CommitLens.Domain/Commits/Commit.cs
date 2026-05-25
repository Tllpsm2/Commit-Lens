namespace CommitLens.Domain.Commits;

public record Commit
{
    public CommitHash Hash { get; init; }
    public Author Author { get; init; }
    public DateTimeOffset Date { get; init; }
    public string Subject { get; init; }

    public Commit(CommitHash hash, Author author, DateTimeOffset date, string subject)
    {
        Hash = hash;
        Author = author;
        Date = date;
        
        Subject = string.IsNullOrWhiteSpace(subject) ? "<no message>" : subject.Trim();
    }
}
