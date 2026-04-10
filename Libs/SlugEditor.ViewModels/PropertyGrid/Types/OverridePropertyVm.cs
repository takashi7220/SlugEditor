using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SlugEditor.ViewModels.Command;

namespace SlugEditor.ViewModels.PropertyGrid.Types
{
    public class OverridePropertyVm : ViewModelBase
    {
        public OverridePropertyVm()
        {
            ResetCommand = new RelayCommand(p =>
            {
                ResetAction?.Invoke();
            });
        }

        public bool IsOverride
        {
            get
            {
                return IsOverrideFunc?.Invoke() ?? false;
            }
        }

        public Func<bool>? IsOverrideFunc { get; set; }

        public Action? ResetAction { get; set; }

        public ICommand ResetCommand { get; set; }

    }
}
