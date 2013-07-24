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
                IDocumentStore store = (new EmbeddableDocumentStore() { RunInMemory = true }).Initialize();
                
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
                IDocumentStore store = (new EmbeddableDocumentStore() { RunInMemory = true }).Initialize();

                // When
                var collections = store.QueryCollections();

                // Then
                collections.Any().Should().BeFalse();
            }
        }
    }
}
