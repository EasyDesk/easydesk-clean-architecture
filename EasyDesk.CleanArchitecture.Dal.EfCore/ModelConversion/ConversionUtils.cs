using EasyDesk.Tools.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.ModelConversion
{
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
            var dstByKey = dst.ToDictionary(dstKey);
            foreach (var s in src)
            {
                dstByKey.GetOption(srcKey(s)).Match(
                    some: m1 => converter.ApplyChanges(s, m1),
                    none: () => dst.Add(converter.ToPersistence(s)));
            }

            var srcKeys = src.Select(srcKey).ToHashSet();
            dst.RemoveWhere(d => !srcKeys.Contains(dstKey(d)));
        }
    }
}
