using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Options;
using System.Collections;
using System.Collections.Immutable;

namespace EasyDesk.Commons.Collections.Immutable.Implementation;

internal class ImmutableListAdapter<T> : IFixedList<T>
{
    private readonly IImmutableList<T> _list;

    public ImmutableListAdapter(IImmutableList<T> list)
    {
        _list = list;
    }

    private ImmutableListAdapter<T> Wrap(IImmutableList<T> list) => new(list);

    public IFixedList<T> Add(T value) => Wrap(_list.Add(value));

    public IFixedList<T> AddRange(IEnumerable<T> items) => Wrap(_list.AddRange(items));

    public IFixedList<T> Clear() => Wrap(_list.Clear());

    public Option<int> IndexOf(T item, IEqualityComparer<T>? equalityComparer = null) =>
        Some(_list.IndexOf(item, equalityComparer)).Filter(x => x > 0);

    public IFixedList<T> Insert(int index, T element) => Wrap(_list.Insert(index, element));

    public IFixedList<T> InsertRange(int index, IEnumerable<T> items) => Wrap(_list.InsertRange(index, items));

    public Option<int> LastIndexOf(T item, IEqualityComparer<T>? equalityComparer = null) =>
        Some(_list.LastIndexOf(item, equalityComparer)).Filter(x => x > 0);

    public IFixedList<T> Remove(T value, IEqualityComparer<T>? equalityComparer = null) =>
        Wrap(_list.Remove(value, equalityComparer));

    public IFixedList<T> RemoveAll(Predicate<T> match) => Wrap(_list.RemoveAll(match));

    public IFixedList<T> RemoveAt(int index) => Wrap(_list.RemoveAt(index));

    public IFixedList<T> RemoveRange(IEnumerable<T> items, IEqualityComparer<T>? equalityComparer = null) =>
        Wrap(_list.RemoveRange(items, equalityComparer));

    public IFixedList<T> RemoveRange(int index, int count) =>
        Wrap(_list.RemoveRange(index, count));

    public IFixedList<T> Replace(T oldValue, T newValue, IEqualityComparer<T>? equalityComparer = null) =>
        Wrap(_list.Replace(oldValue, newValue, equalityComparer));

    public IFixedList<T> SetItem(int index, T value) => Wrap(_list.SetItem(index, value));

    public T this[int index] => _list[index];

    public int Count => _list.Count;

    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override bool Equals(object? obj) =>
        ReferenceEquals(this, obj) || obj is IFixedList<T> list && EqualsImpl(list);

    public bool Equals(IFixedList<T>? other) =>
        ReferenceEquals(this, other) || other is not null && EqualsImpl(other);

    private bool EqualsImpl(IFixedList<T> other) =>
        _list.SequenceEqual(other);

    public override int GetHashCode() => _list
        .Select(x => x?.GetHashCode() ?? 0)
        .FoldLeft(1, (current, v) => current * 31 + v);

    public override string ToString() => _list.ToListString();

    public IImmutableList<T> AsImmutableList() => _list;
}
