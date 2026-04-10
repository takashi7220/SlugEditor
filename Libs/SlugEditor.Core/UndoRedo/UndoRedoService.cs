using Microsoft.Extensions.DependencyInjection;
using SlugEditor.Core.Service;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SlugEditor.Core.UndoRedo
{
    [Service(ServiceType.Singleton)]
    public class UndoRedoService : IService
    {
        public UndoRedoService() 
        {
            History = new History();
        }

        public History History { get; }
    }
}
