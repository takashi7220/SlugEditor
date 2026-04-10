using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlugEditor.ViewModels.PropertyGrid.Types
{
    public class IntengerPropertyVm : NumericPropertyVm<int>
    {
        public IntengerPropertyVm(int value)
            : base(value)
        {
        }
    }
}
