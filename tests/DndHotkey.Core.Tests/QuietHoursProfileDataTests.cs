namespace DndHotkey.Core.Tests;

public sealed class QuietHoursProfileDataTests
{
    [Fact]
    public void Build_CreatesDetectableEnabledProfile()
    {
        var data = QuietHoursProfileData.Build(DndState.Enabled, DateTimeOffset.UnixEpoch);

        Assert.Equal(DndState.Enabled, QuietHoursProfileData.DetectState(data));
    }

    [Fact]
    public void Build_CreatesDetectableDisabledProfile()
    {
        var data = QuietHoursProfileData.Build(DndState.Disabled, DateTimeOffset.UnixEpoch);

        Assert.Equal(DndState.Disabled, QuietHoursProfileData.DetectState(data));
    }

    [Fact]
    public void SetState_ReplacesExistingProfileAndRefreshesTimestamp()
    {
        var original = QuietHoursProfileData.Build(DndState.Disabled, DateTimeOffset.UnixEpoch);
        var updatedAt = new DateTimeOffset(2026, 5, 12, 10, 30, 0, TimeSpan.Zero);

        var updated = QuietHoursProfileData.SetState(original, DndState.Enabled, updatedAt);

        Assert.Equal(DndState.Enabled, QuietHoursProfileData.DetectState(updated));
        Assert.Equal(updatedAt.ToFileTime(), BitConverter.ToInt64(updated, 4));
    }

    [Fact]
    public void SetState_BuildsProfileWhenInputIsUnknown()
    {
        var updated = QuietHoursProfileData.SetState([1, 2, 3], DndState.Disabled, DateTimeOffset.UnixEpoch);

        Assert.Equal(DndState.Disabled, QuietHoursProfileData.DetectState(updated));
    }
}
