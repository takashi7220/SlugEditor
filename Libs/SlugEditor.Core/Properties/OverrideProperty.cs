using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SlugEditor.Core.Properties;

public class OverrideProperty<T> : IOverrideProperty, INotifyPropertyChanged where T : notnull
{
    private static readonly EqualityComparer<T> Comparer = EqualityComparer<T>.Default;
    private T _value;
    private T _default;

    public OverrideProperty(T value)
    {
        _value = value;
        _default = value;
    }

    public OverrideProperty(T value, T @default)
    {
        _value = value;
        _default = @default;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Reset()
    {
        Value = Default;
    }

    public bool ResetIfOverridden()
    {
        if (!IsOverride())
        {
            return false;
        }

        Reset();
        return true;
    }

    public bool IsOverride()
    {
        return !Comparer.Equals(Value, Default);
    }

    public bool IsDefault()
    {
        return !IsOverride();
    }

    public bool SetValue(T value)
    {
        return SetField(ref _value, value, nameof(Value));
    }

    public bool SetDefault(T @default, bool resetValue = false)
    {
        var changed = SetField(ref _default, @default, nameof(Default));

        if (resetValue)
        {
            changed |= SetValue(@default);
        }

        return changed;
    }

    public bool SetBoth(T value, T @default)
    {
        var changed = false;
        changed |= SetDefault(@default);
        changed |= SetValue(value);
        return changed;
    }

    public T Value
    {
        get => _value;
        set => SetValue(value);
    }

    public T Default
    {
        get => _default;
        set => SetDefault(value);
    }

    private bool SetField(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        var beforeOverride = IsOverride();
        if (Comparer.Equals(field, value))
        {
            return false;
        }

        field = value;
        if (propertyName is not null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        var afterOverride = IsOverride();
        if (beforeOverride != afterOverride)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsOverride)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDefault)));
        }

        return true;
    }
}
