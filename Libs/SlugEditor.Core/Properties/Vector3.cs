using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SlugEditor.Core.Properties;

public class Vector3 : INotifyPropertyChanged
{
    private float _x;
    private float _y;
    private float _z;

    public Vector3()
    {
    }

    public Vector3(float x, float y, float z)
    {
        _x = x;
        _y = y;
        _z = z;
    }

    public Vector3(System.Numerics.Vector3 value) : this(value.X, value.Y, value.Z)
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

    public System.Numerics.Vector3 Value
    {
        get => new(_x, _y, _z);
        set
        {
            var changed = false;
            changed |= SetField(ref _x, value.X, nameof(X));
            changed |= SetField(ref _y, value.Y, nameof(Y));
            changed |= SetField(ref _z, value.Z, nameof(Z));
            if (changed)
            {
                OnPropertyChanged(nameof(Value));
            }
        }
    }

    public static implicit operator System.Numerics.Vector3(Vector3 value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Value;
    }

    public static explicit operator Vector3(System.Numerics.Vector3 value)
    {
        return new Vector3(value);
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
