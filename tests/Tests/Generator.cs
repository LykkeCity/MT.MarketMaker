using System;
using System.Collections;
using System.Collections.Generic;

namespace Tests
{
    internal class Generator<T> : IEnumerable<T>
    {
        private readonly Func<T, int, T> _increment;
        private T _value;
        private int _counter;

        public Generator(Func<T, int, T> increment)
        {
            _increment = increment;
        }

        public T Next()
        {
            return _value = _increment(_value, _counter++);
        }

        public void Reset()
        {
            _value = default;
            _counter = default;
        }

        public Generator<T> Clone()
        {
            return new Generator<T>(_increment);
        }

        private IEnumerable<T> Func()
        {
            while (true)
            {
                yield return Next();
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return Func().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Func().GetEnumerator();
        }
    }

    internal static class Generate
    {
        public static Generator<T> FromLambda<T>(Func<T, int, T> increment)
        {
            return new Generator<T>(increment);
        }

        public static Generator<decimal> Decimals(decimal multiplier = 1)
        {
            return FromLambda<decimal>((o, i) => (i + 1) * multiplier);
        }

        public static Generator<int> Ints()
        {
            return FromLambda<int>((o, i) => i + 1);
        }

        public static Generator<T> Values<T>(params T[] values)
        {
            return FromLambda<T>((o, i) => values[i % values.Length]);
        }
    }
}