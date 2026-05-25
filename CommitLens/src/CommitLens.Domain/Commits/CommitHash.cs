using System.Text.RegularExpressions;

namespace CommitLens.Domain.Commits;

public partial record CommitHash
{
    public string Value { get; init; }

    public CommitHash(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Commit hash cannot be empty.", nameof(value));

        if (!HashRegex().IsMatch(value))
            throw new ArgumentException("Invalid hash format. Expected 7 to 64 hexadecimal characters.", nameof(value));

        Value = value;
    }

    public string Abbreviated => Value[..7];

    public override string ToString() => Value;

    [GeneratedRegex(@"^[a-fA-F0-9]{7,64}$")]
    private static partial Regex HashRegex();
}
