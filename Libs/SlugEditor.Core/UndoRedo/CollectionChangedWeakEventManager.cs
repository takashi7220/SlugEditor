using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlugEditor.Core.UndoRedo
{
    internal class CollectionChangedWeakEventManager : IDisposable
    {
        private readonly Dictionary<CollectionChangedWeakEventListener, NotifyCollectionChangedEventHandler> m_listeners = new();

        public void AddWeakEventListener(INotifyCollectionChanged source, NotifyCollectionChangedEventHandler handler)
        {
            m_listeners.Add(new CollectionChangedWeakEventListener(source, handler), handler);
        }

        public void RemoveWeakEventListener(INotifyCollectionChanged source)
        {
            var toRemoveListeners = ArrayPool<CollectionChangedWeakEventListener>.Shared.Rent(m_listeners.Count);

            try
            {
                var count = 0;

                foreach (var listener in m_listeners.Keys)
                {
                    if (listener is { IsAlive: false })
                    {
                        toRemoveListeners[count++] = listener;
                    }

                    else if (listener.Source == source)
                    {
                        listener.Dispose();
                        toRemoveListeners[count++] = listener;
                    }
                }

                for (var i = 0; i != count; ++i)
                {
                    m_listeners.Remove(toRemoveListeners[i]);
                }
            }
            finally
            {
                ArrayPool<CollectionChangedWeakEventListener>.Shared.Return(toRemoveListeners);
            }
        }

        public void Dispose()
        {
            foreach (var listener in m_listeners.Keys)
            {
                if (listener is { IsAlive: false })
                {
                    continue;
                }

                listener.Dispose();
            }

            m_listeners.Clear();
        }

        private sealed class CollectionChangedWeakEventListener : IDisposable
        {
            public bool IsAlive => m_handler.TryGetTarget(out _) && m_source.TryGetTarget(out _);
            public object? Source => m_source.TryGetTarget(out var source) ? source : default;

            private readonly WeakReference<INotifyCollectionChanged> m_source;
            private readonly WeakReference<NotifyCollectionChangedEventHandler> m_handler;

            public CollectionChangedWeakEventListener(INotifyCollectionChanged source, NotifyCollectionChangedEventHandler handler)
            {
                m_source = new WeakReference<INotifyCollectionChanged>(source);
                m_handler = new WeakReference<NotifyCollectionChangedEventHandler>(handler);

                source.CollectionChanged += HandleEvent;
            }

            private void HandleEvent(object? sender, NotifyCollectionChangedEventArgs e)
            {
                if (m_handler.TryGetTarget(out var handler))
                {
                    handler(sender, e);
                }
                else
                {
                    Dispose();
                }
            }

            public void Dispose()
            {
                if (m_source.TryGetTarget(out var source))
                {
                    source.CollectionChanged -= HandleEvent;
                }
            }
        }
    }
}
