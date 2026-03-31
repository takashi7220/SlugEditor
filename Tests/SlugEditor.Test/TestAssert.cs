namespace SlugEditor.Test;

public static class TestAssert
{
    public static void True(bool condition, string? message = null)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message ?? "Expected condition to be true.");
        }
    }

    public static void False(bool condition, string? message = null)
    {
        if (condition)
        {
            throw new InvalidOperationException(message ?? "Expected condition to be false.");
        }
    }

    public static void Equal<T>(T expected, T actual, string? message = null)
    {
        if (expected is System.Collections.IEnumerable expectedEnumerable &&
            actual is System.Collections.IEnumerable actualEnumerable &&
            expected is not string &&
            actual is not string)
        {
            var expectedItems = expectedEnumerable.Cast<object?>().ToArray();
            var actualItems = actualEnumerable.Cast<object?>().ToArray();
            if (!expectedItems.SequenceEqual(actualItems))
            {
                throw new InvalidOperationException(
                    message ?? $"Expected sequence: [{string.Join(", ", expectedItems)}], Actual: [{string.Join(", ", actualItems)}]");
            }

            return;
        }

        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException(
                message ?? $"Expected: {expected}, Actual: {actual}");
        }
    }

    public static void Same(object expected, object actual, string? message = null)
    {
        if (!ReferenceEquals(expected, actual))
        {
            throw new InvalidOperationException(message ?? "Expected the same instance.");
        }
    }

    public static void NotNull(object? value, string? message = null)
    {
        if (value is null)
        {
            throw new InvalidOperationException(message ?? "Expected non-null value.");
        }
    }

    public static void Empty<T>(IEnumerable<T> values, string? message = null)
    {
        if (values.Any())
        {
            throw new InvalidOperationException(message ?? "Expected sequence to be empty.");
        }
    }

    public static void Contains<T>(T expected, IEnumerable<T> values, string? message = null)
    {
        if (!values.Contains(expected))
        {
            throw new InvalidOperationException(message ?? $"Expected sequence to contain: {expected}");
        }
    }

    public static T Single<T>(IEnumerable<T> values, string? message = null)
    {
        var array = values.ToArray();
        if (array.Length != 1)
        {
            throw new InvalidOperationException(message ?? $"Expected one item but was {array.Length}.");
        }

        return array[0];
    }
}
