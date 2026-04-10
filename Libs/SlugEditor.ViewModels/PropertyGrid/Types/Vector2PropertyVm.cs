using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SlugEditor.ViewModels.PropertyGrid.Types
{
    public class Vector2PropertyVm : VectorPropertyVm<Vector2>
    {
        public Vector2PropertyVm(Vector2 value)
            : base(value)
        {
        }

        public float X
        {
            get => Value.X;
            set
            {
                OnpropertyChanging(Value.X, value);
                m_value = new Vector2 { X = value, Y = Value.Y };
                OnPropertyChanged();
            }
        }

        public float Y
        {
            get => Value.Y;
            set
            {
                OnpropertyChanging(Value.Y, value);
                m_value = new Vector2 { X = Value.X, Y = value };
                OnPropertyChanged();
            }
        }

        public override void UpdateMultiEditProperty(PropetyType type)
        {
            var ret = GetMultiEditValue(type, Value.X, Value.Y);
            m_value = new Vector2(ret.X, ret.Y);
        }
    }
}
