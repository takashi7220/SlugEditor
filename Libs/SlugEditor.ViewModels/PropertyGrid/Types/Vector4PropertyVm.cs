using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SlugEditor.ViewModels.PropertyGrid.Types
{
    public class Vector4PropertyVm : VectorPropertyVm<Vector4>
    {
        public Vector4PropertyVm(Vector4 value)
            : base(value)
        {
        }

        public float X
        {
            get => Value.X;
            set
            {
                m_value = new Vector4 { X = value, Y = Value.Y, Z = Value.Z, W = Value.W };
                OnPropertyChanged();
            }
        }

        public float Y
        {
            get => Value.Y;
            set
            {
                m_value = new Vector4 { X = Value.X, Y = value, Z = Value.Z, W = Value.W };
                OnPropertyChanged();
            }
        }

        public float Z
        {
            get => Value.Z;
            set
            {
                m_value = new Vector4 { X = Value.X, Y = Value.Y, Z = value, W = Value.W };
                OnPropertyChanged();
            }
        }

        public float W
        {
            get => Value.Z;
            set
            {
                m_value = new Vector4 { X = Value.X, Y = Value.Y, Z = Value.Z, W = value };
                OnPropertyChanged();
            }
        }

        public override void UpdateMultiEditProperty(PropetyType type)
        {
            var ret = GetMultiEditValue(type, Value.X, Value.Y, Value.Z, Value.W);
            m_value = new Vector4(ret.X, ret.Y, ret.Z, ret.W);
        }
    }
}
