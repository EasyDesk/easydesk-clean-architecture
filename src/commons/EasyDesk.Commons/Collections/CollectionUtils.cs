namespace EasyDesk.Commons.Collections;

public static class CollectionUtils
{
    public static bool HasAny<T>(this ICollection<T> collection) => collection.Count != 0;

    public static bool IsEmpty<T>(this ICollection<T> collection) => collection.Count == 0;

    public static IEnumerable<T> RemoveWhere<T>(this ICollection<T> collection, Func<T, bool> predicate)
    {
        var itemsToRemove = collection.Where(predicate).ToList();
        itemsToRemove.ForEach(x => collection.Remove(x));
        return itemsToRemove;
    }

    public static void AddAll<T>(this ICollection<T> collection, IEnumerable<T> items) =>
        items.ForEach(collection.Add);

    public static void RemoveAll<T>(this ICollection<T> collection, IEnumerable<T> items) =>
        items.ForEach(x => collection.Remove(x));

    public static void AddTo<T>(this IEnumerable<T> items, ICollection<T> collection) =>
        collection.AddAll(items);

    public static void RemoveFrom<T>(this IEnumerable<T> items, ICollection<T> collection) =>
        collection.RemoveAll(items);

    public static string ToCollectionString<T>(this IEnumerable<T> sequence, string prefix = "[", string suffix = "]", Func<T, string>? formatter = null)
    {
        return sequence
            .Select(formatter ?? (x => x!.ToString()!))
            .ConcatStrings(", ", prefix, suffix);
    }

    public static string ToListString<T>(this IEnumerable<T> sequence, Func<T, string>? formatter = null) =>
        sequence.ToCollectionString(prefix: "[", suffix: "]", formatter);

    public static string ToSetString<T>(this IEnumerable<T> sequence, Func<T, string>? formatter = null) =>
        sequence.ToCollectionString(prefix: "{", suffix: "}", formatter);

    public static string ToTupleString<T>(this IEnumerable<T> sequence, Func<T, string>? formatter = null) =>
        sequence.ToCollectionString(prefix: "(", suffix: ")", formatter);
}
