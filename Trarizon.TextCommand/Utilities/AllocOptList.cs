using System.Diagnostics;

namespace Trarizon.TextCommand.Utilities;
internal struct AllocOptList<T>
{
    private T[] _items;
    private int _size;

    public AllocOptList()
    {
        _items = [];
    }

    public AllocOptList(int capacity)
    {
        _items = new T[capacity];
    }

    public readonly int Count => _size;

    #region Accessors

    public readonly T this[int index]
    {
        get => _items[index];
        set => _items[index] = value;
    }

    public readonly Span<T> AsSpan() => _items.AsSpan(0, _size);

    #endregion

    #region Builders

    public void Add(T item)
    {
        Debug.Assert(_size <= _items.Length);

        if (_size == _items.Length) {
            Grow(_size + 1);
        }
        _items[_size++] = item;
    }

    private void Grow(int expectedCapacity)
    {
        T[] newItems = new T[GetNewCapacity(expectedCapacity)];
        Array.Copy(_items, newItems, _size);
        _items = newItems;
    }

    private readonly int GetNewCapacity(int expectedCapacity)
    {
        Debug.Assert(_items.Length < expectedCapacity);

        int newCapacity;
        if (_items.Length == 0)
            newCapacity = 4;
        else
            newCapacity = int.Min(_items.Length * 2, Array.MaxLength);

        if (newCapacity < expectedCapacity)
            newCapacity = expectedCapacity;

        return int.Max(newCapacity, expectedCapacity);
    }

    #endregion
}
