﻿using EasyDesk.Commons.Options;
using EasyDesk.Commons.Tasks;

namespace EasyDesk.Commons.Collections;

public static class AsyncEnumerable
{
    public static IAsyncEnumerable<T> Create<T>(Func<CancellationToken, IAsyncEnumerator<T>> enumeratorProvider) =>
        new AsyncEnumerableImpl<T>(enumeratorProvider);

    private readonly struct AsyncEnumerableImpl<T> : IAsyncEnumerable<T>
    {
        private readonly Func<CancellationToken, IAsyncEnumerator<T>> _enumeratorProvider;

        public AsyncEnumerableImpl(Func<CancellationToken, IAsyncEnumerator<T>> enumeratorProvider)
        {
            _enumeratorProvider = enumeratorProvider;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
            _enumeratorProvider(cancellationToken);
    }

    public static IAsyncEnumerable<T> Empty<T>() => EmptyAsyncEnumerable<T>.Instance;

    private class EmptyAsyncEnumerable<T> : IAsyncEnumerable<T>
    {
        public static EmptyAsyncEnumerable<T> Instance { get; } = new();

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
            default(EmptyAsyncEnumerator<T>);
    }

    private readonly struct EmptyAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(false);

        public T Current => throw new InvalidOperationException("Cannot access current element of an empty async enumerator");

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    public static IAsyncEnumerable<T> Of<T>(params T[] items) => items.ToAsyncEnumerable();

    public static async Task ForEach<T>(this IAsyncEnumerable<T> sequence, AsyncAction<T> action)
    {
        await foreach (var item in sequence)
        {
            await action(item);
        }
    }

    public static async Task ForEach<T>(this IAsyncEnumerable<T> sequence, Action<T> action)
    {
        await foreach (var item in sequence)
        {
            action(item);
        }
    }

    public static async Task<bool> Any<T>(this IAsyncEnumerable<T> sequence)
    {
        await using (var enumerator = sequence.GetAsyncEnumerator())
        {
            return await enumerator.MoveNextAsync();
        }
    }

    public static IAsyncEnumerable<T> Concat<T>(this IAsyncEnumerable<T> left, IAsyncEnumerable<T> right) =>
        left.ThenConcat(() => right);

    public static async IAsyncEnumerable<T> ThenConcat<T>(this IAsyncEnumerable<T> left, Func<IAsyncEnumerable<T>> right)
    {
        await foreach (var item in left)
        {
            yield return item;
        }

        await foreach (var item in right())
        {
            yield return item;
        }
    }

    public static async Task<Option<T>> FirstOption<T>(this IAsyncEnumerable<T> sequence)
    {
        await using (var enumerator = sequence.GetAsyncEnumerator())
        {
            if (!await enumerator.MoveNextAsync())
            {
                return None;
            }
            return Some(enumerator.Current);
        }
    }

    public static async Task<Option<T>> FirstOption<T>(this IAsyncEnumerable<T> sequence, Func<T, bool> predicate) =>
        await sequence.Where(predicate).FirstOption();

    public static async IAsyncEnumerable<R> Select<T, R>(this IAsyncEnumerable<T> sequence, Func<T, R> mapper)
    {
        await foreach (var item in sequence)
        {
            yield return mapper(item);
        }
    }

    public static async IAsyncEnumerable<T> Where<T>(this IAsyncEnumerable<T> sequence, Func<T, bool> predicate)
    {
        await foreach (var item in sequence)
        {
            if (predicate(item))
            {
                yield return item;
            }
        }
    }

    public static async IAsyncEnumerable<R> SelectMany<T, R>(this IAsyncEnumerable<T> sequence, Func<T, IAsyncEnumerable<R>> mapper)
    {
        await foreach (var item in sequence)
        {
            await foreach (var mapped in mapper(item))
            {
                yield return mapped;
            }
        }
    }

    public static async Task<TResult> AggregateAsync<T, TResult>(
        this IAsyncEnumerable<T> sequence,
        TResult seed,
        Func<TResult, T, TResult> combine)
    {
        var current = seed;
        await foreach (var item in sequence)
        {
            current = combine(current, item);
        }
        return current;
    }

    public static async Task<IEnumerable<T>> ToEnumerableAsync<T>(this IAsyncEnumerable<T> sequence)
    {
        return await sequence.AggregateAsync(new List<T>(), (list, item) =>
        {
            list.Add(item);
            return list;
        });
    }

    public static IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> sequence) =>
        Create(_ => new AsyncEnumeratorFromEnumerator<T>(sequence.GetEnumerator()));

    private readonly struct AsyncEnumeratorFromEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _enumerator;

        public AsyncEnumeratorFromEnumerator(IEnumerator<T> enumerator)
        {
            _enumerator = enumerator;
        }

        public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(_enumerator.MoveNext());

        public T Current => _enumerator.Current;

        public ValueTask DisposeAsync()
        {
            _enumerator.Dispose();
            return ValueTask.CompletedTask;
        }
    }

    public static async Task<bool> SequenceEqualAsync<T>(
        this IAsyncEnumerable<T> sequence,
        IAsyncEnumerable<T> other,
        IEqualityComparer<T>? comparer = null)
    {
        await using var first = sequence.GetAsyncEnumerator();
        await using var second = other.GetAsyncEnumerator();

        while (await first.MoveNextAsync())
        {
            comparer ??= EqualityComparer<T>.Default;
            if (!(await second.MoveNextAsync() && comparer.Equals(first.Current, second.Current)))
            {
                return false;
            }
        }

        return !await second.MoveNextAsync();
    }
}
