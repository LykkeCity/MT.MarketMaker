using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;

namespace MarginTrading.MarketMaker.Infrastructure.Implemetation
{
    internal static class EnumerableExtensions
    {
        public static IEnumerable<(TSource Old, TSource New)> FindChanges<TSource, TKey, TElem>(
            this IEnumerable<TSource> oldSource,
            IEnumerable<TSource> newSource,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElem> elemSelector,
            Func<TElem, TElem, bool> elemsComparer)
            where TSource : class
        {
            return oldSource.FullJoin(newSource,
                    keySelector,
                    oldEx => (Old: oldEx, New: null),
                    newEx => (Old: null, New: newEx),
                    (oldEx, newEx) => (Old: oldEx, New: newEx))
                .Where(t => t.Old == null || t.New == null || !elemsComparer(elemSelector(t.Old), elemSelector(t.New)));
        }
    }
}