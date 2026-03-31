using Xunit;

namespace SlugEditor.Test;

public static class AssertEx
{
    public static void SinglePropertyChanged(
        PropertyChangedTracker tracker,
        string propertyName,
        string? because = null)
    {
        ArgumentNullException.ThrowIfNull(tracker);
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        var changes = tracker.Changes.Where(x => string.Equals(x, propertyName, StringComparison.Ordinal)).ToArray();
        Assert.True(
            changes.Length == 1,
            because ?? $"Expected exactly one PropertyChanged for '{propertyName}', but got {changes.Length}.");
    }

    public static void NoPropertyChanged(PropertyChangedTracker tracker, string? because = null)
    {
        ArgumentNullException.ThrowIfNull(tracker);
        Assert.True(
            tracker.Changes.Count == 0,
            because ?? $"Expected no PropertyChanged events, but got {tracker.Changes.Count}.");
    }

    public static void SequenceEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual)
    {
        ArgumentNullException.ThrowIfNull(expected);
        ArgumentNullException.ThrowIfNull(actual);
        Assert.Equal(expected.ToArray(), actual.ToArray());
    }
}
