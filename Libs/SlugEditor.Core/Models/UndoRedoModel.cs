using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

using SlugEditor.Core.UndoRedo;

namespace SlugEditor.Core.Models;

public class UndoRedoModel : ModelBase
{
    private readonly Dictionary<(INotifyPropertyChanged Target, string PropertyName), object?> _nestedPropertyCache = [];
    private readonly List<(INotifyPropertyChanged Target, PropertyChangedEventHandler Handler)> _nestedSubscriptions = [];
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
