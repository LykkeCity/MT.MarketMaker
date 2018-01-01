using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Common;
using MarginTrading.MarketMaker.Models;
using MarginTrading.MarketMaker.Services.ExtPrices;
using MarginTrading.MarketMaker.Services.ExtPrices.Implementation;
using NUnit.Framework;

namespace Tests.Services
{
    public class OutliersOrderbooksServiceTests
    {
        private static readonly TestSuit<OutliersOrderbooksService> _testSuit =
            TestSuit.Create<OutliersOrderbooksService>();

        [TearDown]
        public void TearDown()
        {
            _testSuit.Reset();
        }

        private static IEnumerable<TestCaseData> GetCasesForPrices(decimal[] prices,
            params (decimal Threshold, int[] Result)[] others)
        {
            foreach (var (threshold, result) in others)
                yield return new TestCaseData(prices, threshold).Returns(result);
        }

        public static IEnumerable<TestCaseData> GetCases()
        {
            return new[]
            {
                // last exchange encompassing all, odd exchanges count
                GetCasesForPrices(new decimal[] {100, 101, 102, 103, 100, 103, 99, 101, 70, 110},
                    (0.02m, new[] {2, 5}),
                    (0.03m, new[] {5}),
                    (0.3m, new[] {5}),
                    (0.301m, new int[0])),

                GetCasesForPrices(new decimal[] {100, 101, 102, 103, 100, 103, 99, 101, 110, 111},
                    (0.019m, new[] {1, 2, 4, 5}),
                    (0.02m, new[] {2, 5}),
                    (0.021m, new[] {5}),
                    (0.10m, new[] {5}),
                    (0.101m, new int[0])),

                GetCasesForPrices(new decimal[] {100, 101, 102, 103, 100, 103, 99, 101, 50, 51},
                    (0.50m, new[] {5}),
                    (0.501m, new int[0])),

                GetCasesForPrices(new decimal[] {150, 152, 100, 103, 149, 163, 111, 116, 89, 99},
                    (0.11m, new[] {1, 2, 3, 5}),
                    (0.12m, new[] {1, 3, 5}),
                    (0.19m, new[] {1, 3, 5}),
                    (0.2m, new[] {1, 3}),
                    (0.35m, new[] {1, 3}),
                    (0.36m, new[] {3}),
                    (0.4m, new[] {3}),
                    (0.41m, new int[0])),

                GetCasesForPrices(new decimal[] {100, 200, 110, 170, 140, 160, 100, 110, 107, 109},
                    (0.175m, new[] {1, 3, 4, 5}),
                    (0.18m, new[] {3, 4, 5}),
                    (0.27m, new[] {3, 4, 5}),
                    (0.28m, new[] {4, 5}),
                    (0.35m, new[] {4, 5}),
                    (0.355m, new[] {5}),
                    (0.36m, new int[0])),

                GetCasesForPrices(new[] {150, 150, 152, 151.95m, 146, 151, 150, 157, 155, 170},
                    (0.012m, new[] {1, 3, 4, 5}),
                    (0.014m, new[] {3, 4, 5}),
                    (0.032m, new[] {3, 4, 5}),
                    (0.034m, new[] {3, 5}),
                    (0.038m, new[] {3, 5}),
                    (0.04m, new[] {5}),
                    (0.115m, new[] {5}),
                    (0.12m, new int[0])),
            }.SelectMany(c => c);
        }

        [TestCaseSource(nameof(GetCases))]
        public IEnumerable<int> Always_ShouldReturnValidResults(decimal[] prices, decimal threshold)
        {
            //arrange
            var validOrderbooks = prices.ToChunks(2).Select((p, i) =>
            {
                var exchangeName = (i + 1).ToString();
                var orderbook = CreateOrderbook(exchangeName);
                _testSuit.Setup<IBestPricesService>(m =>
                    m.Setup(s => s.Calc(orderbook)).Returns(new BestPrices(p.First(), p.Last())));
                return KeyValuePair.Create(exchangeName, orderbook);
            }).ToImmutableDictionary();

            _testSuit.Setup<IExtPricesSettingsService>(s => s.GetOutlierThreshold("pair") == threshold);
            _testSuit.Setup<IPrimaryExchangeService>(s => s.GetLastPrimaryExchange("pair") == "ex1");

            //act
            var result = _testSuit.Sut.FindOutliers("pair", validOrderbooks);

            //assert
            return result.Select(r => int.Parse(r.ExchangeName)).ToArray();
        }

        private static ExternalOrderbook CreateOrderbook(string exchangeName)
        {
            return new ExternalOrderbook("pair", exchangeName, DateTime.Now,
                ImmutableArray.Create<OrderbookPosition>(), ImmutableArray.Create<OrderbookPosition>());
        }
    }
}