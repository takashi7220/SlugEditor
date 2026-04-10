using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlugEditor.ViewModels.PropertyGrid.Types
{
    public class BooleanPropertyVm : PropertyBaseVm<bool>
    {
        public BooleanPropertyVm(bool value)
            : base(value)
        {
        }
    }
}
