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
        public static void ShouldContainEquivalentInOrder<TSubject, TExpectation>(
            this IEnumerable<TSubject> subjectEnumerable,
            IEnumerable<TExpectation> expectationEnumerable,
            Func<EquivalencyAssertionOptions<TExpectation>, EquivalencyAssertionOptions<TExpectation>> config)
        {
            AssertIsNotNull(subjectEnumerable);
            var subject = subjectEnumerable.ToList();
            var expectation = expectationEnumerable.ToList();
            AssertLengthSufficient(subject.Count, expectation.Count);
            AssertContais(subject, expectation, config);
        }

        public static void ShouldContainEquivalentInOrder<TSubject, TExpectation>(this IEnumerable<TSubject> subject,
            IEnumerable<TExpectation> expectation)
        {
            subject.ShouldContainEquivalentInOrder(expectation, o => o);
        }

        private static void AssertIsNotNull(object subject)
        {
            subject.Should().NotBeNull(" subject collection should not be null");
        }

        private static void AssertLengthSufficient(int subjectLength, int expectationLength)
        {
            subjectLength.Should().BeGreaterOrEqualTo(expectationLength, " subject collection should be long enouth");
        }

        private static void AssertContais<TSubject, TExpectation>(IReadOnlyList<TSubject> subjects,
            IReadOnlyList<TExpectation> expectations,
            Func<EquivalencyAssertionOptions<TExpectation>, EquivalencyAssertionOptions<TExpectation>> config)
        {
            var subjectIndex = 0;
            var matchedExpectationsCount = 0;
            foreach (var expectation in expectations)
            {
                while (subjectIndex < subjects.Count)
                {
                    var subject = subjects[subjectIndex];
                    if (subject.GetType() != expectation.GetType())
                    {
                        subjectIndex++;
                        continue;
                    }
                    try
                    {
                        subject.Should().BeEquivalentTo(expectation, config);
                        subjectIndex++;
                        matchedExpectationsCount++;
                        break;
                    }
                    catch (AssertionException e) when (subjectIndex < subjects.Count)
                    {
                        subjectIndex++;
                        Console.WriteLine(e);
                    }
                }
            }

            matchedExpectationsCount.Should().Be(expectations.Count, " all expected objects should be found");
        }
    }
}