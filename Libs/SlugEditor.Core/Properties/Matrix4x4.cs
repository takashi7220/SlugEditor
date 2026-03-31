using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SlugEditor.Core.Properties;

public class Matrix4x4 : INotifyPropertyChanged
{
    private System.Numerics.Matrix4x4 _value = System.Numerics.Matrix4x4.Identity;

    public Matrix4x4()
    {
    }

    public Matrix4x4(System.Numerics.Matrix4x4 value)
    {
        _value = value;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public float M11 { get => _value.M11; set => SetElement(ref _value.M11, value, nameof(M11)); }
    public float M12 { get => _value.M12; set => SetElement(ref _value.M12, value, nameof(M12)); }
    public float M13 { get => _value.M13; set => SetElement(ref _value.M13, value, nameof(M13)); }
    public float M14 { get => _value.M14; set => SetElement(ref _value.M14, value, nameof(M14)); }
    public float M21 { get => _value.M21; set => SetElement(ref _value.M21, value, nameof(M21)); }
    public float M22 { get => _value.M22; set => SetElement(ref _value.M22, value, nameof(M22)); }
    public float M23 { get => _value.M23; set => SetElement(ref _value.M23, value, nameof(M23)); }
    public float M24 { get => _value.M24; set => SetElement(ref _value.M24, value, nameof(M24)); }
    public float M31 { get => _value.M31; set => SetElement(ref _value.M31, value, nameof(M31)); }
    public float M32 { get => _value.M32; set => SetElement(ref _value.M32, value, nameof(M32)); }
    public float M33 { get => _value.M33; set => SetElement(ref _value.M33, value, nameof(M33)); }
    public float M34 { get => _value.M34; set => SetElement(ref _value.M34, value, nameof(M34)); }
    public float M41 { get => _value.M41; set => SetElement(ref _value.M41, value, nameof(M41)); }
    public float M42 { get => _value.M42; set => SetElement(ref _value.M42, value, nameof(M42)); }
    public float M43 { get => _value.M43; set => SetElement(ref _value.M43, value, nameof(M43)); }
    public float M44 { get => _value.M44; set => SetElement(ref _value.M44, value, nameof(M44)); }

    public System.Numerics.Matrix4x4 Value
    {
        get => _value;
        set => SetField(ref _value, value);
    }

    public static implicit operator System.Numerics.Matrix4x4(Matrix4x4 value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Value;
    }

    public static explicit operator Matrix4x4(System.Numerics.Matrix4x4 value)
    {
        return new Matrix4x4(value);
    }

    private void SetElement(ref float field, float value, string propertyName)
    {
        if (field.Equals(value))
        {
            return;
        }

        field = value;
        OnPropertyChanged(propertyName);
        OnPropertyChanged(nameof(Value));
    }

    private bool SetField(ref System.Numerics.Matrix4x4 field, System.Numerics.Matrix4x4 value, [CallerMemberName] string? propertyName = null)
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
