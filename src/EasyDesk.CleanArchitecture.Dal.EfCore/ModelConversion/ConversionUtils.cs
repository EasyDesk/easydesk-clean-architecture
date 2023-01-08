using EasyDesk.Tools.Collections;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.ModelConversion;

public static class ConversionUtils
{
    public static void ApplyChangesToCollection<D, P, K>(
        IEnumerable<D> src,
        Func<D, K> srcKey,
        ICollection<P> dst,
        Func<P, K> dstKey)
        where P : IPersistenceModel<D, P>
        where K : IEquatable<K>
    {
        var dstByKey = dst.ToDictionary(dstKey);
        foreach (var s in src)
        {
            dstByKey.GetOption(srcKey(s)).Match(
                some: m1 => P.ApplyChanges(s, m1),
                none: () => dst.Add(s.ToPersistence<D, P>()));
        }

        var srcKeys = src.Select(srcKey).ToHashSet();
        dst.RemoveWhere(d => !srcKeys.Contains(dstKey(d)));
    }
}
