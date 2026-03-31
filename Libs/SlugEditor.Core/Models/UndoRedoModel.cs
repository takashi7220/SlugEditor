using System.Reflection;
using System.Runtime.CompilerServices;

using SlugEditor.Core.UndoRedo;

namespace SlugEditor.Core.Models;

public class UndoRedoModel : ModelBase
{
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
