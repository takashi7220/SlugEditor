using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SlugEditor.ViewModels.PropertyGrid.Types
{
    public class Vector3PropertyVm : VectorPropertyVm<Vector3>
    {
        public Vector3PropertyVm(Vector3 value)
            : base(value)
        {
        }

        public float X
        {
            get => Value.X;
            set
            {
                m_value = new Vector3 { X = value, Y = Value.Y, Z = Value.Z };
                OnPropertyChanged();
            }
        }

        public float Y
        {
            get => Value.Y;
            set
            {
                m_value = new Vector3 { X = Value.X, Y = value, Z = Value.Z };
                OnPropertyChanged();
            }
        }

        public float Z
        {
            get => Value.Z;
            set
            {
                m_value = new Vector3 { X = Value.X, Y = Value.Y, Z = value };
                OnPropertyChanged();
            }
        }

        public override void UpdateMultiEditProperty(PropetyType type)
        {
            var ret = GetMultiEditValue(type, Value.X, Value.Y, Value.Z);
            m_value = new Vector3(ret.X, ret.Y, ret.Z);
        }
    }
}
