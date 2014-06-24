using System;
using System.Linq;
using FluentAssertions;
using Xunit;
using Raven.Client.Embedded;

namespace RavenMagic.Tests
{
    public class MemoryDocumentStoreTests
    {
        public class Constructor : MemoryDocumentStoreTests
        {
            [Fact]
            public void ShouldReturnADocumentStoreThatRunsInMemory()
            {
                // When
                var documentStore = new MemoryDocumentStore();

                // Then
                documentStore.RunInMemory.Should().BeTrue();
            }

            [Fact]
            public void ShouldReturnAnInitializedDocumentStoreWhen_initialize_IsTrue()
            {
                // When
                var documentStore = new MemoryDocumentStore(initialize: true);

                // Then
                Action action = () => documentStore.OpenSession();

                action.ShouldNotThrow<Exception>("because we are expect document documentStore has been initialized and documents stores need to initialized before opening a session.");
            }

            [Fact]
            public void ShouldReturnAnUninitializedDocumentStoreWhen_initialize_IsFalse()
            {
                // When
                var documentStore = new MemoryDocumentStore(initialize: false, createDocumentsByEntityNameIndex: false);

                // Then
                Action action = () => documentStore.OpenSession();

                action
                    .ShouldThrow<InvalidOperationException>("because we are expect document store has not been initialized and documents stores need to initialized before opening a session.")
                    .WithMessage("You cannot open a session or access the database commands before initializing the document store. Did you forget calling Initialize()?");
            }

            [Fact]
            public void ShouldReturnDocumentStoreThatDoesNotAllowStaleResultsWhen_waitForNonStaleResults_IsTrue()
            {
                // When
                var documentStore = new MemoryDocumentStore(waitForNonStaleResults: true);

                // Then
                this.IsNoStaleQueriesListenerRegistered(documentStore).Should().BeTrue();
            }

            [Fact]
            public void ShouldReturnDocumentStoreThatDoesAllowStaleResultsWhen_waitForNonStaleResults_IsFalse()
            {
                // When
                var documentStore = new MemoryDocumentStore(waitForNonStaleResults: false);

                // Then
                this.IsNoStaleQueriesListenerRegistered(documentStore).Should().BeFalse();
            }

            [Fact]
            public void ShouldThrowArgumentExceptionWhen_initialize_IsFalseAnd_createDocumentsByEntityNameIndex_IsTrue()
            {
                // When
                Action action = () => new MemoryDocumentStore(initialize: false, createDocumentsByEntityNameIndex: true);

                // Then
                action.ShouldThrow<ArgumentException>().WithMessage("initialize cannot be false when createDocumentsByEntityNameIndex is true.");
            }
        }

        private bool IsNoStaleQueriesListenerRegistered(EmbeddableDocumentStore documentStore)
        {
            return documentStore.RegisteredQueryListeners.Any(q => q.GetType().IsAssignableFrom(typeof(WaitForNonStaleResultsListener)));
        }
    }
}
