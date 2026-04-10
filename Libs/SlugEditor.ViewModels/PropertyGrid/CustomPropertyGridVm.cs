using Microsoft.Win32;
using SlugEditor.ViewModels.PropertyGrid.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace SlugEditor.ViewModels.PropertyGrid
{
    public class CustomPropertyGridVm : ViewModelBase
    {
        public ObservableCollection<PropertyBaseVm> PropertyItems
        {
            get => m_propertyItems;
            set => SetProperty(ref m_propertyItems, value);
        }
        private ObservableCollection<PropertyBaseVm> m_propertyItems;

        public CustomPropertyGridVm(object item, string url)
        {
            m_propertyItems = new ObservableCollection<PropertyBaseVm>();
            var descriptors = TypeDescriptor.GetProperties(item);
            foreach (PropertyDescriptor descriptor in descriptors)
            {
                var elementModel = descriptor.GetValue(item);
                if (elementModel != null)
                {
                    m_propertyItems.Add(PropertyVmBuilder.Build(elementModel, descriptor, url));
                }
            }
        }

        public IEnumerable GetChildren(object? parent)
        {
            var key = parent as PropertyBaseVm;
            if (parent == null)
            {
                foreach (var item in m_propertyItems)
                {
                    if (item.IsBrowsable)
                    {
                        yield return item;
                    }
                }
            }
            else if (key != null)
            {
                foreach (var element in key.Elemtents)
                {
                    if (element.IsBrowsable)
                    {
                        yield return element;
                    }
                }
            }
        }
        public bool HasChildren(object? parent)
        {
            if (parent is PropertyBaseVm vm)
            {
                return vm.Elemtents.Count > 0;
            }
            return false;
        }

        public bool IsExpanded(object? parent) 
        {
            if (parent is HasElementPropertyVm vm)
            {
                return vm.IsExpand;
            }
            return false;

        }

        public bool IsReadOnly(object? parent)
        {
            if (parent is PropertyBaseVm vm)
            {
                return vm.IsReadonly;
            }
            return false;

        }
    }
}
