using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlugEditor.ViewModels.PropertyGrid.Types
{
    public class FloatPropertyVm : NumericPropertyVm<float>
    {
        public FloatPropertyVm(float value)
            : base(value)
        {
        }
    }
}
