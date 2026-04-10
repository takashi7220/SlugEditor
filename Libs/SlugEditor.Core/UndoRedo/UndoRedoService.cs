using SlugEditor.Core.Service;


namespace SlugEditor.Core.UndoRedo
{
    [Service(ServiceType.Singleton, ServiceRegisterType.Auto)]
    public class UndoRedoService : IService
    {
        public UndoRedoService() 
        {
            History = new History();
        }

        public History History { get; }
    }
}
