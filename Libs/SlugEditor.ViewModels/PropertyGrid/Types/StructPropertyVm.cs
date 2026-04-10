using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlugEditor.ViewModels.PropertyGrid.Types
{
    public class StructPropertyVm : HasElementPropertyVm
    {
        public StructPropertyVm(object value)
            : base(value)
        {
            m_strcutType = value.GetType();
        }

        public Type StructType
        {
            get => m_strcutType;
            set => SetProperty(ref m_strcutType, value);
        }
        private Type m_strcutType;
    }
}
