using System;
using System.Linq;
using Raven.Client.Embedded;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RavenMagic.Tests
{
    public class MemoryDocumentStoreTests
    {
        [TestClass]
        public class Create : MemoryDocumentStoreTests
        {
            [TestMethod]
            public void ShouldReturnAnUninitializeDocumentStoreThatRunsInMemory()
            {
                // When
                var documentStore = MemoryDocumentStore.Create(waitForNonStaleResults: true);

                // Then
                documentStore.RunInMemory.Should().BeTrue();

                Action action = () => documentStore.OpenSession();

                action
                    .ShouldThrow<InvalidOperationException>("because we are expect document store has not been initialized and documents stores need to initialized before opening a session.")
                    .WithMessage("You cannot open a session or access the database commands before initializing the document store. Did you forget calling Initialize()?");
            }

            [TestMethod]
            public void ShouldReturnDocumentStoreThatDoesNotAllowStaleResultsWhen_waitForNonStaleResults_IsTrue()
            {
                // When
                var documentStore = MemoryDocumentStore.Create(waitForNonStaleResults: true);

                // Then
                this.IsNoStaleQueriesListenerRegistered(documentStore).Should().BeTrue();
            }

            [TestMethod]
            public void ShouldReturnDocumentStoreThatDoesAllowStaleResultsWhen_waitForNonStaleResults_IsFalse()
            {
                // When
                var documentStore = MemoryDocumentStore.Create(waitForNonStaleResults: false);

                // Then
                this.IsNoStaleQueriesListenerRegistered(documentStore).Should().BeFalse();
            }
        }

        [TestClass]
        public class Initialize : MemoryDocumentStoreTests
        {
            [TestMethod]
            public void ShouldReturnAnInitializeDocumentStoreThatRunsInMemory()
            {
                // When
                var documentStore = MemoryDocumentStore.Initialize(waitForNonStaleResults: true);

                // Then
                documentStore.RunInMemory.Should().BeTrue();

                Action action = () => documentStore.OpenSession();

                action.ShouldNotThrow<Exception>("because we are expect document store has been initialized and documents stores need to initialized before opening a session.");
            }

            [TestMethod]
            public void ShouldReturnDocumentStoreThatDoesNotAllowStaleResultsWhen_waitForNonStaleResults_IsTrue()
            {
                // When
                var documentStore = MemoryDocumentStore.Initialize(waitForNonStaleResults: true);

                // Then
                this.IsNoStaleQueriesListenerRegistered(documentStore).Should().BeTrue();
            }

            [TestMethod]
            public void ShouldReturnDocumentStoreThatDoesAllowStaleResultsWhen_waitForNonStaleResults_IsFalse()
            {
                // When
                var documentStore = MemoryDocumentStore.Initialize(waitForNonStaleResults: false);

                // Then
                this.IsNoStaleQueriesListenerRegistered(documentStore).Should().BeFalse();
            }
        }

        private bool IsNoStaleQueriesListenerRegistered(EmbeddableDocumentStore documentStore)
        {
            return documentStore.RegisteredQueryListeners.Any(q => q.GetType().IsAssignableFrom(typeof(WaitForNonStaleResultsListener)));
        }
    }
}
