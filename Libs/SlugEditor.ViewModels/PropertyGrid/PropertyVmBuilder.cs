using System.Collections;
using System.Numerics;
using System.Collections.ObjectModel;
using System.ComponentModel;
using SlugEditor.Core.Models;
using SlugEditor.ViewModels.PropertyGrid.Types;
using SlugEditor.Core.Service;
using SlugEditor.Core.Attributes;

namespace SlugEditor.ViewModels.PropertyGrid
{
    public class PropertyVmBuilder
    {
        public static PropertyBaseVm Build(object model, PropertyDescriptor descriptor)
        {
            var builderService = ServiceManager.GetService<PropertyGridBuilderService>();
            if (builderService != null)
            {
                var vm = builderService.BuildViewModel(model, descriptor);
                if (vm != null)
                {
                    return vm;
                }
            }

            if (IsOverrideProperty(model))
            {
                var properties = model.GetType().GetProperties();
                var valueProperty = properties.Where(x => x.Name == "Value").FirstOrDefault();
                if (valueProperty != null)
                {
                    var valueModel = valueProperty.GetValue(model);
                    if (valueModel != null)
                    {
                        var vm = Build(valueModel, descriptor);
                        SetupOverrideProperty(vm, model);
                        return vm;
                    }
                }
            }
            else if (model is bool boolModel)
            {
                var vm = new BooleanPropertyVm(boolModel);
                SetupProperty(vm, model, descriptor);
                return vm;
            }
            else if (model is int intModel)
            {
                var vm = new IntengerPropertyVm(intModel);
                SetupProperty(vm, model, descriptor);
                return vm;
            }
            else if (model is uint uintModel)
            {
                var vm = new UintengerPropertyVm(uintModel);
                SetupProperty(vm, model, descriptor);
                return vm;
            }
            else if (model is float floatModel)
            {
                var vm = new FloatPropertyVm(floatModel);
                SetupProperty(vm, model, descriptor);
                return vm;
            }
            else if (model is Enum enumModel)
            {
                var vm = new EnumPropertyVm(enumModel);
                SetupProperty(vm, model, descriptor);
                return vm;
            }
            else if (model is string stringModel)
            {
                var vm = new StringPropertyVm(stringModel);
                SetupProperty(vm, model, descriptor);
                return vm;
            }
            else if (model is Vector2 vec2Model)
            {
                var vm = new Vector2PropertyVm(vec2Model);
                SetupProperty(vm, model, descriptor);
                return vm;
            }
            else if (model is Vector3 vec3Model)
            {
                var vm = new Vector3PropertyVm(vec3Model);
                SetupProperty(vm, model, descriptor);
                return vm;
            }
            else if (model is Vector4 vec4Model)
            {
                var vm = new Vector4PropertyVm(vec4Model);
                SetupProperty(vm, model, descriptor);
                return vm;
            }
            else if (model is Array)
            {
                var ret = new VariablePropertyVm(model);
                ret.Elemtents = new ObservableCollection<PropertyBaseVm>(BuildElements(model, descriptor));
                ret.IsVariable = false;
                SetupProperty(ret, model, descriptor);
                return ret;
            }
            else if (model is IList)
            {
                var ret = new VariablePropertyVm(model);
                ret.Elemtents = new ObservableCollection<PropertyBaseVm>(BuildElements(model, descriptor));
                ret.IsVariable = true;
                SetupProperty(ret, model, descriptor);
                return ret;
            }
            else if (model.GetType().IsClass)
            {
                var ret = new StructPropertyVm(model);
                ret.Elemtents = new ObservableCollection<PropertyBaseVm>(BuildElements(model, descriptor));
                SetupProperty(ret, model, descriptor);
                return ret;
            }
            return BuildDefault(model);
        }

        public static IEnumerable<PropertyBaseVm> BuildElements(object model, PropertyDescriptor ownerDescriptor)
        {
            var elements = new List<PropertyBaseVm>();
            if (model is Array arrayModel)
            {
                for (var i = 0; i < arrayModel.Length; i++)
                {
                    var elementModel = arrayModel.GetValue(i);
                    if (elementModel != null)
                    {
                        var elementProp = Build(elementModel, ownerDescriptor);
                        SetupIEnamerateProperty(elementProp, i);
                        elements.Add(elementProp);
                    }
                }
            }
            else if (model is IList listModel)
            {
                for (var i = 0; i < listModel.Count; i++)
                {
                    var elementModel = listModel[i];
                    if (elementModel != null)
                    {
                        var elementProp = Build(elementModel, ownerDescriptor);
                        SetupIEnamerateProperty(elementProp, i);
                        elements.Add(elementProp);
                    }
                }
            }
            else if (model.GetType().IsClass)
            {
                var descriptors = TypeDescriptor.GetProperties(model);
                foreach (PropertyDescriptor descriptor in descriptors)
                {
                    var elementModel = descriptor.GetValue(model);
                    if (elementModel != null)
                    {
                        var elementProp = Build(elementModel, descriptor);
                        elements.Add(elementProp);
                    }
                }
            }
            return elements;
        }

        public static PropertyBaseVm BuildDefault(object value)
        {
            var typeStr = value.GetType().ToString();
            var prop = new StringPropertyVm("error");
            prop.Caption = "Error";
            prop.Description = "Error";
            prop.IsReadonly = true;
            prop.IsBrowsable = true;
            return prop;
        }

        public static bool IsOverrideProperty(object value)
        {
            Type type = value.GetType();
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(OverrideProperty<>);
        }

        public static void SetupProperty(PropertyBaseVm vm, object model, PropertyDescriptor descriptor)
        {
            vm.Name = GetName(descriptor);
            vm.Caption = GetCaption(descriptor);
            vm.Description = GetDescription(descriptor);
            vm.IsReadonly = GetIsReadOnly(descriptor);
            vm.IsBrowsable = GetIsBrowsable(descriptor);
            if (vm is StructPropertyVm structPropVm)
            {
                var expandAttribute = descriptor.Attributes?.OfType<ExpandAttribute>()?.FirstOrDefault();
                structPropVm.IsExpand = expandAttribute?.IsExpanded ?? false;
            }
        }

        public static void SetupOverrideProperty(PropertyBaseVm vm, object model)
        {
            vm.IsOverrideFunc = GetIsOverrideFunc(ref model);
            vm.ResetAction = GetResetAction(ref model);
        }

        public static string GetName(PropertyDescriptor descriptor)
        {
            return descriptor.Name;
        }

        public static string GetCaption(PropertyDescriptor descriptor)
        {
            string caption = descriptor.DisplayName;
            if (string.IsNullOrEmpty(caption)) 
            {
                caption = descriptor.Name;
            }
            return caption;
        }

        public static string GetDescription(PropertyDescriptor descriptor)
        {
            var description = descriptor.DisplayName;
            if (string.IsNullOrEmpty(description))
            {
                description = descriptor.Name;
            }
            return description;
        }

        public static bool GetIsReadOnly(PropertyDescriptor descriptor)
        {
            return descriptor.IsReadOnly;
        }

        public static bool GetIsBrowsable(PropertyDescriptor descriptor)
        {
            return descriptor.IsBrowsable;
        }

        public static bool GetIsOverrideProperty(ref object model)
        {
            return false;
        }

        public static Func<bool>? GetIsOverrideFunc(ref object model)
        {
            if (model is IOverrideProperty overrideProperty)
            {
                return overrideProperty.IsOverride;
            }
            return null;
        }

        public static Action? GetResetAction(ref object model)
        {
			if (model is IOverrideProperty overrideProperty)
			{
				return overrideProperty.Reset;
			}
			return null;
		}

        public static void SetupIEnamerateProperty(PropertyBaseVm vm, int index) 
        {
            vm.Name = $"[{index}]";
            vm.Caption = $"[{index}]";
        }
    }
}
