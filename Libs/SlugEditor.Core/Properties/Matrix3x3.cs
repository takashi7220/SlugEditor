using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SlugEditor.Core.Properties;

public class Matrix3x3 : INotifyPropertyChanged
{
    private float _m11 = 1f;
    private float _m22 = 1f;
    private float _m33 = 1f;
    private float _m12;
    private float _m13;
    private float _m21;
    private float _m23;
    private float _m31;
    private float _m32;

    public event PropertyChangedEventHandler? PropertyChanged;

    public float M11 { get => _m11; set => SetField(ref _m11, value); }
    public float M12 { get => _m12; set => SetField(ref _m12, value); }
    public float M13 { get => _m13; set => SetField(ref _m13, value); }
    public float M21 { get => _m21; set => SetField(ref _m21, value); }
    public float M22 { get => _m22; set => SetField(ref _m22, value); }
    public float M23 { get => _m23; set => SetField(ref _m23, value); }
    public float M31 { get => _m31; set => SetField(ref _m31, value); }
    public float M32 { get => _m32; set => SetField(ref _m32, value); }
    public float M33 { get => _m33; set => SetField(ref _m33, value); }

    public void Set(
        float m11, float m12, float m13,
        float m21, float m22, float m23,
        float m31, float m32, float m33)
    {
        var changed = false;
        changed |= SetField(ref _m11, m11, nameof(M11));
        changed |= SetField(ref _m12, m12, nameof(M12));
        changed |= SetField(ref _m13, m13, nameof(M13));
        changed |= SetField(ref _m21, m21, nameof(M21));
        changed |= SetField(ref _m22, m22, nameof(M22));
        changed |= SetField(ref _m23, m23, nameof(M23));
        changed |= SetField(ref _m31, m31, nameof(M31));
        changed |= SetField(ref _m32, m32, nameof(M32));
        changed |= SetField(ref _m33, m33, nameof(M33));
        if (changed)
        {
            OnPropertyChanged(nameof(Elements));
        }
    }

    public float[] Elements => [_m11, _m12, _m13, _m21, _m22, _m23, _m31, _m32, _m33];

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
