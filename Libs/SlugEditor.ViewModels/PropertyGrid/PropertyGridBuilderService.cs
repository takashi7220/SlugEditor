using SlugEditor.Core.Assembly;
using SlugEditor.Core.Service;
using SlugEditor.ViewModels.PropertyGrid.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlugEditor.ViewModels.PropertyGrid
{
    public interface IPropertyViewModelBuilder
    {
        int Priority { get; set; }
        bool IsMatch(object value);
        PropertyBaseVm? Build(object value, PropertyDescriptor descriptor);
    }

    [Service(ServiceType.Singleton, ServiceRegistType.Auto)]
    public class PropertyGridBuilderService : IService
    {
        private List<IPropertyViewModelBuilder> m_propertyViewModelBuilders = new List<IPropertyViewModelBuilder>();

        public PropertyGridBuilderService(AssemblyService assemblyService)
        {
            SetupBuildTable(assemblyService);
        }

        public PropertyBaseVm? BuildViewModel(object value, PropertyDescriptor descriptor)
        {
            foreach (var builder in m_propertyViewModelBuilders) 
            {
                if (builder.IsMatch(value)) 
                {
                    return builder.Build(value, descriptor);
                }
            }
            return null;
        }

        private void SetupBuildTable(AssemblyService assemblyService) 
        {
            if (assemblyService != null)
            {
                foreach (var assembly in assemblyService.EnumerateAssembly())
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (!type.IsClass || type.IsInterface)
                        {
                            continue;
                        }

                        if (typeof(IPropertyViewModelBuilder).IsAssignableFrom(type))
                        {
                            var buildViewModel = Activator.CreateInstance(type) as IPropertyViewModelBuilder;
                            if (buildViewModel != null)
                            {
                                m_propertyViewModelBuilders.Add(buildViewModel);
                            }
                        }
                    }
                }
            }
        }
    }
}
