using System.ComponentModel;
using System.Collections.Specialized;
using System.Reflection;
using System.Runtime.CompilerServices;

using SlugEditor.Core.UndoRedo;

namespace SlugEditor.Core.Models;

public class UndoRedoModel : ModelBase
{
    private readonly Dictionary<(INotifyPropertyChanged Target, string PropertyName), object?> _nestedPropertyCache = [];
    private readonly List<(INotifyPropertyChanged Target, PropertyChangedEventHandler Handler)> _nestedSubscriptions = [];
    private readonly List<(INotifyCollectionChanged Target, NotifyCollectionChangedEventHandler Handler)> _nestedCollectionSubscriptions = [];
    private int _nestedReplayDepth;

    protected UndoRedoModel(History? history = null)
    {
        History = history ?? new History();
    }

    public History History { get; }

    protected bool SetTrackedProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            throw new ArgumentException("Property name is required.", nameof(propertyName));
        }

        var oldValue = field;
        if (!SetProperty(ref field, value, propertyName))
        {
            return false;
        }

        // Undo/Redo replay should not enqueue a new history item.
        if (History.IsInUndoing)
        {
            return true;
        }

        var newValue = value;
        History.Push(
            undo: () => SetTrackedPropertyValue(propertyName, oldValue),
            redo: () => SetTrackedPropertyValue(propertyName, newValue));

        return true;
    }

    protected void TrackNestedProperties(
        INotifyPropertyChanged nestedObject,
        string ownerPropertyName,
        params string[] ignoredPropertyNames)
    {
        ArgumentNullException.ThrowIfNull(nestedObject);
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerPropertyName);
        var ignored = ignoredPropertyNames.Length is 0
            ? null
            : new HashSet<string>(ignoredPropertyNames, StringComparer.Ordinal);

        var properties = nestedObject
            .GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.CanRead && p.CanWrite && (ignored is null || !ignored.Contains(p.Name)))
            .ToArray();

        foreach (var property in properties)
        {
            _nestedPropertyCache[(nestedObject, property.Name)] = property.GetValue(nestedObject);
        }

        PropertyChangedEventHandler handler = (_, e) =>
        {
            if (string.IsNullOrWhiteSpace(e.PropertyName))
            {
                return;
            }
            if (ignored is not null && ignored.Contains(e.PropertyName))
            {
                return;
            }

            var changedProperty = nestedObject.GetType().GetProperty(
                e.PropertyName,
                BindingFlags.Instance | BindingFlags.Public);
            if (changedProperty is null || !changedProperty.CanRead || !changedProperty.CanWrite)
            {
                return;
            }

            var key = (nestedObject, e.PropertyName);
            if (!_nestedPropertyCache.TryGetValue(key, out var oldValue))
            {
                oldValue = changedProperty.GetValue(nestedObject);
                _nestedPropertyCache[key] = oldValue;
                return;
            }

            var newValue = changedProperty.GetValue(nestedObject);
            if (Equals(oldValue, newValue))
            {
                return;
            }

            _nestedPropertyCache[key] = newValue;

            if (_nestedReplayDepth > 0 || History.IsInUndoing)
            {
                OnPropertyChanged(ownerPropertyName);
                return;
            }

            History.Push(
                undo: () =>
                {
                    SetNestedPropertyValue(nestedObject, changedProperty, oldValue);
                    OnPropertyChanged(ownerPropertyName);
                },
                redo: () =>
                {
                    SetNestedPropertyValue(nestedObject, changedProperty, newValue);
                    OnPropertyChanged(ownerPropertyName);
                });

            OnPropertyChanged(ownerPropertyName);
        };

        nestedObject.PropertyChanged += handler;
        _nestedSubscriptions.Add((nestedObject, handler));
    }

    protected void TrackNestedCollection<T>(
        IList<T> list,
        INotifyCollectionChanged collectionObject,
        string ownerPropertyName)
    {
        ArgumentNullException.ThrowIfNull(list);
        ArgumentNullException.ThrowIfNull(collectionObject);
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerPropertyName);

        NotifyCollectionChangedEventHandler handler = (_, e) =>
        {
            if (_nestedReplayDepth > 0 || History.IsInUndoing)
            {
                OnPropertyChanged(ownerPropertyName);
                return;
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    if (e.NewItems is null || e.NewItems.Count is 0 || e.NewStartingIndex < 0)
                    {
                        return;
                    }

                    var item = (T)e.NewItems[0]!;
                    var index = e.NewStartingIndex;
                    History.Push(
                        undo: () =>
                        {
                            _nestedReplayDepth++;
                            try
                            {
                                list.RemoveAt(index);
                            }
                            finally
                            {
                                _nestedReplayDepth--;
                            }
                            OnPropertyChanged(ownerPropertyName);
                        },
                        redo: () =>
                        {
                            _nestedReplayDepth++;
                            try
                            {
                                list.Insert(index, item);
                            }
                            finally
                            {
                                _nestedReplayDepth--;
                            }
                            OnPropertyChanged(ownerPropertyName);
                        });

                    OnPropertyChanged(ownerPropertyName);
                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    if (e.OldItems is null || e.OldItems.Count is 0 || e.OldStartingIndex < 0)
                    {
                        return;
                    }

                    var item = (T)e.OldItems[0]!;
                    var index = e.OldStartingIndex;
                    History.Push(
                        undo: () =>
                        {
                            _nestedReplayDepth++;
                            try
                            {
                                list.Insert(index, item);
                            }
                            finally
                            {
                                _nestedReplayDepth--;
                            }
                            OnPropertyChanged(ownerPropertyName);
                        },
                        redo: () =>
                        {
                            _nestedReplayDepth++;
                            try
                            {
                                list.RemoveAt(index);
                            }
                            finally
                            {
                                _nestedReplayDepth--;
                            }
                            OnPropertyChanged(ownerPropertyName);
                        });

                    OnPropertyChanged(ownerPropertyName);
                    break;
                }
                case NotifyCollectionChangedAction.Replace:
                {
                    if (e.NewItems is null || e.NewItems.Count is 0 || e.OldItems is null || e.OldItems.Count is 0 || e.NewStartingIndex < 0)
                    {
                        return;
                    }

                    var newItem = (T)e.NewItems[0]!;
                    var oldItem = (T)e.OldItems[0]!;
                    var index = e.NewStartingIndex;
                    History.Push(
                        undo: () =>
                        {
                            _nestedReplayDepth++;
                            try
                            {
                                list[index] = oldItem;
                            }
                            finally
                            {
                                _nestedReplayDepth--;
                            }
                            OnPropertyChanged(ownerPropertyName);
                        },
                        redo: () =>
                        {
                            _nestedReplayDepth++;
                            try
                            {
                                list[index] = newItem;
                            }
                            finally
                            {
                                _nestedReplayDepth--;
                            }
                            OnPropertyChanged(ownerPropertyName);
                        });

                    OnPropertyChanged(ownerPropertyName);
                    break;
                }
                default:
                    break;
            }
        };

        collectionObject.CollectionChanged += handler;
        _nestedCollectionSubscriptions.Add((collectionObject, handler));
    }

    private void SetNestedPropertyValue(INotifyPropertyChanged target, PropertyInfo property, object? value)
    {
        _nestedReplayDepth++;
        try
        {
            property.SetValue(target, value);
            _nestedPropertyCache[(target, property.Name)] = value;
        }
        finally
        {
            _nestedReplayDepth--;
        }
    }

    private void SetTrackedPropertyValue<T>(string propertyName, T value)
    {
        var property = GetType().GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (property is null || !property.CanWrite)
        {
            throw new InvalidOperationException(
                $"Property '{propertyName}' was not found or is not writable in {GetType().Name}.");
        }

        property.SetValue(this, value);
    }
}
