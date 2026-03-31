namespace SlugEditor.Test;

public sealed class TestScope : IDisposable
{
    private readonly List<IDisposable> _disposables = [];

    public T Track<T>(T disposable) where T : IDisposable
    {
        _disposables.Add(disposable);
        return disposable;
    }

    public void Dispose()
    {
        for (var i = _disposables.Count - 1; i >= 0; i--)
        {
            _disposables[i].Dispose();
        }

        _disposables.Clear();
    }
}
