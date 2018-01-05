using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Equivalency;
using NUnit.Framework;

namespace Tests
{
    public static class AssertExtensions
    {
        public static void ShouldContainEquivalentInOrder<T>(this IEnumerable<T> subjectEnumerable,
            IEnumerable expectationEnumerable,
            Func<EquivalencyAssertionOptions<T>, EquivalencyAssertionOptions<T>> config)
        {
            AssertIsNotNull(subjectEnumerable);
            var subject = subjectEnumerable.ToArray();
            var expectation = expectationEnumerable.Cast<object>().ToArray();
            AssertLengthSufficient(subject.Length, expectation.Length);
            AssertContais(subject, expectation, config);
        }

        public static void ShouldContainEquivalentInOrder<T>(this IEnumerable<T> subject, IEnumerable expectation)
        {
            subject.ShouldContainEquivalentInOrder(expectation, o => o);
        }

        private static void AssertIsNotNull(object subject)
        {
            subject.Should().NotBeNull();
        }

        private static void AssertLengthSufficient(int subjectLength, int expectationLength)
        {
            subjectLength.Should().BeGreaterOrEqualTo(expectationLength);
        }

        private static void AssertContais<T>(T[] subjects, object[] expectations, Func<EquivalencyAssertionOptions<T>, EquivalencyAssertionOptions<T>> config)
        {
            var subjectIndex = 0;
            var matchedExpectationsCount = 0;
            for (var expectationIndex = 0; expectationIndex < expectations.Length; expectationIndex++)
            {
                while (subjectIndex < subjects.Length)
                {
                    try
                    {
                        var subject = subjects[subjectIndex];
                        var expectation = expectations[expectationIndex];
                        subject.ShouldBeEquivalentTo(expectation, config);
                        subjectIndex++;
                        matchedExpectationsCount++;
                        break;
                    }
                    catch (AssertionException) when (subjectIndex < subjects.Length)
                    {
                        subjectIndex++;
                    }
                }
            }

            matchedExpectationsCount.Should().Be(expectations.Length);
        }
    }
}