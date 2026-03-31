using System.ComponentModel;

namespace SlugEditor.Test;

public sealed class PropertyChangedTracker : IDisposable
{
    private readonly INotifyPropertyChanged _source;
    private readonly List<string> _changes = [];

    public PropertyChangedTracker(INotifyPropertyChanged source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _source.PropertyChanged += OnPropertyChanged;
    }

    public IReadOnlyList<string> Changes => _changes;

    public int Count(string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);
        return _changes.Count(x => string.Equals(x, propertyName, StringComparison.Ordinal));
    }

    public bool Contains(string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);
        return _changes.Contains(propertyName, StringComparer.Ordinal);
    }

    public void Clear()
    {
        _changes.Clear();
    }

    public void Dispose()
    {
        _source.PropertyChanged -= OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.PropertyName))
        {
            return;
        }

        _changes.Add(e.PropertyName);
    }
}
