using System.Text.RegularExpressions;

namespace DndHotkey.Core;

public sealed record CommitMessage(string Subject, string Body = "");

public static partial class SemanticVersionCalculator
{
    public static SemanticVersion? NextVersion(SemanticVersion current, IEnumerable<CommitMessage> commits)
    {
        var bump = ReleaseBump(commits);
        return bump switch
        {
            ReleaseBumpKind.Major => new SemanticVersion(current.Major + 1, 0, 0),
            ReleaseBumpKind.Minor => new SemanticVersion(current.Major, current.Minor + 1, 0),
            ReleaseBumpKind.Patch => new SemanticVersion(current.Major, current.Minor, current.Patch + 1),
            _ => null
        };
    }

    private static ReleaseBumpKind ReleaseBump(IEnumerable<CommitMessage> commits)
    {
        var bump = ReleaseBumpKind.None;
        foreach (var commit in commits.Where(IsReleaseEligible))
        {
            if (IsBreakingChange(commit))
            {
                return ReleaseBumpKind.Major;
            }

            if (FeatureSubject().IsMatch(commit.Subject))
            {
                bump = Max(bump, ReleaseBumpKind.Minor);
            }
            else if (PatchSubject().IsMatch(commit.Subject))
            {
                bump = Max(bump, ReleaseBumpKind.Patch);
            }
        }

        return bump;
    }

    private static bool IsReleaseEligible(CommitMessage commit) =>
        !SkipRelease().IsMatch(commit.Subject) &&
        !ReleaseChore().IsMatch(commit.Subject);

    private static bool IsBreakingChange(CommitMessage commit) =>
        BreakingSubject().IsMatch(commit.Subject) ||
        BreakingBody().IsMatch(commit.Body);

    private static ReleaseBumpKind Max(ReleaseBumpKind left, ReleaseBumpKind right) =>
        left > right ? left : right;

    [GeneratedRegex(@"\[skip-release\]|\[skip release\]", RegexOptions.IgnoreCase)]
    private static partial Regex SkipRelease();

    [GeneratedRegex(@"^chore\(release\):", RegexOptions.IgnoreCase)]
    private static partial Regex ReleaseChore();

    [GeneratedRegex(@"^[a-zA-Z]+(\([^)]+\))?!:")]
    private static partial Regex BreakingSubject();

    [GeneratedRegex(@"^BREAKING CHANGE:", RegexOptions.Multiline)]
    private static partial Regex BreakingBody();

    [GeneratedRegex(@"^feat(\([^)]+\))?:")]
    private static partial Regex FeatureSubject();

    [GeneratedRegex(@"^(fix|perf)(\([^)]+\))?:")]
    private static partial Regex PatchSubject();

    private enum ReleaseBumpKind
    {
        None = 0,
        Patch = 1,
        Minor = 2,
        Major = 3
    }
}
