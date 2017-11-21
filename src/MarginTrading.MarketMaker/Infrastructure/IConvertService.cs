using System;
using AutoMapper;

namespace MarginTrading.MarketMaker.Infrastructure
{
    public interface IConvertService
    {
        TResult Convert<TSource, TResult>(TSource source, Action<IMappingOperationOptions<TSource, TResult>> opts);
        TResult Convert<TSource, TResult>(TSource source);
        T Clone<T>(T source, Action<IMappingOperationOptions<T, T>> opts);
    }
}