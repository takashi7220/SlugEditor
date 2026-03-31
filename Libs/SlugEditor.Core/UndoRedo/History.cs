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

    internal readonly CollectionChangedWeakEventManager CollectionChangedWeakEventManager = new();

    public void Dispose()
    {
    }

    public void BeginPause()
    {
        ++PauseDepth;
    }

    public void EndPause()
    {
        if (PauseDepth is 0)
        {
            throw new InvalidOperationException("Pause is not begun.");
        }

        --PauseDepth;
    }

    public void BeginBatch()
    {
        ++BatchDepth;

        if (BatchDepth is 1)
        {
            BeginBatchInternal();
        }

        public void EndBatch()
        {
            if (BatchDepth is 0)
            {
                throw new InvalidOperationException("Batch recording has not begun.");
            }

            --BatchDepth;

            if (BatchDepth is 0)
            {
                EndBatchInternal();
            }
        }

        public void Undo()
        {
            if (IsInBatch)
            {
                throw new InvalidOperationException("Can't call Undo() during batch recording.");
            }

            if (IsInPaused)
            {
                throw new InvalidOperationException("Can't call Undo() during in paused.");
            }

            if (CanUndo is false)
            {
                return;
            }

            var currentFlags = CanUndoRedoClear;
            var currentUndoRedoCount = UndoRedoCount;
            var currentDepth = PauseBatchDepth;

            var action = m_undoStack.Pop();

            try
            {
                IsInUndoing = true;
                action.Undo();
            }
            finally
            {
                IsInUndoing = false;
            }

            m_redoStack.Push(action);

            InvokePropertyChanged(currentFlags, currentUndoRedoCount, currentDepth);
        }

        public void Redo()
        {
            if (IsInBatch)
            {
                throw new InvalidOperationException("Can't call Redo() during batch recording.");
            }

            if (IsInPaused)
            {
                throw new InvalidOperationException("Can't call Redo() during in paused.");
            }

            if (CanRedo is false)
            {
                return;
            }

            var currentFlags = CanUndoRedoClear;
            var currentUndoRedoCount = UndoRedoCount;
            var currentDepth = PauseBatchDepth;

            var action = m_redoStack.Pop();

            try
            {
                IsInUndoing = true;
                action.Redo();
            }
            finally
            {
                IsInUndoing = false;
            }

            m_undoStack.Push(action);

            InvokePropertyChanged(currentFlags, currentUndoRedoCount, currentDepth);
        }

        public void Push(Action undo, Action redo)
        {
            if (IsInPaused)
            {
                return;
            }

            if (IsInBatch)
            {
                _ = m_batchHistory ?? throw new NullReferenceException();

                m_batchHistory.Push(undo, redo);
                return;
            }

            var currentFlags = CanUndoRedoClear;
            var currentUndoRedoCount = UndoRedoCount;
            var currentDepth = PauseBatchDepth;

            m_undoStack.Push(new HistoryAction(undo, redo));

            if (m_redoStack.Count > 0)
            {
                m_redoStack.Clear();
            }

            InvokePropertyChanged(currentFlags, currentUndoRedoCount, currentDepth);
        }

        public void Clear()
        {
            var currentFlags = CanUndoRedoClear;
            var currentUndoRedoCount = UndoRedoCount;
            var currentDepth = PauseBatchDepth;

            m_undoStack.Clear();
            m_redoStack.Clear();

            InvokePropertyChanged(currentFlags, currentUndoRedoCount, currentDepth);
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

    private record struct HistoryAction(Action Undo, Action Redo);
    private readonly Stack<HistoryAction> _undoStack = new();
    private readonly Stack<HistoryAction> _redoStack = new();

    private static readonly PropertyChangedEventArgs CanUndoArgs = new(nameof(CanUndo));
    private static readonly PropertyChangedEventArgs CanRedoArgs = new(nameof(CanRedo));
    private static readonly PropertyChangedEventArgs CanClearArgs = new(nameof(CanClear));
    private static readonly PropertyChangedEventArgs UndoCountArgs = new(nameof(UndoCount));
    private static readonly PropertyChangedEventArgs RedoCountArgs = new(nameof(RedoCount));
    private static readonly PropertyChangedEventArgs PauseDepthArgs = new(nameof(PauseDepth));
    private static readonly PropertyChangedEventArgs BatchDepthArgs = new(nameof(BatchDepth));
}
