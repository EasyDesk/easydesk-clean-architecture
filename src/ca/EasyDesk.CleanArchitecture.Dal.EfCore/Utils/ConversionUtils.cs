using EasyDesk.CleanArchitecture.Dal.EfCore.Abstractions;
using EasyDesk.CleanArchitecture.Dal.EfCore.Interfaces.Abstractions;
using EasyDesk.Commons.Collections;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Utils;

public static class ConversionUtils
{
    public static void ApplyChangesToCollection<D, P, K>(
        IEnumerable<D> src,
        Func<D, K> srcKey,
        ICollection<P> dst,
        Func<P, K> dstKey)
        where P : IEntityPersistence<D, P>
        where K : IEquatable<K> =>
        ApplyChangesToCollection(src, srcKey, dst, dstKey, P.ToPersistence);

    public static void ApplyChangesToCollection<D, P, K>(
        IEnumerable<D> src,
        Func<D, K> srcKey,
        ICollection<P> dst,
        Func<P, K> dstKey,
        Func<D, P> toPersistence)
        where P : IMutablePersistence<D, P>
        where K : IEquatable<K>
    {
        var dstByKey = dst.ToDictionary(dstKey);
        foreach (var s in src)
        {
            dstByKey.GetOption(srcKey(s)).Match(
                some: m1 => P.ApplyChanges(s, m1),
                none: () => dst.Add(toPersistence(s)));
        }

        var srcKeys = src.Select(srcKey).ToHashSet();
        dst.RemoveWhere(d => !srcKeys.Contains(dstKey(d)));
    }
}
