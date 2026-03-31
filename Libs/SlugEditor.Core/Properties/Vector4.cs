using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SlugEditor.Core.Properties;

public class Vector4 : INotifyPropertyChanged
{
    private float _x;
    private float _y;
    private float _z;
    private float _w;

    public Vector4()
    {
    }

    public Vector4(float x, float y, float z, float w)
    {
        _x = x;
        _y = y;
        _z = z;
        _w = w;
    }

    public Vector4(System.Numerics.Vector4 value) : this(value.X, value.Y, value.Z, value.W)
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

    public float Z
    {
        get => _z;
        set => SetField(ref _z, value);
    }

    public float W
    {
        get => _w;
        set => SetField(ref _w, value);
    }

    public System.Numerics.Vector4 Value
    {
        get => new(_x, _y, _z, _w);
        set
        {
            var changed = false;
            changed |= SetField(ref _x, value.X, nameof(X));
            changed |= SetField(ref _y, value.Y, nameof(Y));
            changed |= SetField(ref _z, value.Z, nameof(Z));
            changed |= SetField(ref _w, value.W, nameof(W));
            if (changed)
            {
                OnPropertyChanged(nameof(Value));
            }
        }
    }

    public static implicit operator System.Numerics.Vector4(Vector4 value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Value;
    }

    public static explicit operator Vector4(System.Numerics.Vector4 value)
    {
        return new Vector4(value);
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
