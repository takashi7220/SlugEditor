using System.ComponentModel;

namespace SlugEditor.Core.UndoRedo;

public class History : INotifyPropertyChanged, IDisposable
{
    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;
    public bool CanClear => CanUndo || CanRedo;
    public int UndoCount => _undoStack.Count;
    public int RedoCount => _redoStack.Count;
    public int PauseDepth { get; private set; }
    public int BatchDepth { get; private set; }
    public bool IsInUndoing { get; private set; }
    public bool IsInPaused => PauseDepth > 0;
    public bool IsInBatch => BatchDepth > 0;

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Dispose()
    {
        _batchHistory?.Dispose();
        _batchHistory = null;
        _undoStack.Clear();
        _redoStack.Clear();
    }

    public void BeginPause()
    {
        var currentDepth = PauseBatchDepth;

        PauseDepth++;
        InvokePropertyChanged(currentDepth);
    }

    public void EndPause()
    {
        if (PauseDepth is 0)
        {
            throw new InvalidOperationException("Pause is not begun.");
        }

        var currentDepth = PauseBatchDepth;

        PauseDepth--;
        InvokePropertyChanged(currentDepth);
    }

    public void BeginBatch()
    {
        var currentDepth = PauseBatchDepth;

        BatchDepth++;
        if (BatchDepth is 1)
        {
            _batchHistory = new BatchHistory();
        }

        InvokePropertyChanged(currentDepth);
    }

    public void EndBatch()
    {
        if (BatchDepth is 0)
        {
            throw new InvalidOperationException("Batch recording has not begun.");
        }

        var currentFlags = CanUndoRedoClear;
        var currentUndoRedoCount = UndoRedoCount;
        var currentDepth = PauseBatchDepth;

        BatchDepth--;
        if (BatchDepth is 0)
        {
            var batchHistory = _batchHistory;
            _batchHistory = null;

            if (batchHistory is not null && batchHistory.CanUndo)
            {
                _undoStack.Push(new HistoryAction(batchHistory.UndoAll, batchHistory.RedoAll));

                if (_redoStack.Count > 0)
                {
                    _redoStack.Clear();
                }
            }

            batchHistory?.Dispose();
        }

        InvokePropertyChanged(currentFlags, currentUndoRedoCount, currentDepth);
    }

    public void Undo()
    {
        if (IsInBatch)
        {
            throw new InvalidOperationException("Can't call Undo() during batch recording.");
        }

        if (IsInPaused)
        {
            throw new InvalidOperationException("Can't call Undo() while paused.");
        }

        if (!CanUndo)
        {
            return;
        }

        var currentFlags = CanUndoRedoClear;
        var currentUndoRedoCount = UndoRedoCount;
        var currentIsInUndoing = IsInUndoing;

        var action = _undoStack.Pop();
        try
        {
            IsInUndoing = true;
            action.Undo();
        }
        finally
        {
            IsInUndoing = false;
        }

        _redoStack.Push(action);

        InvokePropertyChanged(currentFlags, currentUndoRedoCount, currentIsInUndoing);
    }

    public void Redo()
    {
        if (IsInBatch)
        {
            throw new InvalidOperationException("Can't call Redo() during batch recording.");
        }

        if (IsInPaused)
        {
            throw new InvalidOperationException("Can't call Redo() while paused.");
        }

        if (!CanRedo)
        {
            return;
        }

        var currentFlags = CanUndoRedoClear;
        var currentUndoRedoCount = UndoRedoCount;
        var currentIsInUndoing = IsInUndoing;

        var action = _redoStack.Pop();
        try
        {
            IsInUndoing = true;
            action.Redo();
        }
        finally
        {
            IsInUndoing = false;
        }

        _undoStack.Push(action);

        InvokePropertyChanged(currentFlags, currentUndoRedoCount, currentIsInUndoing);
    }

    public void Push(Action undo, Action redo)
    {
        ArgumentNullException.ThrowIfNull(undo);
        ArgumentNullException.ThrowIfNull(redo);

        if (IsInPaused)
        {
            return;
        }

        if (IsInBatch)
        {
            _ = _batchHistory ?? throw new InvalidOperationException("Batch recording has not begun.");
            _batchHistory.Push(undo, redo);
            return;
        }

        var currentFlags = CanUndoRedoClear;
        var currentUndoRedoCount = UndoRedoCount;

        _undoStack.Push(new HistoryAction(undo, redo));
        if (_redoStack.Count > 0)
        {
            _redoStack.Clear();
        }

        InvokePropertyChanged(currentFlags, currentUndoRedoCount);
    }

    public void Clear()
    {
        if (_undoStack.Count is 0 && _redoStack.Count is 0)
        {
            return;
        }

        var currentFlags = CanUndoRedoClear;
        var currentUndoRedoCount = UndoRedoCount;

        _undoStack.Clear();
        _redoStack.Clear();

        InvokePropertyChanged(currentFlags, currentUndoRedoCount);
    }

    private void InvokePropertyChanged(
        (bool CanUndo, bool CanRedo, bool CanClear) previousFlags,
        (int UndoCount, int RedoCount) previousCount)
    {
        if (previousFlags.CanUndo != CanUndo)
        {
            PropertyChanged?.Invoke(this, CanUndoArgs);
        }

        if (previousFlags.CanRedo != CanRedo)
        {
            PropertyChanged?.Invoke(this, CanRedoArgs);
        }

        if (previousFlags.CanClear != CanClear)
        {
            PropertyChanged?.Invoke(this, CanClearArgs);
        }

        if (previousCount.UndoCount != UndoCount)
        {
            PropertyChanged?.Invoke(this, UndoCountArgs);
        }

        if (previousCount.RedoCount != RedoCount)
        {
            PropertyChanged?.Invoke(this, RedoCountArgs);
        }
    }

    private void InvokePropertyChanged(
        (bool CanUndo, bool CanRedo, bool CanClear) previousFlags,
        (int UndoCount, int RedoCount) previousCount,
        (int PauseDepth, int BatchDepth) previousDepth)
    {
        InvokePropertyChanged(previousFlags, previousCount);
        InvokePropertyChanged(previousDepth);
    }

    private void InvokePropertyChanged((int PauseDepth, int BatchDepth) previousDepth)
    {
        if (previousDepth.PauseDepth != PauseDepth)
        {
            PropertyChanged?.Invoke(this, PauseDepthArgs);
            PropertyChanged?.Invoke(this, IsInPausedArgs);
        }

        if (previousDepth.BatchDepth != BatchDepth)
        {
            PropertyChanged?.Invoke(this, BatchDepthArgs);
            PropertyChanged?.Invoke(this, IsInBatchArgs);
        }
    }

    private void InvokePropertyChanged(
        (bool CanUndo, bool CanRedo, bool CanClear) previousFlags,
        (int UndoCount, int RedoCount) previousCount,
        bool previousIsInUndoing)
    {
        InvokePropertyChanged(previousFlags, previousCount);

        if (previousIsInUndoing != IsInUndoing)
        {
            PropertyChanged?.Invoke(this, IsInUndoingArgs);
        }
    }

    private sealed class BatchHistory : History
    {
        public void UndoAll()
        {
            while (CanUndo)
            {
                Undo();
            }
        }

        public void RedoAll()
        {
            while (CanRedo)
            {
                Redo();
            }
        }
    }

    private BatchHistory? _batchHistory;

    private (int UndoCount, int RedoCount) UndoRedoCount => (UndoCount, RedoCount);
    private (bool CanUndo, bool CanRedo, bool CanClear) CanUndoRedoClear => (CanUndo, CanRedo, CanClear);
    private (int PauseDepth, int BatchDepth) PauseBatchDepth => (PauseDepth, BatchDepth);

    private readonly record struct HistoryAction(Action Undo, Action Redo);
    private readonly Stack<HistoryAction> _undoStack = new();
    private readonly Stack<HistoryAction> _redoStack = new();

    private static readonly PropertyChangedEventArgs CanUndoArgs = new(nameof(CanUndo));
    private static readonly PropertyChangedEventArgs CanRedoArgs = new(nameof(CanRedo));
    private static readonly PropertyChangedEventArgs CanClearArgs = new(nameof(CanClear));
    private static readonly PropertyChangedEventArgs UndoCountArgs = new(nameof(UndoCount));
    private static readonly PropertyChangedEventArgs RedoCountArgs = new(nameof(RedoCount));
    private static readonly PropertyChangedEventArgs PauseDepthArgs = new(nameof(PauseDepth));
    private static readonly PropertyChangedEventArgs BatchDepthArgs = new(nameof(BatchDepth));
    private static readonly PropertyChangedEventArgs IsInPausedArgs = new(nameof(IsInPaused));
    private static readonly PropertyChangedEventArgs IsInBatchArgs = new(nameof(IsInBatch));
    private static readonly PropertyChangedEventArgs IsInUndoingArgs = new(nameof(IsInUndoing));
}
