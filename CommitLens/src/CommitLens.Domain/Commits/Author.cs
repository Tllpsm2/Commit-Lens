namespace CommitLens.Domain.Commits;

public record Author
{
    public string Name { get; init; }
    public string Email { get; init; }

    public Author(string name, string email)
    {
        Name = string.IsNullOrWhiteSpace(name) ? "Unknown" : name.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? string.Empty : email.Trim();
    }
}
