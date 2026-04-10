using System.Collections.ObjectModel;
using SlugEditor.Core.Models;
using SlugEditor.ViewModels.Command;

namespace SlugEditor.ViewModels.PropertyGrid.Types
{
    public class PropertyBaseVm : OverridePropertyVm
    {
        public PropertyBaseVm(Type valueType)
            : base()
        {
            m_valueType = valueType;
        }

        public Type ValueType
        {
            get => m_valueType;
        }
        private Type m_valueType;

        public string Name
        {
            get => m_name;
            set => SetProperty(ref m_name, value);
        }
        private string m_name = string.Empty;

        public string Caption
        {
            get => m_caption;
            set => SetProperty(ref m_caption, value);
        }
        private string m_caption = string.Empty;

        public string Description
        {
            get => m_description;
            set => SetProperty(ref m_description, value);
        }
        private string m_description = string.Empty;

        public bool IsReadonly
        {
            get => m_isReadonly;
            set => SetProperty(ref m_isReadonly, value);
        }
        private bool m_isReadonly = false;

        public bool IsBrowsable
        {
            get => m_isBrowsable;
            set => SetProperty(ref m_isBrowsable, value);
        }
        private bool m_isBrowsable = true;

        public ObservableCollection<PropertyBaseVm> Elemtents
        {
            get => m_elements;
            set => SetProperty(ref m_elements, value);
        }
        private ObservableCollection<PropertyBaseVm> m_elements = new ObservableCollection<PropertyBaseVm>();

        public virtual object? GetValue() 
        {
            return null;
        }
    }

    public class PropertyBaseVm<T> : PropertyBaseVm
    {
        public PropertyBaseVm(T value)
            : base(typeof(T))
        {
            m_value = value;
        }

        public T Value
        {
            get => m_value;
            set => SetProperty(ref m_value, value);
        }
        protected T m_value;

        public override object? GetValue()
        {
            if (Value != null)
            {
                return (object)Value;
            }
            return null;
        }
    }
}
