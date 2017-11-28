using System;
using System.Linq.Expressions;
using AutoMapper;

namespace MarginTrading.MarketMaker.Infrastructure
{
    public interface IConvertService
    {
        TResult Convert<TSource, TResult>(TSource source, Action<IMappingOperationOptions<TSource, TResult>> opts);
        TResult Convert<TSource, TResult>(TSource source);
    }
}