using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace SlugEditor.Core.Properties;

public class TrackedList<T> : IList<T>, INotifyCollectionChanged, INotifyPropertyChanged
{
    private readonly List<T> _items = [];

    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    public int Count => _items.Count;
    public bool IsReadOnly => false;

    public T this[int index]
    {
        get => _items[index];
        set
        {
            var oldItem = _items[index];
            if (EqualityComparer<T>.Default.Equals(oldItem, value))
            {
                return;
            }

            _items[index] = value;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Replace,
                value,
                oldItem,
                index));
        }
    }

    public void Add(T item)
    {
        var index = _items.Count;
        _items.Add(item);
        OnCountChanged();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
    }

    public void Clear()
    {
        if (_items.Count is 0)
        {
            return;
        }

        _items.Clear();
        OnCountChanged();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public bool Contains(T item)
    {
        return _items.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        _items.CopyTo(array, arrayIndex);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    public int IndexOf(T item)
    {
        return _items.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
        _items.Insert(index, item);
        OnCountChanged();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
    }

    public bool Remove(T item)
    {
        var index = _items.IndexOf(item);
        if (index < 0)
        {
            return false;
        }

        _items.RemoveAt(index);
        OnCountChanged();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        return true;
    }

    public void RemoveAt(int index)
    {
        var item = _items[index];
        _items.RemoveAt(index);
        OnCountChanged();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private void OnCountChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
    }

    private void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
    {
        CollectionChanged?.Invoke(this, args);
    }
}
