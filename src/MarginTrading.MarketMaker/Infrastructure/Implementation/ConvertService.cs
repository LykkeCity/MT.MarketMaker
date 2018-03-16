using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AutoMapper;
using JetBrains.Annotations;

namespace MarginTrading.MarketMaker.Infrastructure.Implementation
{
    [UsedImplicitly]
    internal class ConvertService : IConvertService
    {
        private readonly IMapper _mapper = CreateMapper();

        private static IMapper CreateMapper()
        {
            return new MapperConfiguration(cfg =>
            {
                cfg.CreateMap(typeof(ImmutableSortedDictionary<,>), typeof(ImmutableSortedDictionary<,>))
                    .ConvertUsing(typeof(ImmutableSortedDictionaryConverter<,,,>));
                cfg.CreateMap(typeof(ImmutableSortedSet<>), typeof(ImmutableSortedSet<>))
                    .ConvertUsing(typeof(ImmutableSortedSetConverter<,>));
            }).CreateMapper();
        }

        public TResult Convert<TSource, TResult>(TSource source,
            Action<IMappingOperationOptions<TSource, TResult>> opts)
        {
            return _mapper.Map(source, opts);
        }

        public TResult Convert<TSource, TResult>(TSource source)
        {
            return _mapper.Map<TSource, TResult>(source);
        }

        public class ImmutableSortedDictionaryConverter<TKey1, TValue1, TKey2, TValue2>
            : ITypeConverter<ImmutableSortedDictionary<TKey1, TValue1>, ImmutableSortedDictionary<TKey2, TValue2>>
        {
            public ImmutableSortedDictionary<TKey2, TValue2> Convert(ImmutableSortedDictionary<TKey1, TValue1> source,
                ImmutableSortedDictionary<TKey2, TValue2> destination, ResolutionContext context)
            {
                return source.Select(p => new KeyValuePair<TKey2, TValue2>(context.Mapper.Map<TKey1, TKey2>(p.Key),
                    context.Mapper.Map<TValue1, TValue2>(p.Value))).ToImmutableSortedDictionary();
            }
        }

        public class ImmutableSortedSetConverter<TValue1, TValue2>
            : ITypeConverter<ImmutableSortedSet<TValue1>, ImmutableSortedSet<TValue2>>
        {
            public ImmutableSortedSet<TValue2> Convert(ImmutableSortedSet<TValue1> source,
                ImmutableSortedSet<TValue2> destination, ResolutionContext context)
            {
                return source.Select(v => context.Mapper.Map<TValue1, TValue2>(v)).ToImmutableSortedSet();
            }
        }
    }
}