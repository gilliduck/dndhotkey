namespace DndHotkey.Core.Tests;

public sealed class SemanticVersionCalculatorTests
{
    [Fact]
    public void NextVersion_ReturnsNullForNonReleaseCommits()
    {
        var next = SemanticVersionCalculator.NextVersion(
            SemanticVersion.Parse("1.2.3"),
            [new CommitMessage("docs: update readme")]);

        Assert.Null(next);
    }

    [Fact]
    public void NextVersion_UsesMajorForBreakingCommit()
    {
        var next = SemanticVersionCalculator.NextVersion(
            SemanticVersion.Parse("1.2.3"),
            [new CommitMessage("feat!: change config shape")]);

        Assert.Equal("2.0.0", next?.ToString());
    }

    [Fact]
    public void NextVersion_UsesMinorForFeatureCommit()
    {
        var next = SemanticVersionCalculator.NextVersion(
            SemanticVersion.Parse("1.2.3"),
            [new CommitMessage("feat: add tray menu")]);

        Assert.Equal("1.3.0", next?.ToString());
    }
}
