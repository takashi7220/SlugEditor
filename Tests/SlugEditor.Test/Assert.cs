namespace SlugEditor.Test;

public static class Assert
{
    public static void True(bool condition, string? message = null) => TestAssert.True(condition, message);
    public static void False(bool condition, string? message = null) => TestAssert.False(condition, message);
    public static void Equal<T>(T expected, T actual, string? message = null) => TestAssert.Equal(expected, actual, message);
    public static void Same(object expected, object actual, string? message = null) => TestAssert.Same(expected, actual, message);
    public static void NotNull(object? value, string? message = null) => TestAssert.NotNull(value, message);
    public static void Empty<T>(IEnumerable<T> values, string? message = null) => TestAssert.Empty(values, message);
    public static void Contains<T>(T expected, IEnumerable<T> values, string? message = null) => TestAssert.Contains(expected, values, message);
    public static T Single<T>(IEnumerable<T> values, string? message = null) => TestAssert.Single(values, message);
}
