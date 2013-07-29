using System;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RavenMagic.Tests
{
    public class IDocumentSessionExtensionsTests
    {
        [TestClass]
        public class IsIndexStale : BaseTestClass
        {
            [TestMethod]
            public void ShouldThrowArgumentNullExceptionWhen_documentSession_IsNull()
            {
                // When
                Action action = () => IDocumentSessionExtensions.IsIndexStale(documentSession: null, indexName: "fake");

                // Then
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: documentSession");
            }

            [TestMethod]
            public void ShouldThrowArgumentExceptionWhen_indexName_IsNull()
            {
                // Given
                var session = (new MemoryDocumentStore()).OpenSession();

                // When
                Action action = () => session.IsIndexStale(indexName: null);

                // Then
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: indexName");
            }

            [TestMethod]
            public void ShouldThrowArgumentExceptionWhen_indexName_IsWhiteSpace()
            {
                // Given
                var session = (new MemoryDocumentStore()).OpenSession();

                // When
                Action action = () => session.IsIndexStale(indexName: "");

                // Then
                action
                    .ShouldThrow<ArgumentException>()
                    .WithMessage("Value cannot be whitespace.\r\nParameter name: indexName");
            }

            [TestMethod]
            public void ShouldReturnTrueWhenTheIndexIsStale()
            {
                // Given
                var store = new MemoryDocumentStore(waitForNonStaleResults: false);
                var indexName = "ProductsIndex";

                StoreFakeProducts(store, 1000);
                CreateProductsIndex(store, indexName);
                ValidateIndexIsStale(store, indexName);

                var session = store.OpenSession();

                // When
                var isStale = session.IsIndexStale(indexName);

                // Then
                isStale.Should().BeTrue();
            }

            [TestMethod]
            public void ShouldReturnFalseWhenTheIndexIsNotStale()
            {
                // Given
                var store = new MemoryDocumentStore(waitForNonStaleResults: false);
                var indexName = "ProductsIndex";

                StoreFakeProducts(store, 1000);
                CreateProductsIndex(store, indexName);
                store.WaitForNonStaleResults(indexName);

                var session = store.OpenSession();

                // When
                var isStale = session.IsIndexStale(indexName);

                // Then
                isStale.Should().BeFalse();
            }
        }

        [TestClass]
        public class WaitForNonStaleResults : BaseTestClass
        {
            [TestMethod]
            public void ShouldThrowArgumentNullExceptionWhen_documentStore_IsNull()
            {
                // When
                Action action = () => IDocumentSessionExtensions.WaitForNonStaleResults(documentSession: null, indexName: "fake");

                // Then
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: documentSession");
            }

            [TestMethod]
            public void ShouldThrowArgumentExceptionWhen_indexName_IsNull()
            {
                // Given
                var fakeSession = (new MemoryDocumentStore()).OpenSession();

                // When
                Action action = () => IDocumentSessionExtensions.WaitForNonStaleResults(documentSession: fakeSession, indexName: null);

                // Then
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: indexName");
            }

            [TestMethod]
            public void ShouldThrowArgumentExceptionWhen_indexName_IsWhiteSpace()
            {
                // Given
                var fakeSession = (new MemoryDocumentStore()).OpenSession();

                // When
                Action action = () => IDocumentSessionExtensions.WaitForNonStaleResults(documentSession: fakeSession, indexName: "");

                // Then
                action
                    .ShouldThrow<ArgumentException>()
                    .WithMessage("Value cannot be whitespace.\r\nParameter name: indexName");
            }

            [TestMethod]
            public void ShouldEnsureIndexIsNotStale()
            {
                // Given
                var store = new MemoryDocumentStore(waitForNonStaleResults: false);
                var indexName = "ProductsIndex";
                var fakeDocumentCount = 1000;

                StoreFakeProducts(store, fakeDocumentCount);
                CreateProductsIndex(store, indexName);
                ValidateIndexIsStale(store, indexName);

                var session = store.OpenSession();

                // When
                session.WaitForNonStaleResults(indexName);

                // Then
                session.IsIndexStale(indexName).Should().BeFalse();
            }
        }
    }
}
