using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Moq;
using Moq.Language.Flow;

namespace Tests
{
    public class TestSuit<TSut> : TestSuit where TSut : class
    {
        private Lazy<TSut> _sut;
        public TSut Sut => _sut.Value;

        public TestSuit()
        {
            _sut = CreateLazy();
        }

        public TestSuit<TSut> Setup<TMocked>(Expression<Func<TMocked, bool>> setup)
            where TMocked : class
        {
            SetupCore(setup);
            return this;
        }

        public TestSuit<TSut> Setup<TMocked>(params Action<Mock<TMocked>>[] setups) where TMocked : class
        {
            SetupCore(setups);
            return this;
        }

        public override void Reset()
        {
            _sut = CreateLazy();
            base.Reset();
        }

        private Lazy<TSut> CreateLazy()
        {
            return new Lazy<TSut>(() => Cache<TSut>.CreateInstance.Value.Invoke(this));
        }
    }

    public abstract class TestSuit : TestSuitBase
    {
        public static TestSuit<TSut> Create<TSut>() where TSut : class
        {
            return new TestSuit<TSut>();
        }
    }

    public abstract class TestSuitBase
    {
        protected readonly ConcurrentDictionary<Type, Mock> Mocks = new ConcurrentDictionary<Type, Mock>();

        public Mock<TMocked> GetMock<TMocked>() where TMocked : class
        {
            return (Mock<TMocked>) Mocks.GetOrAdd(typeof(TMocked), t => Cache<TMocked>.CreateMock.Value());
        }

        public TMocked GetMockObj<TMocked>() where TMocked : class
        {
            return GetMock<TMocked>().Object;
        }
        
        public virtual void Reset()
        {
            foreach (var p in Mocks)
                p.Value.Reset();
        }
        
        protected Mock<TMocked> SetupCore<TMocked>(Expression<Func<TMocked, bool>> setup) where TMocked : class
        {
            var mock = GetMock<TMocked>();
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed - it has side effects
            TestSuitQueryable<TMocked>.CreateMockQuery(mock).First(setup);
            return mock;
        }

        protected Mock<TMocked> SetupCore<TMocked>(IEnumerable<Action<Mock<TMocked>>> setups) where TMocked : class
        {
            var mock = GetMock<TMocked>();
            foreach (var setup in setups)
                setup(mock);
            return mock;
        }

        #region Implementation details (aka "don't even try to understand")

        protected class Cache<T> where T : class
        {
            public static readonly Lazy<Func<TestSuit, T>> CreateInstance =
                new Lazy<Func<TestSuit, T>>(BuildCreateInstance);

            public static readonly Lazy<Func<Mock<T>>> CreateMock = new Lazy<Func<Mock<T>>>(() => BuildCreateMock());

            private static Func<TestSuit, T> BuildCreateInstance()
            {
                var suit = Expression.Parameter(typeof(TestSuit));
                var constructor = typeof(T).GetConstructors()
                    .Select(c => new {Info = c, Parameters = c.GetParameters()})
                    .OrderByDescending(c => c.Parameters.Length).First();

                var args = constructor.Parameters
                    .Select(p => Expression.Call(suit, "GetMockObj", new[] {p.ParameterType}))
                    .ToList();
                var ctor = Expression.New(constructor.Info, args);
                return Expression.Lambda<Func<TestSuit, T>>(ctor, suit).Compile();
            }

            private static Func<Mock<T>> BuildCreateMock()
            {
                var constructor = typeof(Mock<>).MakeGenericType(typeof(T))
                    .GetConstructor(new[] {typeof(MockBehavior)});
                var ctor = Expression.New(constructor, Expression.Constant(MockBehavior.Strict));
                return Expression.Lambda<Func<Mock<T>>>(ctor).Compile();
            }
        }

        protected static class TestSuitQueryable<T> where T : class
        {
            private static readonly Lazy<Func<MethodCallExpression, IQueryable<T>>> _queryableCreator =
                new Lazy<Func<MethodCallExpression, IQueryable<T>>>(BuildQueryableCreator);

            private static readonly MethodInfo _createQueryableMethod =
                typeof(TestSuitQueryable<T>).GetMethod(nameof(CreateQueryable),
                    BindingFlags.NonPublic | BindingFlags.Static);

            /// <summary>
            ///     Creates the mock query with the underlying queriable implementation.
            /// </summary>
            /// <param name="mock"></param>
            public static IQueryable<T> CreateMockQuery(Mock<T> mock)
            {
                var methodCallExpression = Expression.Call(null, _createQueryableMethod, Expression.Constant(mock));
                return _queryableCreator.Value.Invoke(methodCallExpression);
            }

            /// <summary>Wraps the enumerator inside a queryable.</summary>
            private static IQueryable<T> CreateQueryable(Mock<T> mock)
            {
                return CreateMocks(mock).AsQueryable();
            }

            /// <summary>
            ///     Method that is turned into the actual call from .Query{T}, to
            ///     transform the queryable query into a normal enumerable query.
            ///     This method is never used directly by consumers.
            /// </summary>
            /// <param name="mock"></param>
            private static IEnumerable<T> CreateMocks(Mock<T> mock)
            {
                while (true)
                {
                    yield return mock.Object;
                }
            }

            private static Func<MethodCallExpression, IQueryable<T>> BuildQueryableCreator()
            {
                var suit = Expression.Parameter(typeof(MethodCallExpression));
                var constructor = typeof(Mock).Assembly.GetType("Moq.Linq.MockQueryable`1")
                    .MakeGenericType(typeof(T))
                    .GetConstructor(new[] {typeof(MethodCallExpression)});
                var ctor = Expression.New(constructor, suit);
                return Expression.Lambda<Func<MethodCallExpression, IQueryable<T>>>(ctor, suit).Compile();
            }
        }

        #endregion
    }
}