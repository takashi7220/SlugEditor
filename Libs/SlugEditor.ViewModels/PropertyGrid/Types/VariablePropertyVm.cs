using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SlugEditor.ViewModels.Command;

namespace SlugEditor.ViewModels.PropertyGrid.Types
{
    public class VariablePropertyVm : HasElementPropertyVm
    {
        public VariablePropertyVm(object value)
            : base(value)
        {
            AddElementCommand = new RelayCommand(p =>
            {
                var args = new ElementChangedEventArgs();
                AddElemendEvent?.Invoke(args);
            });

            RemoveElementCommand = new RelayCommand(p =>
            {
                var args = new ElementChangedEventArgs();
                RemoveElemendEvent?.Invoke(args);
            });

            ClearElementCommand = new RelayCommand(p =>
            {
                var args = new ElementChangedEventArgs();
                ClearElemendEvent?.Invoke(args);
            });
        }

        public bool IsVariable
        {
            get => m_isVariable;
            set => SetProperty(ref m_isVariable, value);
        }
        private bool m_isVariable = false;

        public ICommand AddElementCommand;
        public ICommand RemoveElementCommand;
        public ICommand ClearElementCommand;

        public class ElementChangedEventArgs
        {
        }

        public delegate void ElementChangedEventHandler(ElementChangedEventArgs args);

        public event ElementChangedEventHandler? AddElemendEvent;
        public event ElementChangedEventHandler? RemoveElemendEvent;
        public event ElementChangedEventHandler? ClearElemendEvent;
    }
}
