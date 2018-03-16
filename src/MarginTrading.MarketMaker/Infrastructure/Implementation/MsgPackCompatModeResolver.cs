using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;

namespace MarginTrading.MarketMaker.Infrastructure.Implementation
{
    /// <summary>
    /// Same as <see cref="TypelessContractlessStandardResolver"/> but with <see cref="OldSpecResolver"/> &amp; <see cref="DynamicEnumAsStringResolver"/>
    /// </summary>
    public sealed class MsgPackCompatModeResolver : IFormatterResolver
    {
        public static readonly IFormatterResolver Instance = new MsgPackCompatModeResolver();

        private static readonly IFormatterResolver[] resolvers = {
            OldSpecResolver.Instance,
            NativeDateTimeResolver.Instance,
            BuiltinResolver.Instance,
            AttributeFormatterResolver.Instance,
            DynamicEnumAsStringResolver.Instance,
            DynamicGenericResolver.Instance,
            DynamicUnionResolver.Instance,
            DynamicObjectResolver.Instance,
            DynamicContractlessObjectResolver.Instance,
            TypelessObjectResolver.Instance
        };

        private MsgPackCompatModeResolver()
        {
        }

        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.formatter;
        }

        private static class FormatterCache<T>
        {
            public static readonly IMessagePackFormatter<T> formatter;

            static FormatterCache()
            {
                foreach (var resolver in resolvers)
                {
                    var formatter = resolver.GetFormatter<T>();
                    if (formatter != null)
                    {
                        FormatterCache<T>.formatter = formatter;
                        break;
                    }
                }
            }
        }
    }
}