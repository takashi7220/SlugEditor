using SlugEditor.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlugEditor.ViewModels.PropertyGrid.Types
{
    public class EnumPropertyVm : PropertyBaseVm<string>
    {
        public EnumPropertyVm(Enum value)
            : base(value.ToString())
        {
            m_enumType = value.GetType();
        }

        public IEnumerable<string> EnumNames
        {
            get
            {
                return Enum.GetNames(m_enumType);
            }
        }

        private Type m_enumType;
    }
}
