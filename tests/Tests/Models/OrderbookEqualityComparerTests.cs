using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using FluentAssertions;
using MarginTrading.MarketMaker.Models;
using NUnit.Framework;

namespace Tests.Models
{
    public class OrderbookEqualityComparerTests
    {
        public static IEnumerable<TestCaseData> GetTestCases()
        {
            yield return new TestCaseData("1", "1", new[] { new OrderbookPosition[0] }, new[] { new OrderbookPosition[0] }).Returns(true);
            yield return new TestCaseData("1", "1", new[] {new []{new OrderbookPosition(1, 1)}}, new[] { new[] { new OrderbookPosition(1, 1) } }).Returns(true);
            yield return new TestCaseData("1", "1", new[] {new []{new OrderbookPosition(1, 1), new OrderbookPosition(1, 1) } }, new[] { new[] { new OrderbookPosition(1, 1) } }).Returns(false);
            yield return new TestCaseData("1", "1", new[] {new []{new OrderbookPosition(1, 2)}}, new[] { new[] { new OrderbookPosition(1, 1) } }).Returns(false);
            yield return new TestCaseData("1", "2", new[] { new OrderbookPosition[0] }, new[] { new OrderbookPosition[0] }).Returns(false);
        }

        [TestCase("1","2", ExpectedResult = false)]
        [TestCase("1","1", ExpectedResult = true)]
        public bool ArraysEqual_ShouldReturnValidResult(string id1, string id2)
        {
            //arrange
            var orderbook1 = new Orderbook(id1, ImmutableArray<OrderbookPosition>.Empty, ImmutableArray<OrderbookPosition>.Empty);
            var orderbook2 = new Orderbook(id2, ImmutableArray<OrderbookPosition>.Empty, ImmutableArray<OrderbookPosition>.Empty);

            //act
            return Orderbook.Comparer.Equals(orderbook1, orderbook2);
        }

        [TestCase(new double[] { 1,2, 1,1, 1,1, 1,1, }, ExpectedResult = false)]
        [TestCase(new double[] { 1,1, 2,1, 1,1, 1,1, }, ExpectedResult = false)]
        [TestCase(new double[] { 1,1, 1,1, 2,1, 1,1, }, ExpectedResult = false)]
        [TestCase(new double[] { 1,1, 1,1, 1,1, 1,2, }, ExpectedResult = false)]
        [TestCase(new double[] { 1,1, 1,1, 1,1, 1,1, }, ExpectedResult = true)]
        public bool IdsEqual_ShouldReturnValidResult(double[] doubles)
        {
            //arrange
            var arr = doubles.Select(d => (decimal) d).ToArray();
            int i = 0;
            var orderbook1 = new Orderbook("1", ImmutableArray.Create(new OrderbookPosition(arr[i++], arr[i++])), ImmutableArray.Create(new OrderbookPosition(arr[i++], arr[i++])));
            var orderbook2 = new Orderbook("1", ImmutableArray.Create(new OrderbookPosition(arr[i++], arr[i++])), ImmutableArray.Create(new OrderbookPosition(arr[i++], arr[i++])));

            //act
            return Orderbook.Comparer.Equals(orderbook1, orderbook2);
        }

        [TestCase(ExpectedResult = false)]
        public bool ArraysNonEqualLength_ShouldReturnValidResult()
        {
            //arrange
            var arr = new decimal[] { 1,1, 1,1, 1,1, 1,1, 1,1, };
            int i = 0;
            var orderbook1 = new Orderbook("1", ImmutableArray.Create(new OrderbookPosition(arr[i++], arr[i++])), ImmutableArray.Create(new OrderbookPosition(arr[i++], arr[i++])));
            var orderbook2 = new Orderbook("1", ImmutableArray.Create(new OrderbookPosition(arr[i++], arr[i++])), ImmutableArray.Create(new OrderbookPosition(arr[i++], arr[i++]), new OrderbookPosition(arr[i++], arr[i++])));

            //act
            return Orderbook.Comparer.Equals(orderbook1, orderbook2);
        }
    }
}
