using System;
using AutoMapper;
using JetBrains.Annotations;

namespace MarginTrading.MarketMaker.Infrastructure.Implementation
{
    [UsedImplicitly]
    internal class ConvertService : IConvertService
    {
        private readonly IMapper _mapper =
            new MapperConfiguration(cfg => cfg.CreateMissingTypeMaps = true).CreateMapper();

        public TResult Convert<TSource, TResult>(TSource source, Action<IMappingOperationOptions<TSource, TResult>> opts)
        {
            return _mapper.Map(source, opts);
        }

        public T Clone<T>(T source, Action<IMappingOperationOptions<T, T>> opts)
        {
            return _mapper.Map(source, opts);
        }

        public TResult Convert<TSource, TResult>(TSource source)
        {
            return _mapper.Map<TSource, TResult>(source);
        }
    }
}
