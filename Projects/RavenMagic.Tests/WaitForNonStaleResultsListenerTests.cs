using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Raven.Client;

namespace RavenMagic.Tests
{
    public class WaitForNonStaleResultsListenerTests
    {
        [TestClass]
        public class Constructor
        {
            [TestMethod]
            public void ShouldThrowNotArgumentNullExceptionWhen_timeSpan_IsNull()
            {
                // When
                Action action = () => new WaitForNonStaleResultsListener(null);

                // Then
                action.ShouldNotThrow<ArgumentException>("because WaitForNonStaleResultsListener.TimeSpan can be null");
            }
        }

        [TestClass]
        public class TimeSpan
        {
            [TestMethod]
            public void ShouldBeValuePassedToConstructor()
            {
                (new WaitForNonStaleResultsListener()).WaitTimeout.HasValue.Should().BeFalse("because value was not passed to constructor");
                (new WaitForNonStaleResultsListener(System.TimeSpan.FromSeconds(10))).WaitTimeout.Value.Should().Be(System.TimeSpan.FromSeconds(10), "because it is the value passed to constructor");
            }
        }

        [TestClass]
        public class BeforeQueryExecuted
        {
            [TestMethod]
            public void ShouldThrowArgumentNullExceptionWhen_queryCustomization_IsNull()
            {
                // Given
                var listener = new WaitForNonStaleResultsListener();

                // When
                Action action = () => listener.BeforeQueryExecuted(null);

                // Then
                action.ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("queryCustomization");
            }

            [TestMethod]
            public void ShouldSet_queryCustomization_WaitForNonStaleResults_When_WaitTimeout_IsNull()
            {
                // Given
                var listener = new WaitForNonStaleResultsListener();
                var queryCustomization = new Mock<IDocumentQueryCustomization>();

                // When
                listener.BeforeQueryExecuted(queryCustomization.Object);

                // Then
                queryCustomization.Verify(x => x.WaitForNonStaleResults(), Times.Exactly(1));
            }

            [TestMethod]
            public void ShouldSet_queryCustomization_WaitForNonStaleResults_When_WaitTimeout_IsNotNull()
            {
                // Given
                var waitTimeOut = System.TimeSpan.FromSeconds(11);
                var listener = new WaitForNonStaleResultsListener(waitTimeOut);
                var queryCustomization = new Mock<IDocumentQueryCustomization>();

                // When
                listener.BeforeQueryExecuted(queryCustomization.Object);

                // Then
                queryCustomization.Verify(x => x.WaitForNonStaleResults(waitTimeOut), Times.Exactly(1));
            }
        }
    }
}
