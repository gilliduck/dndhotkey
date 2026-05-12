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
            _                     => null
        };
    }

    [GeneratedRegex(@"^BREAKING CHANGE:", RegexOptions.Multiline)]
    private static partial Regex BreakingBody();

    [GeneratedRegex(@"^[a-zA-Z]+(\([^)]+\))?!:")]
    private static partial Regex BreakingSubject();

    [GeneratedRegex(@"^feat(\([^)]+\))?:")]
    private static partial Regex FeatureSubject();

    private static bool IsBreakingChange(CommitMessage commit)
    {
        return BreakingSubject().IsMatch(commit.Subject) ||
               BreakingBody().IsMatch(commit.Body);
    }

    private static bool IsReleaseEligible(CommitMessage commit)
    {
        return !SkipRelease().IsMatch(commit.Subject) &&
               !ReleaseChore().IsMatch(commit.Subject);
    }

    private static ReleaseBumpKind Max(ReleaseBumpKind left, ReleaseBumpKind right)
    {
        return left > right ? left : right;
    }

    [GeneratedRegex(@"^(fix|perf)(\([^)]+\))?:")]
    private static partial Regex PatchSubject();

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

    [GeneratedRegex(@"^chore\(release\):", RegexOptions.IgnoreCase)]
    private static partial Regex ReleaseChore();

    [GeneratedRegex(@"\[skip-release\]|\[skip release\]", RegexOptions.IgnoreCase)]
    private static partial Regex SkipRelease();

    private enum ReleaseBumpKind
    {
        None = 0,
        Patch = 1,
        Minor = 2,
        Major = 3
    }
}
