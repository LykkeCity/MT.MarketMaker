using System;
using JetBrains.Annotations;
using MarginTrading.MarketMaker.Infrastructure.Implementation;

namespace MarginTrading.MarketMaker.Models
{
    public class AssetPairInfo
    {
        public AssetPairInfo([NotNull] string id, int accuracy)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Accuracy = accuracy.RequiredNotLessThan(0, nameof(accuracy));
        }

        public string Id { get; }
        public int Accuracy { get; }
    }
}