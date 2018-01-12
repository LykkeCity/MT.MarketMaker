using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Common;
using JetBrains.Annotations;

namespace MarginTrading.MarketMaker.Infrastructure.Implementation
{
    public static class ValidationExtensions
    {
        [NotNull]
        [ContractAnnotation("value:null => halt")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T RequiredNotNull<T>(this T value, string paramName, string message = null) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName, message);
            }

            return value;
        }

        [ContractAnnotation("value:null => halt")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T RequiredNotNull<T>(this T? value, string paramName, string message = null) where T : struct
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName, message);
            }

            return value.Value;
        }

        [NotNull]
        [ContractAnnotation("value:null => halt")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string RequiredNotNullOrWhiteSpace(this string value, string paramName, string message = null)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(paramName, message);
            }

            return value;
        }

        [NotNull]
        [ContractAnnotation("value:null => halt")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string RequiredNotNullOrEmpty(this string value, string paramName, string message = null)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(paramName, message);
            }

            return value;
        }

        [NotNull]
        [ContractAnnotation("values:null => halt")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ICollection<T> RequiredNotNullOrEmpty<T>(this ICollection<T> values, string paramName,
            string message = null)
        {
            if (values == null || values.Count == 0)
            {
                throw new ArgumentNullException(paramName, message);
            }

            return values;
        }

        [Pure]
        [NotNull]
        [ContractAnnotation("values:null => halt")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> RequiredNotNullOrEmpty<T>([CanBeNull] this IEnumerable<T> values, string paramName,
            string message = null)
        {
            if (values == null)
            {
                throw new ArgumentNullException(paramName, message);
            }

            var hasValues = false;
            foreach (var value in values)
            {
                yield return value;
                hasValues = true;
            }

            if (!hasValues)
            {
                throw new ArgumentException(message, paramName);
            }
        }

        [Pure]
        [NotNull]
        [ContractAnnotation("values:null => halt")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> RequiredNotNullElems<T>([CanBeNull] [ItemCanBeNull] this IEnumerable<T> values,
            string paramName, string message = null)
        {
            if (values == null)
            {
                throw new ArgumentNullException(paramName, message);
            }

            foreach (var value in values)
            {
                if (value == null)
                {
                    throw new ArgumentException(message, paramName);
                }

                yield return value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T RequiredEqualsTo<T>(this T value, T value2, string paramName, string message = null)
        {
            if (Equals(value, value2))
            {
                return value;
            }

            if (string.IsNullOrEmpty(message))
            {
                message = string.Format("{0} = {1} should be equals to {2}.", paramName, value, value2);
            }

            throw new ArgumentOutOfRangeException(paramName, message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T RequiredNotEqualsTo<T>(this T value, T value2, string paramName, string message = null)
        {
            if (!Equals(value, value2))
            {
                return value;
            }

            if (string.IsNullOrEmpty(message))
            {
                message = string.Format("{0} = {1} should not be equals to {2}.", paramName, value, value2);
            }

            throw new ArgumentOutOfRangeException(paramName, message);
        }

        [CanBeNull]
        [ContractAnnotation("value:null => null")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? RequiredLessThan<T>(this T? value, T maxValue, string paramName, string message = null)
            where T : struct, IComparable
        {
            if (value != null)
            {
                value.Value.RequiredLessThan(maxValue, paramName, message);
            }

            return value;
        }

        [CanBeNull]
        [ContractAnnotation("value:null => null")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? RequiredLessThan<T>(this T? value, T? maxValue, string paramName, string message = null)
            where T : struct, IComparable
        {
            if (value != null && maxValue != null)
            {
                value.Value.RequiredLessThan(maxValue.Value, paramName, message);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T RequiredLessThan<T>(this T value, T maxValue, string paramName, string message = null)
            where T : struct, IComparable
        {
            if (value.CompareTo(maxValue) < 0)
            {
                return value;
            }

            if (string.IsNullOrEmpty(message))
            {
                message = string.Format("{0} = {1} can not be greater or equals than {2}.", paramName, value, maxValue);
            }

            throw new ArgumentOutOfRangeException(paramName, message);
        }

        [CanBeNull]
        [ContractAnnotation("value:null => null")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? RequiredNotLessThan<T>(this T? value, T minValue, string paramName, string message = null)
            where T : struct, IComparable
        {
            if (value != null)
            {
                value.Value.RequiredNotLessThan(minValue, paramName, message);
            }

            return value;
        }

        [CanBeNull]
        [ContractAnnotation("value:null => null")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? RequiredNotLessThan<T>(this T? value, T? minValue, string paramName, string message = null)
            where T : struct, IComparable
        {
            if (value != null && minValue != null)
            {
                value.Value.RequiredNotLessThan(minValue.Value, paramName, message);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T RequiredNotLessThan<T>(this T value, T minValue, string paramName, string message = null)
            where T : struct, IComparable
        {
            if (value.CompareTo(minValue) >= 0)
            {
                return value;
            }

            if (string.IsNullOrEmpty(message))
            {
                message = string.Format("{0} = {1} can not be less than {2}", paramName, value, minValue);
            }

            throw new ArgumentOutOfRangeException(paramName, message);
        }

        [CanBeNull]
        [ContractAnnotation("value:null => null")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? RequiredGreaterThan<T>(this T? value, T minValue, string paramName, string message = null)
            where T : struct, IComparable
        {
            if (value != null)
            {
                value.Value.RequiredGreaterThan(minValue, paramName, message);
            }

            return value;
        }

        [CanBeNull]
        [ContractAnnotation("value:null => null")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? RequiredGreaterThan<T>(this T? value, T? minValue, string paramName, string message = null)
            where T : struct, IComparable
        {
            if (value != null && minValue != null)
            {
                value.Value.RequiredGreaterThan(minValue.Value, paramName, message);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T RequiredGreaterThan<T>(this T value, T minValue, string paramName, string message = null)
            where T : struct, IComparable
        {
            if (value.CompareTo(minValue) <= 0)
            {
                if (string.IsNullOrEmpty(message))
                {
                    message = string.Format("{0} = {1} must be greater than {2}.", paramName, value, minValue);
                }

                throw new ArgumentOutOfRangeException(paramName, message);
            }

            return value;
        }

        [CanBeNull]
        [ContractAnnotation("value:null => null")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? RequiredNotGreaterThan<T>(this T? value, T maxValue, string paramName, string message = null)
            where T : struct, IComparable
        {
            if (value != null)
            {
                value.Value.RequiredNotGreaterThan(maxValue, paramName, message);
            }

            return value;
        }

        [CanBeNull]
        [ContractAnnotation("value:null => null")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? RequiredNotGreaterThan<T>(this T? value, T? maxValue, string paramName, string message = null)
            where T : struct, IComparable
        {
            if (value != null && maxValue != null)
            {
                value.Value.RequiredNotGreaterThan(maxValue.Value, paramName, message);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T RequiredNotGreaterThan<T>(this T value, T maxValue, string paramName, string message = null)
            where T : struct, IComparable
        {
            if (value.CompareTo(maxValue) <= 0)
            {
                return value;
            }

            if (string.IsNullOrEmpty(message))
            {
                message = string.Format("{0} = {1} can not be greater than {2}.", paramName, value, maxValue);
            }

            throw new ArgumentOutOfRangeException(paramName, message);
        }

        [CanBeNull]
        [ContractAnnotation("value:null => null")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? RequiredBetween<T>(this T? value, T minValue, T maxValue, string paramName, string message = null)
            where T : struct, IComparable
        {
            if (value != null)
            {
                value.Value.RequiredBetween(minValue, maxValue, paramName, message);
            }

            return value;
        }

        [CanBeNull]
        [ContractAnnotation("value:null => null")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? RequiredBetween<T>(this T? value, T? minValue, T? maxValue, string paramName, string message = null)
            where T : struct, IComparable
        {
            if (value == null)
            {
                return null;
            }

            if (minValue != null && maxValue != null)
            {
                value.Value.RequiredBetween(minValue.Value, maxValue.Value, paramName, message);
            }
            else if (minValue != null)
            {
                value.Value.RequiredNotLessThan(minValue.Value, paramName, message);
            }
            else if (maxValue != null)
            {
                value.Value.RequiredLessThan(maxValue.Value, paramName, message);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T RequiredBetween<T>(this T value, T minValue, T maxValue, string paramName, string message = null)
            where T : IComparable
        {
            if (value.CompareTo(minValue) >= 0 && value.CompareTo(maxValue) <= 0)
            {
                return value;
            }

            if (string.IsNullOrEmpty(message))
            {
                message = $"{paramName} = {value} should be within interval {minValue} .. {maxValue}.";
            }

            throw new ArgumentOutOfRangeException(paramName, message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T RequiredInSet<T>(this T value, IEnumerable<T> set, string paramName, string message = null)
            where T : IEquatable<T>
        {
            if (set.Any(value.Equals))
            {
                return value;
            }

            if (string.IsNullOrEmpty(message))
            {
                message = $"{paramName} = {value} should be one of {string.Join(", ", set)}.";
            }

            throw new ArgumentOutOfRangeException(paramName, message);
        }

        [CanBeNull]
        [ContractAnnotation("value:null => null")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? RequiredEnum<T>(this T? value, string paramName, string message = null)
            where T : struct
        {
            if (value != null)
            {
                value.Value.RequiredEnum(paramName, message);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T RequiredEnum<T>(this T value, string paramName, string message = null)
            where T : struct
        {
            if (Enum.GetValues(typeof(T)).Cast<T>().Contains(value))
            {
                return value;
            }

            if (string.IsNullOrEmpty(message))
            {
                message = string.Format("{0} = {1} should be valid {2} enum value.", paramName, value, typeof(T).Name);
            }

            throw new ArgumentOutOfRangeException(paramName, message);
        }

        [CanBeNull]
        [ContractAnnotation("values:null => null")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> RequiredAll<T>([CanBeNull] this IEnumerable<T> values, Action<T> check)
        {
            if (values == null)
            {
                return null;
            }

            // ReSharper disable PossibleMultipleEnumeration
            foreach (var value in values)
            {
                check(value);
            }

            return values;
            // ReSharper restore PossibleMultipleEnumeration
        }
    }
}