using System;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raven.Client;
using Raven.Client.Embedded;
using RavenMagic.Tests.TestHelpers.Models;
using Raven.Json.Linq;

namespace RavenMagic.Tests
{
    public class IDocumentStoreExtensionsTests
    {
        [TestClass]
        public class ChangeRavenClrTypeForDocument
        {
            [TestMethod]
            public void ShouldThrowArgumentNullExceptionWhen_documentStore_IsNull()
            {
                // When
                Action action = () => IDocumentStoreExtensions.ChangeRavenClrTypeForDocument(documentStore: null, id: "fake", newRavenClrType: "fake");

                // Then
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: documentStore");
            }

            [TestMethod]
            public void ShouldThrowArgumentNullExceptionWhen_id_IsNull()
            {
                // Given 
                var fakeDocumentStore = MemoryDocumentStore.Create(waitForNonStaleResults: true);

                // When
                Action action = () => IDocumentStoreExtensions.ChangeRavenClrTypeForDocument(documentStore: fakeDocumentStore, id: null, newRavenClrType: "fake");

                // Then
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: id");
            }

            [TestMethod]
            public void ShouldThrowArgumentNullExceptionWhen_id_IsWhiteSpace()
            {
                // Given 
                var fakeDocumentStore = MemoryDocumentStore.Create(waitForNonStaleResults: true);

                // When
                Action action = () => IDocumentStoreExtensions.ChangeRavenClrTypeForDocument(documentStore: fakeDocumentStore, id: "", newRavenClrType: "fake");

                // Then
                action
                    .ShouldThrow<ArgumentException>()
                    .WithMessage("Value cannot be whitespace.\r\nParameter name: id");
            }

            [TestMethod]
            public void ShouldThrowArgumentNullExceptionWhen_newRavenClrType_IsNull()
            {
                // Given 
                var fakeDocumentStore = MemoryDocumentStore.Create(waitForNonStaleResults: true);

                // When
                Action action = () => IDocumentStoreExtensions.ChangeRavenClrTypeForDocument(documentStore: fakeDocumentStore, id: "fake", newRavenClrType: null);

                // Then
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: newRavenClrType");
            }

            [TestMethod]
            public void ShouldThrowArgumentNullExceptionWhen_newRavenClrType_IsWhiteSpace()
            {
                // Given 
                var fakeDocumentStore = MemoryDocumentStore.Create(waitForNonStaleResults: true);

                // When
                Action action = () => IDocumentStoreExtensions.ChangeRavenClrTypeForDocument(documentStore: fakeDocumentStore, id: "fake", newRavenClrType: "");

                // Then
                action
                    .ShouldThrow<ArgumentException>()
                    .WithMessage("Value cannot be whitespace.\r\nParameter name: newRavenClrType");
            }

            [TestMethod]
            public void ShouldThrowArgumentNullExceptionWhen_id_DoesNotExist()
            {
                // Given 
                var store = MemoryDocumentStore.Initialize(waitForNonStaleResults: true);

                // When
                Action action = () => IDocumentStoreExtensions.ChangeRavenClrTypeForDocument(documentStore: store, id: "fakes/1", newRavenClrType: "fake");

                // Then
                action
                    .ShouldThrow<Exception>()
                    .WithMessage("fakes/1 cannot be found.");
            }

            [TestMethod]
            public void ShouldChange_Raven_Clr_Type()
            {
                // Given
                var expectedRavenClrType = "fake raven clr type";
                var store = MemoryDocumentStore.Initialize(waitForNonStaleResults: true);
                var session = store.OpenSession();
                var product = new Product { Id = "products/1" };

                session.Store(product);
                session.SaveChanges();

                // When
                store.ChangeRavenClrTypeForDocument(product.Id, expectedRavenClrType);

                // Then
                var actualRavenClrType = store.OpenSession().Advanced.GetMetadataFor(product).Value<string>(RavenMetaDataNames.RavenClrType);

                actualRavenClrType.Should().Be(expectedRavenClrType);
            }
        }

        [TestClass]
        public class ClearCollection
        {
            [TestMethod]
            public void ShouldThrowArgumentNullExceptionWhen_documentStore_IsNull()
            {
                // When
                Action action = () => IDocumentStoreExtensions.ClearCollection<object>(documentStore: null);

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
        public class CorrectRavenClrTypeFor
        {
            [TestMethod]
            public void ShouldThrowArgumentNullExceptionWhen_documentStore_IsNull()
            {
                // When
                Action action = () => IDocumentStoreExtensions.CorrectRavenClrTypeForCollection<object>(documentStore: null);

                // Then
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: documentStore");
            }

            [TestMethod]
            public void ShouldChange_MetaData_Raven_Clr_Type_ForDocumentsOf_T()
            {
                // Given
                var store = MemoryDocumentStore.Initialize(waitForNonStaleResults: true);
                var product = new Product { Name = "fake product name"};
                var person = new Person { Name = "fake person name"};

                using (var session = store.OpenSession())
                {
                    session.Store(product);
                    session.Store(person);
                    session.SaveChanges();
                }

                store.ChangeRavenClrTypeForDocument(product.Id, "fake Raven-Clr-Type");
                store.ChangeRavenClrTypeForDocument(person.Id, "fake Raven-Clr-Type");

                // When
                store.CorrectRavenClrTypeForCollection<Product>();

                // Then
                string expectedRavenClrTypeForProduct = string.Format("{0}, {1}", product.GetType().FullName, product.GetType().Assembly.GetName().Name);
                string expectedRavenClrTypeForPerson = "fake Raven-Clr-Type";

                string actualRavenClrTypeForProduct;
                string actualRavenClrTypeForPerson;

                // Get values to perform tests on
                using (var session = store.OpenSession())
                {
                    product = session.Query<Product>().Single();

                    actualRavenClrTypeForProduct = session.Advanced.GetMetadataFor(product).Value<string>(RavenMetaDataNames.RavenClrType);

                    person = session.Query<Person>().Single();

                    actualRavenClrTypeForPerson = session.Advanced.GetMetadataFor(person).Value<string>(RavenMetaDataNames.RavenClrType);
                }

                // Perform the tests
                actualRavenClrTypeForProduct.Should().Be(expectedRavenClrTypeForProduct, "because that is what CorrectRavenClrTypeForCollection() should have changed it to");
                actualRavenClrTypeForPerson.Should().Be(expectedRavenClrTypeForPerson, "because CorrectRavenClrTypeForCollection() should not have touch person documents");

                // Perform sanity checks
                product.Should().NotBeNull("because I'm doing a sanity check that the document could still be read");
                product.Name.Should().Be("fake product name", "because I'm doing a sanity check that the document could still be read");

                person.Should().NotBeNull("because I'm doing a sanity check that the document could still be read");
                person.Name.Should().Be("fake person name", "because I'm doing a sanity check that the document could still be read");
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
                collections.Should().BeEquivalentTo(new string[] { "People", "Products" }, "because we have added model types Person & Product to the database");
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
