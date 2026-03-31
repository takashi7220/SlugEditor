using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SlugEditor.Core.Properties;

public class Vector2 : INotifyPropertyChanged
{
    private float _x;
    private float _y;

    public Vector2()
    {
    }

    public Vector2(float x, float y)
    {
        _x = x;
        _y = y;
    }

    public Vector2(System.Numerics.Vector2 value) : this(value.X, value.Y)
    {
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public float X
    {
        get => _x;
        set => SetField(ref _x, value);
    }

    public float Y
    {
        get => _y;
        set => SetField(ref _y, value);
    }

    public System.Numerics.Vector2 Value
    {
        get => new(_x, _y);
        set
        {
            var changed = false;
            changed |= SetField(ref _x, value.X, nameof(X));
            changed |= SetField(ref _y, value.Y, nameof(Y));
            if (changed)
            {
                OnPropertyChanged(nameof(Value));
            }
        }
    }

    public static implicit operator System.Numerics.Vector2(Vector2 value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Value;
    }

    public static explicit operator Vector2(System.Numerics.Vector2 value)
    {
        return new Vector2(value);
    }

    private bool SetField(ref float field, float value, [CallerMemberName] string? propertyName = null)
    {
        if (field.Equals(value))
        {
            return false;
        }

        field = value;
        if (propertyName is not null)
        {
            OnPropertyChanged(propertyName);
        }
        return true;
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
