using EasyDesk.Tools.Collections;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.ModelConversion;

public static class ConversionUtils
{
    public static void ApplyChangesToCollection<D, P, K>(
        IEnumerable<D> src,
        Func<D, K> srcKey,
        ICollection<P> dst,
        Func<P, K> dstKey,
        IModelConverter<D, P> converter)
        where P : class, new()
        where K : IEquatable<K>
    {
        ApplyChangesToCollection(src, srcKey, dst, dstKey, converter.ApplyChanges);
    }

    public static void ApplyChangesToCollection<D, P, K>(
        IEnumerable<D> src,
        Func<D, K> srcKey,
        ICollection<P> dst,
        Func<P, K> dstKey,
        Action<D, P> applyChanges)
        where P : class, new()
        where K : IEquatable<K>
    {
        var dstByKey = dst.ToDictionary(dstKey);
        foreach (var s in src)
        {
            dstByKey.GetOption(srcKey(s)).Match(
                some: m1 => applyChanges(s, m1),
                none: () =>
                {
                    var p = new P();
                    applyChanges(s, p);
                    dst.Add(p);
                });
        }

        var srcKeys = src.Select(srcKey).ToHashSet();
        dst.RemoveWhere(d => !srcKeys.Contains(dstKey(d)));
    }
}
