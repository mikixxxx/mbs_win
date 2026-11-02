namespace mbs;

// Small observable playlist, retaining the original program's event style.
public sealed class MyList<T> : List<T>
{
    public event EventHandler? OnAdd;
    public event EventHandler? OnRemove;

    public new void Add(T item)
    {
        base.Add(item);
        OnAdd?.Invoke(this, EventArgs.Empty);
    }

    public new void AddRange(IEnumerable<T> items)
    {
        var startCount = Count;
        base.AddRange(items);
        if (Count > startCount) OnAdd?.Invoke(this, EventArgs.Empty);
    }

    public new bool Remove(T item)
    {
        if (!base.Remove(item)) return false;
        OnRemove?.Invoke(this, EventArgs.Empty);
        return true;
    }
}
