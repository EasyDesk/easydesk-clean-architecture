﻿using EasyDesk.CleanArchitecture.Dal.EfCore.Interfaces;
using EasyDesk.Commons.Collections;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Utils;

public static class CollectionDiff
{
    public static void ApplyChangesToCollection<D, P, K>(
        IEnumerable<D> src,
        Func<D, K> srcKey,
        ICollection<P> dst,
        Func<P, K> dstKey)
        where P : IEntityPersistence<D, P>
        where K : notnull =>
        ApplyChangesToCollection(src, srcKey, dst, dstKey, P.ToPersistence, (p, d) => p.ApplyChanges(d));

    public static void ApplyChangesToCollection<D, P, K>(
        IEnumerable<D> src,
        Func<D, K> srcKey,
        ICollection<P> dst,
        Func<P, K> dstKey,
        Func<D, P> toPersistence,
        Action<P, D> applyChanges)
        where K : notnull
    {
        var dstByKey = dst.ToDictionary(dstKey);
        foreach (var s in src)
        {
            dstByKey.GetOption(srcKey(s)).Match(
                some: m1 => applyChanges(m1, s),
                none: () => dst.Add(toPersistence(s)));
        }

        var srcKeys = src.Select(srcKey).ToHashSet();
        dst.RemoveWhere(d => !srcKeys.Contains(dstKey(d)));
    }
}
