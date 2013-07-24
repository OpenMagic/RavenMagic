using System;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raven.Client;
using Raven.Client.Embedded;
using RavenMagic.Tests.TestHelpers.Models;

namespace RavenMagic.Tests
{
    public class IDocumentStoreExtensionsTests
    {
        [TestClass]
        public class ClearCollection
        {
            [TestMethod]
            public void ShouldThrowArgumentNullExceptionWhen_documentStore_IsNull()
            {
                // When
                Action action = () => IDocumentStoreExtensions.ClearCollection<object>(documentStore:null);

                // Then
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: documentStore");
            }

            [TestMethod]
            public void ShouldNotThrowExceptionDocumentStoreDoesNotHaveRequestedCollection()
            {
                // Given
                IDocumentStore store = MemoryDocumentStore.Initialize(waitForNonStaleResults: false);

                // When
                Action action = () => store.ClearCollection<Person>();

                // Then
                action.ShouldNotThrow<Exception>("because we want to allow unit tests to be overly cautious");
            }

            [TestMethod]
            public void ShouldDeleteAllDocumentsFromTheDatabase()
            {
                // Given
                IDocumentStore store = MemoryDocumentStore.Initialize(waitForNonStaleResults: false);

                using (IDocumentSession session = store.OpenSession())
                {
                    session.Store(new Person());
                    session.SaveChanges();
                }

                // When
                store.ClearCollection<Person>();

                // Then
                using (IDocumentSession session = store.OpenSession())
                {
                    session.Query<Person>().Any().Should().BeFalse("because all documents should have been deleted.");
                }                
            }
        }

        [TestClass]
        public class QueryCollections
        {
            [TestMethod]
            public void ShouldThrowArgumentNullExceptionWhen_documentStore_IsNull()
            {
                // When
                Action action = () => IDocumentStoreExtensions.QueryCollections(null);

                // Then
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: documentStore");
            }

            [TestMethod]
            public void ShouldReturnEnumerableOfCollectionsInDocumentStore()
            {
                // Given
                IDocumentStore store = MemoryDocumentStore.Initialize(waitForNonStaleResults: false);
                
                using (IDocumentSession session = store.OpenSession())
                {
                    session.Store(new Person());
                    session.Store(new Product());

                    session.SaveChanges();
                }

                // When
                var collections = store.QueryCollections();

                // Then
                collections.Should().BeEquivalentTo(new string[] {"People", "Products"}, "because we have added model types Person & Product to the database");
            }

            [TestMethod]
            public void ShouldReturnEmptyEnumerableWhenDocumentStoreIsEmpty()
            {
                // Given
                IDocumentStore store = MemoryDocumentStore.Initialize(waitForNonStaleResults: false);

                // When
                var collections = store.QueryCollections();

                // Then
                collections.Any().Should().BeFalse();
            }
        }
    }
}
