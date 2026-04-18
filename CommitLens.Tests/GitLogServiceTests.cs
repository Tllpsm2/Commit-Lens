using CommitLens.Models;
using CommitLens.Infrastructure;
using CommitLens.Services;
using Xunit;

namespace CommitLens.Tests;

public class GitLogServiceTests
{
    private static readonly CommitParser Parser = new();

    // Representative log line used for basic validation
    private const string ValidLine = "abcdef1234567|Alice|alice@example.com|2024-06-15T09:30:00Z|Fix null ref";

    public class ParseOutput_InvalidInput
    {
        // Tests edge cases where the entire input stream is null or empty
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\n\t")]
        public void WhenNullOrWhitespace_ReturnsEmptyList(string? input)
        {
            // Act
            var result = Parser.ParseOutput(input!);

            // Assert
            Assert.Empty(result);
        }

        // Validates that lines failing the minimum field requirement are safely ignored
        [Theory]
        [InlineData("abcdef1|Joao|j@e.com|2024-01-01T12:00:00Z")] // 4 parts: missing message
        [InlineData("abcdef1|Joao|j@e.com")]                       // 3 parts
        [InlineData("abcdef1")]                                     // 1 part
        [InlineData("invalid_text_without_delimiter")]              // No delimiters
        public void WhenFewerThanFiveParts_LineIsSkipped(string input)
        {
            // Act
            var result = Parser.ParseOutput(input);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void WhenDateIsUnparseable_LineIsSkipped()
        {
            // Arrange
            const string input = "abcdef1|Joao|j@e.com|NOT-A-DATE|mensagem";

            // Act
            var result = Parser.ParseOutput(input);

            // Assert
            Assert.Empty(result);
        }
    }

    public class ParseOutput_ValidInput
    {
        // Tests structural boundary cases for valid git log formats
        [Theory]
        [InlineData("12345678|Joao|j@e.com|2024-01-01T12:00:00Z|mensagem")]
        [InlineData("1234567|Joao|j@e.com|2024-01-01T12:00:00Z|mensagem")]        // 7-char hash boundary
        [InlineData("12345678|Joao|j@e.com|2024-01-01T12:00:00Z|")]               // Trailing delimiter with empty message
        [InlineData("12345678|Joao|j@e.com|2024-01-01T12:00:00Z|msg | with | pipes")]
        public void WhenStructureIsValid_ReturnsSingleCommit(string input)
        {
            // Act
            var result = Parser.ParseOutput(input);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public void WhenSingleValidLine_MapsAllFieldsCorrectly()
        {
            // Arrange
            const string input = "abcdef1234567|Alice|alice@example.com|2024-06-15T09:30:00Z|Fix null ref";

            // Act
            var result = Parser.ParseOutput(input);

            // Assert
            CommitEntry commit = Assert.Single(result);
            Assert.Equal("abcdef1", commit.Hash); // Verify short-hash truncation (7 chars)
            Assert.Equal("Alice", commit.AuthorName);
            Assert.Equal("alice@example.com", commit.AuthorEmail);
            Assert.Equal("Fix null ref", commit.Message);
            Assert.Equal(2024, commit.Date.Year);
            Assert.Equal(6, commit.Date.Month);
            Assert.Equal(15, commit.Date.Day);
        }

        // Verifies that the parser correctly handles delimiters within the message field
        [Fact]
        public void WhenMessageContainsPipes_CapturesFullMessageText()
        {
            // Arrange
            const string input = "12345678|Joao|j@e.com|2024-01-01T12:00:00Z|msg | with | pipes";

            // Act
            var result = Parser.ParseOutput(input);

            // Assert
            CommitEntry commit = Assert.Single(result);
            Assert.Equal("msg | with | pipes", commit.Message);
        }

        [Fact]
        public void WhenMultipleValidLines_ReturnsAllCommitsInOrder()
        {
            // Arrange
            const string input =
                "aaa0001|Alice|a@a.com|2024-01-01T00:00:00Z|Commit A\n" +
                "bbb0002|Bob|b@b.com|2024-01-02T00:00:00Z|Commit B\n" +
                "ccc0003|Carol|c@c.com|2024-01-03T00:00:00Z|Commit C";

            // Act
            var result = Parser.ParseOutput(input);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("aaa0001", result[0].Hash);
            Assert.Equal("bbb0002", result[1].Hash);
            Assert.Equal("ccc0003", result[2].Hash);
        }

        // Validates the parser's robustness when encountering malformed data mid-stream
        [Fact]
        public void WhenMixedValidAndInvalidLines_ReturnsOnlyValidCommits()
        {
            // Arrange
            const string input =
                "aaa0001|Alice|a@a.com|2024-01-01T00:00:00Z|Valid commit\n" +
                "invalid_line_no_pipes\n" +
                "bbb0002|Bob|b@b.com|NOT-A-DATE|Wrong date format\n" +
                "ccc0003|Carol|c@c.com|2024-01-03T00:00:00Z|Another valid commit";

            // Act
            var result = Parser.ParseOutput(input);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("aaa0001", result[0].Hash);
            Assert.Equal("ccc0003", result[1].Hash);
        }
    }
}