using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlugEditor.ViewModels.PropertyGrid.Types
{
    public class NumericPropertyVm<T> : PropertyBaseVm<T>
    {
        public NumericPropertyVm(T value)
            : base(value)
        {
        }

        public double Min
        {
            get => m_min;
            set => SetProperty(ref m_min, value);
        }
        private double m_min = 0.0;

        public double Max
        {
            get => m_max;
            set => SetProperty(ref m_max, value);
        }
        private double m_max = 0.0;

        public double Step
        {
            get => m_step;
            set => SetProperty(ref m_step, value);
        }
        private double m_step;

        public bool Slider
        {
            get => m_slider;
            set => SetProperty(ref m_slider, value);
        }
        private bool m_slider = false;
    }
}
