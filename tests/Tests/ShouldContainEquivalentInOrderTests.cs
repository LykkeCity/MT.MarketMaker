using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Tests
{
    public class ShouldContainEquivalentInOrderTests
    {
        [Test]
        public void IfSubjectIsNull_ShouldThrow()
        {
            //arrange
            IEnumerable<int> subject = null;
            var expectation = Enumerable.Range(0, 5);

            //act 
            var invocation = subject.Invoking(s => s.ShouldContainEquivalentInOrder(expectation));

            //assert
            invocation.ShouldThrow<AssertionException>()
                .And.Message.Should().StartWith("Expected object not to be <null>.");
        }

        [Test]
        public void IfSubjectIsSmaller_ShouldThrow()
        {
            //arrange
            var subject = Enumerable.Range(0, 4);
            var expectation = Enumerable.Range(0, 5);
            
            //act 
            var invocation = subject.Invoking(s => s.ShouldContainEquivalentInOrder(expectation));

            //assert
            invocation.ShouldThrow<AssertionException>()
                .And.Message.Should()
                .StartWith("Expected a value greater or equal to 5, but found 4.");
        }

        [TestCase("2,2,3")]
        [TestCase("2,1,0,3")]
        public void IfSubjectDoesNotFit_ShouldThrow(string subjectStr)
        {
            //arrange
            var subject = subjectStr.Split(',').Select(int.Parse);
            var expectation = new[] {1, 2, 3};
            
            //act 
            var invocation = subject.Invoking(s => s.ShouldContainEquivalentInOrder(expectation));

            //assert
            invocation.ShouldThrow<AssertionException>()
                .And.Message.Should()
                .StartWith("Expected value to be ");
        }
        
        [TestCase("1,0,2,3")]
        [TestCase("1,2,3")]
        public void IfSubjectDoesFits_ShouldNotThrow(string subjectStr)
        {
            //arrange
            var subject = subjectStr.Split(',').Select(int.Parse);
            var expectation = new[] {1, 2, 3};
            
            //act 
            subject.ShouldContainEquivalentInOrder(expectation);
        }
    }
}