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
        TestAssert.True(
            changes.Length == 1,
            because ?? $"Expected exactly one PropertyChanged for '{propertyName}', but got {changes.Length}.");
    }

    public static void NoPropertyChanged(PropertyChangedTracker tracker, string? because = null)
    {
        ArgumentNullException.ThrowIfNull(tracker);
        TestAssert.True(
            tracker.Changes.Count == 0,
            because ?? $"Expected no PropertyChanged events, but got {tracker.Changes.Count}.");
    }

    public static void SequenceEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual)
    {
        ArgumentNullException.ThrowIfNull(expected);
        ArgumentNullException.ThrowIfNull(actual);
        TestAssert.Equal(expected.ToArray(), actual.ToArray());
    }
}
