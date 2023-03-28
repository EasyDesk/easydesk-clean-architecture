using System.Collections;
using System.Collections.Immutable;

namespace EasyDesk.Commons.Collections.Immutable;

public class EquatableImmutableList<T> : IImmutableList<T>
{
    private readonly IImmutableList<T> _list;

    private EquatableImmutableList(IImmutableList<T> list)
    {
        _list = list;
    }

    public static EquatableImmutableList<T> FromList(ImmutableList<T> list) => new(list);

    private IImmutableList<T> Wrap(IImmutableList<T> list) => new EquatableImmutableList<T>(list);

    public IImmutableList<T> Add(T value) => Wrap(_list.Add(value));

    public IImmutableList<T> AddRange(IEnumerable<T> items) => Wrap(_list.AddRange(items));

    public IImmutableList<T> Clear() => Wrap(_list.Clear());

    public int IndexOf(T item, int index, int count, IEqualityComparer<T>? equalityComparer) =>
        _list.IndexOf(item, index, count, equalityComparer);

    public IImmutableList<T> Insert(int index, T element) => Wrap(_list.Insert(index, element));

    public IImmutableList<T> InsertRange(int index, IEnumerable<T> items) => Wrap(_list.InsertRange(index, items));

    public int LastIndexOf(T item, int index, int count, IEqualityComparer<T>? equalityComparer) =>
        _list.LastIndexOf(item, index, count, equalityComparer);

    public IImmutableList<T> Remove(T value, IEqualityComparer<T>? equalityComparer) =>
        Wrap(_list.Remove(value, equalityComparer));

    public IImmutableList<T> RemoveAll(Predicate<T> match) => Wrap(_list.RemoveAll(match));

    public IImmutableList<T> RemoveAt(int index) => Wrap(_list.RemoveAt(index));

    public IImmutableList<T> RemoveRange(IEnumerable<T> items, IEqualityComparer<T>? equalityComparer) =>
        Wrap(_list.RemoveRange(items, equalityComparer));

    public IImmutableList<T> RemoveRange(int index, int count) =>
        Wrap(_list.RemoveRange(index, count));

    public IImmutableList<T> Replace(T oldValue, T newValue, IEqualityComparer<T>? equalityComparer) =>
        Wrap(_list.Replace(oldValue, newValue, equalityComparer));

    public IImmutableList<T> SetItem(int index, T value) => Wrap(_list.SetItem(index, value));

    public T this[int index] => _list[index];

    public int Count => _list.Count;

    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || (
            obj is IImmutableList<T> list &&
            this.SequenceEqual(list));
    }

    public override int GetHashCode() => _list
        .Select(x => x?.GetHashCode() ?? 0)
        .FoldLeft(1, (current, v) => current * 31 + v);

    public override string ToString() => _list.ToListString();
}
