using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace SlugEditor.ViewModels.PropertyGrid.Types
{
    public class HasElementPropertyVm : PropertyBaseVm<object>
    {
        public HasElementPropertyVm(object value)
            : base(value)
        {
        }

        public bool IsExpand
        {
            get => m_isExpand;
            set => SetProperty(ref m_isExpand, value);
        }
        private bool m_isExpand = false;
    }
}
