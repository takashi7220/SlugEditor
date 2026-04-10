using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.ComponentModel;

namespace SlugEditor.ViewModels.PropertyGrid.Types
{
    public class VectorPropertyVm<T> : NumericPropertyVm<T>
    {
        public enum PropetyType
        {
            X,
            Y,
            Z,
            W
        }

        public enum MultiEdit
        {
            None,
            SameValue,
            RateValue,
        }

        public VectorPropertyVm(T value)
            : base(value)
        {
            PropertyChanged += VectorPropertyChanged;
        }

        public MultiEdit MultiEditType
        {
            get => m_multiEditType;
            set
            {
                m_multiEditType = value;
                OnPropertyChanged();
            }
        }
        private MultiEdit m_multiEditType = MultiEdit.None;

        public bool IsMultiEdit
        {
            get => MultiEditType == MultiEdit.None;
        }

        protected float Scale { get; set; } = 1.0f;
        protected float Offset { get; set; } = 0.0f;

        protected void OnpropertyChanging(float preValue, float nextValue)
        {
            if (Math.Abs(preValue) > float.Epsilon)
            {
                Scale = nextValue / preValue;
                Offset = 0.0f;
            }
            else
            {
                Scale = 1.0f;
                Offset = nextValue;
            }
        }

        public virtual void UpdateMultiEditProperty(PropetyType type)
        {
        }

        protected Vector4 GetMultiEditValue(PropetyType type, float x, float y, float z = 0.0f, float w = 0.0f)
        {
            var ret = new Vector4();
            if (MultiEditType == MultiEdit.SameValue)
            {
                if (type == PropetyType.X)
                {
                    ret.X = x;
                    ret.Y = x;
                    ret.Z = x;
                    ret.W = x;
                }
                else if (type == PropetyType.Y)
                {
                    ret.X = y;
                    ret.Y = y;
                    ret.Z = y;
                    ret.W = y;
                }
                else if (type == PropetyType.Z)
                {
                    ret.X = z;
                    ret.Y = z;
                    ret.Z = z;
                    ret.W = z;
                }
                else if (type == PropetyType.W)
                {
                    ret.X = w;
                    ret.Y = w;
                    ret.Z = w;
                    ret.W = w;
                }
            }
            else
            {
                if (type == PropetyType.X)
                {
                    ret.X = x;
                    ret.Y = y * Scale + Offset;
                    ret.Z = z * Scale + Offset;
                    ret.W = w * Scale + Offset;
                }
                else if (type == PropetyType.Y)
                {
                    ret.X = x * Scale + Offset;
                    ret.Y = y;
                    ret.Z = z * Scale + Offset;
                    ret.W = w * Scale + Offset;
                }
                else if (type == PropetyType.Z)
                {
                    ret.X = x * Scale + Offset;
                    ret.Y = y * Scale + Offset;
                    ret.Z = z;
                    ret.W = w * Scale + Offset;
                }
                else if (type == PropetyType.W)
                {
                    ret.X = x * Scale + Offset;
                    ret.Y = y * Scale + Offset;
                    ret.Z = z * Scale + Offset;
                    ret.W = w;
                }
            }
            return ret;
        }

        private static void VectorPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is VectorPropertyVm<T> vectorVm)
            {
                if (vectorVm.IsMultiEdit)
                {
                    if (e.PropertyName == "X")
                    {
                        vectorVm.UpdateMultiEditProperty(PropetyType.X);
                    }
                    else if (e.PropertyName == "Y")
                    {
                        vectorVm.UpdateMultiEditProperty(PropetyType.Y);
                    }
                    else if (e.PropertyName == "Z")
                    {
                        vectorVm.UpdateMultiEditProperty(PropetyType.Z);
                    }
                    else if (e.PropertyName == "W")
                    {
                        vectorVm.UpdateMultiEditProperty(PropetyType.W);
                    }
                }
            }
        }
    }
}
