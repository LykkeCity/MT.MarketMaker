using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;

namespace MarginTrading.MarketMaker.Infrastructure.Implementation
{
    internal static class EnumerableExtensions
    {
        public static IEnumerable<(TSource Old, TSource New)> FindChanges<TSource, TKey, TElem>(
            this IEnumerable<TSource> oldSource,
            IEnumerable<TSource> newSource,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElem> elemSelector,
            Func<TElem, TElem, bool> elemsComparer,
            params ChangeType[] changesTypeFilter)
        {
            // todo test
            return oldSource.FullJoin(newSource,
                    keySelector,
                    oldEx => (Old: oldEx, New: default, ChangeType: ChangeType.Delete),
                    newEx => (Old: default, New: newEx, ChangeType: ChangeType.Add),
                    (oldEx, newEx) => (Old: oldEx, New: newEx, ChangeType: ChangeType.Update))
                .Where(t => t.ChangeType != ChangeType.Update ||
                            !elemsComparer(elemSelector(t.Old), elemSelector(t.New)))
                .Where(t => changesTypeFilter.Contains(t.ChangeType))
                .Select(t => (t.Old, t.New));
        }
    }

    internal enum ChangeType
    {
        Add,
        Delete,
        Update,
    }
}