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
                cfg.CreateMap(typeof(ImmutableDictionary<,>), typeof(ImmutableDictionary<,>))
                    .ConvertUsing(typeof(ImmutableDictionaryConverter<,,,>));
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

        public class ImmutableDictionaryConverter<TKey1, TValue1, TKey2, TValue2>
            : ITypeConverter<ImmutableDictionary<TKey1, TValue1>, ImmutableDictionary<TKey2, TValue2>>
        {
            public ImmutableDictionary<TKey2, TValue2> Convert(ImmutableDictionary<TKey1, TValue1> source,
                ImmutableDictionary<TKey2, TValue2> destination, ResolutionContext context)
            {
                return source.Select(p => new KeyValuePair<TKey2, TValue2>(context.Mapper.Map<TKey1, TKey2>(p.Key),
                    context.Mapper.Map<TValue1, TValue2>(p.Value))).ToImmutableDictionary();
            }
        }
    }
}