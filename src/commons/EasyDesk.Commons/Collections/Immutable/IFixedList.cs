using EasyDesk.Commons.Options;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace EasyDesk.Commons.Collections.Immutable;

[CollectionBuilder(typeof(FixedList), nameof(FixedList.FromSpan))]
public interface IFixedList<T> : IReadOnlyCollection<T>, IEquatable<IFixedList<T>>
{
    T this[int index] { get; }

    IFixedList<T> Add(T value);

    IFixedList<T> AddRange(IEnumerable<T> items);

    IFixedList<T> Clear();

    Option<int> IndexOf(T item, IEqualityComparer<T>? equalityComparer = null);

    IFixedList<T> Insert(int index, T element);

    IFixedList<T> InsertRange(int index, IEnumerable<T> items);

    Option<int> LastIndexOf(T item, IEqualityComparer<T>? equalityComparer = null);

    IFixedList<T> Remove(T value, IEqualityComparer<T>? equalityComparer = null);

    IFixedList<T> RemoveAll(Predicate<T> match);

    IFixedList<T> RemoveAt(int index);

    IFixedList<T> RemoveRange(IEnumerable<T> items, IEqualityComparer<T>? equalityComparer = null);

    IFixedList<T> RemoveRange(int index, int count);

    IFixedList<T> Replace(T oldValue, T newValue, IEqualityComparer<T>? equalityComparer = null);

    IFixedList<T> SetItem(int index, T value);

    IImmutableList<T> AsImmutableList();
}
