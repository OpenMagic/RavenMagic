using System;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raven.Client;
using Raven.Client.Indexes;
using RavenMagic.Tests.TestHelpers.Models;

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
                var fakeDocumentStore = new MemoryDocumentStore();

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
                var fakeDocumentStore = new MemoryDocumentStore();

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
                var fakeDocumentStore = new MemoryDocumentStore();

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
                var fakeDocumentStore = new MemoryDocumentStore();

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
                var store = new MemoryDocumentStore();

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
                var store = new MemoryDocumentStore();
                var session = store.OpenSession();
                var product = new Product { Id = "products/1" };

                session.Store(product);
                session.SaveChanges();

                // When
                store.ChangeRavenClrTypeForDocument(product.Id, expectedRavenClrType);

                // Then
                var actualRavenClrType = store.OpenSession().Advanced.GetMetadataFor(product).Value<string>(RavenConstants.Metadata.RavenClrType);

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
            public void ShouldThrowArgumentNullExceptionWhen_indexName_IsNull()
            {
                // Given
                var store = new MemoryDocumentStore();

                // When
                Action action = () => store.ClearCollection<object>(indexName: null);

                // Then
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: indexName");
            }

            [TestMethod]
            public void ShouldThrowArgumentExceptionWhen_indexName_IsWhitespace()
            {
                // Given
                var store = new MemoryDocumentStore();

                // When
                Action action = () => store.ClearCollection<object>(indexName: "");

                // Then
                action
                    .ShouldThrow<ArgumentException>()
                    .WithMessage("Value cannot be whitespace.\r\nParameter name: indexName");
            }

            [TestMethod]
            public void ShouldNotThrowExceptionDocumentStoreDoesNotHaveRequestedCollection()
            {
                // Given
                IDocumentStore store = new MemoryDocumentStore();

                // When
                Action action = () => store.ClearCollection<Person>();

                // Then
                action.ShouldNotThrow<Exception>("because we want to allow unit tests to be overly cautious");
            }

            [TestMethod]
            public void ShouldDeleteAllDocumentsFromTheDatabase()
            {
                // Given
                IDocumentStore store = new MemoryDocumentStore();

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
        public class CreateDocumentsByEntityNameIndex
        {
            [TestMethod]
            public void ShouldThrow_ArgumentNullException_When_documentStore_IsNull()
            {
                // When
                Action action = () => IDocumentStoreExtensions.CreateDocumentsByEntityNameIndex(null);

                // Then
                action.ShouldThrow<ArgumentNullException>().WithMessage("Value cannot be null.\r\nParameter name: documentStore");
            }

            [TestMethod]
            public void ShouldCreate_Raven_DocumentsByEntityName_IndexWhenDoesNotExist()
            {
                // Given
                var store = new MemoryDocumentStore(createDocumentsByEntityNameIndex: false);

                store.DatabaseCommands.GetIndexes(0, 1024).Count().Should().Be(0, "because I'm testing the assumption a memory document documentStore has 0 indexes when created");

                // When
                store.CreateDocumentsByEntityNameIndex();

                // Then
                var indexes = store.DatabaseCommands.GetIndexes(0, 1024);
                var indexName = indexes.Single().Name;

                indexName.Should().Be("Raven/DocumentsByEntityName");
            }

            [TestMethod]
            public void ShouldNotThrowAnExceptionWhen_Raven_DocumentsByEntityName_IndexDoesExist()
            {
                // Given
                var store = new MemoryDocumentStore(createDocumentsByEntityNameIndex: true);

                // When
                Action action = () => store.CreateDocumentsByEntityNameIndex();

                // Then
                action.ShouldNotThrow<Exception>();
            }
        }

        [TestClass]
        public class CreateTemporaryIndex
        {
            [TestMethod]
            public void ShouldThrowArgumentNullExceptionWhen_documentStore_IsNull()
            {
                // When
                Action action = () => IDocumentStoreExtensions.CreateTemporaryIndex<object>(documentStore: null);

                // Then
                action.ShouldThrow<ArgumentNullException>().WithMessage("Value cannot be null.\r\nParameter name: documentStore");
            }

            [TestMethod]
            public void ShouldThrowArgumentNullExceptionWhen_documentType_IsNull()
            {
                // Given
                var store = new MemoryDocumentStore();

                // When
                Action action = () => store.CreateTemporaryIndex(documentType: null);

                // Then
                action.ShouldThrow<ArgumentNullException>().WithMessage("Value cannot be null.\r\nParameter name: documentType");
            }

            [TestMethod]
            public void ShouldCreateTemporaryIndexAndReturnIndexName()
            {
                // Given
                var store = new MemoryDocumentStore(createDocumentsByEntityNameIndex: false);

                store.DatabaseCommands.GetIndexes(0, 1024).Count().Should().Be(0, "because I'm testing the assumption a memory document documentStore has 0 indexes when created");

                // When
                var indexName = store.CreateTemporaryIndex<Product>();

                // Then
                var indexes = from i in store.DatabaseCommands.GetIndexes(0, 1024) select i.Name;

                indexes.Should().BeEquivalentTo(new string[] { indexName });
            }

            [TestMethod]
            public void ShouldCreateTemporaryIndexAndReturnIndexNameForDocumentType()
            {
                // Given
                var store = new MemoryDocumentStore(createDocumentsByEntityNameIndex: false);

                // When
                var indexName = store.CreateTemporaryIndex(typeof(Product));

                // Then
                var indexes = from i in store.DatabaseCommands.GetIndexes(0, 1024) select i.Name;

                indexes.Should().BeEquivalentTo(new string[] { indexName });
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
            public void ShouldThrowArgumentExceptionWhen_indexName_IsNull()
            {
                // Given
                var fakeStore = new MemoryDocumentStore();

                // When
                Action action = () => IDocumentStoreExtensions.CorrectRavenClrTypeForCollection<object>(documentStore: fakeStore, indexName: null);

                // Then
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: indexName");
            }

            [TestMethod]
            public void ShouldThrowArgumentExceptionWhen_indexName_IsWhiteSpace()
            {
                // Given
                var fakeStore = new MemoryDocumentStore();

                // When
                Action action = () => IDocumentStoreExtensions.CorrectRavenClrTypeForCollection<object>(documentStore: fakeStore, indexName: "");

                // Then
                action
                    .ShouldThrow<ArgumentException>()
                    .WithMessage("Value cannot be whitespace.\r\nParameter name: indexName");
            }

            [TestMethod]
            public void ShouldChange_MetaData_Raven_Clr_Type_ForDocumentsOf_T()
            {
                // Given
                var store = new MemoryDocumentStore();
                var product = new Product { Name = "fake product name" };
                var person = new Person { Name = "fake person name" };

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

                    actualRavenClrTypeForProduct = session.Advanced.GetMetadataFor(product).Value<string>(RavenConstants.Metadata.RavenClrType);

                    person = session.Query<Person>().Single();

                    actualRavenClrTypeForPerson = session.Advanced.GetMetadataFor(person).Value<string>(RavenConstants.Metadata.RavenClrType);
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

            [TestMethod]
            public void ShouldChange_MetaData_Raven_Clr_Type_ForDocumentsOf_T_When_indexName_OfExistingIndexIsProvided()
            {
                // Given
                var store = new MemoryDocumentStore();
                var product = new Product { Name = "fake product name" };
                var person = new Person { Name = "fake person name" };
                var indexName = "ProductsIndex";

                store.DatabaseCommands.PutIndex(indexName, new IndexDefinitionBuilder<Product> { Map = documents => documents.Select(p => new { p.Name }) });

                using (var session = store.OpenSession())
                {
                    session.Store(product);
                    session.Store(person);
                    session.SaveChanges();
                }

                store.ChangeRavenClrTypeForDocument(product.Id, "fake Raven-Clr-Type");
                store.ChangeRavenClrTypeForDocument(person.Id, "fake Raven-Clr-Type");

                // When
                store.CorrectRavenClrTypeForCollection<Product>(indexName);

                // Then
                string expectedRavenClrTypeForProduct = string.Format("{0}, {1}", product.GetType().FullName, product.GetType().Assembly.GetName().Name);
                string expectedRavenClrTypeForPerson = "fake Raven-Clr-Type";

                string actualRavenClrTypeForProduct;
                string actualRavenClrTypeForPerson;

                // Get values to perform tests on
                using (var session = store.OpenSession())
                {
                    product = session.Query<Product>().Single();

                    actualRavenClrTypeForProduct = session.Advanced.GetMetadataFor(product).Value<string>(RavenConstants.Metadata.RavenClrType);

                    person = session.Query<Person>().Single();

                    actualRavenClrTypeForPerson = session.Advanced.GetMetadataFor(person).Value<string>(RavenConstants.Metadata.RavenClrType);
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
        public class CorrectRavenClrTypeFor_NonGeneric
        {
            [TestMethod]
            public void ShouldThrowArgumentNullExceptionWhen_documentStore_IsNull()
            {
                // Given
                var fakeType = typeof(Exception);

                // When
                Action action = () => IDocumentStoreExtensions.CorrectRavenClrTypeForCollection(documentStore: null, documentType: fakeType);

                // Then
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: documentStore");
            }

            [TestMethod]
            public void ShouldThrowArgumentNullExceptionWhen_documentType_IsNull()
            {
                // Given
                var fakeDocumentStore = new MemoryDocumentStore();

                // When
                Action action = () => IDocumentStoreExtensions.CorrectRavenClrTypeForCollection(documentStore: fakeDocumentStore, documentType: null);

                // Then
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: documentType");
            }

            [TestMethod]
            public void ShouldChange_MetaData_Raven_Clr_Type_ForDocumentsOf_T()
            {
                // Given
                var store = new MemoryDocumentStore();
                var product = new Product { Name = "fake product name" };
                var person = new Person { Name = "fake person name" };

                using (var session = store.OpenSession())
                {
                    session.Store(product);
                    session.Store(person);
                    session.SaveChanges();
                }

                store.ChangeRavenClrTypeForDocument(product.Id, "fake Raven-Clr-Type");
                store.ChangeRavenClrTypeForDocument(person.Id, "fake Raven-Clr-Type");

                // When
                store.CorrectRavenClrTypeForCollection(typeof(Product));

                // Then
                string expectedRavenClrTypeForProduct = string.Format("{0}, {1}", product.GetType().FullName, product.GetType().Assembly.GetName().Name);
                string expectedRavenClrTypeForPerson = "fake Raven-Clr-Type";

                string actualRavenClrTypeForProduct;
                string actualRavenClrTypeForPerson;

                // Get values to perform tests on
                using (var session = store.OpenSession())
                {
                    product = session.Query<Product>().Single();

                    actualRavenClrTypeForProduct = session.Advanced.GetMetadataFor(product).Value<string>(RavenConstants.Metadata.RavenClrType);

                    person = session.Query<Person>().Single();

                    actualRavenClrTypeForPerson = session.Advanced.GetMetadataFor(person).Value<string>(RavenConstants.Metadata.RavenClrType);
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
        public class CorrectRavenClrTypeForCollectionWithArguments_documentType_indexName
        {
            [TestMethod]
            public void ShouldThrowArgumentNullExceptionWhen_documentStore_IsNull()
            {
                // When
                Action action = () => IDocumentStoreExtensions.CorrectRavenClrTypeForCollection(documentStore: null, documentType: this.GetType(), indexName: "fake");

                // Then
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: documentStore");
            }

            [TestMethod]
            public void ShouldThrowArgumentNullExceptionWhen_documentType_IsNull()
            {
                // Given
                var store = new MemoryDocumentStore();

                // When
                Action action = () => store.CorrectRavenClrTypeForCollection(documentType: null, indexName: "fake");

                // Then
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: documentType");
            }

            [TestMethod]
            public void ShouldThrowArgumentNullExceptionWhen_indexName_IsNull()
            {
                // Given
                var store = new MemoryDocumentStore();

                // When
                Action action = () => store.CorrectRavenClrTypeForCollection(documentType: this.GetType(), indexName: null);

                // Then
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: indexName");
            }

            [TestMethod]
            public void ShouldThrowArgumentExceptionWhen_indexName_IsWhiteSpace()
            {
                // Given
                var store = new MemoryDocumentStore();

                // When
                Action action = () => store.CorrectRavenClrTypeForCollection(documentType: this.GetType(), indexName: "");

                // Then
                action
                    .ShouldThrow<ArgumentException>()
                    .WithMessage("Value cannot be whitespace.\r\nParameter name: indexName");
            }

            [TestMethod]
            public void ShouldChange_MetaData_Raven_Clr_Type_ForDocuments()
            {
                // Given
                var store = new MemoryDocumentStore();
                var product = new Product { Name = "fake product name" };
                var person = new Person { Name = "fake person name" };

                using (var session = store.OpenSession())
                {
                    session.Store(product);
                    session.Store(person);
                    session.SaveChanges();
                }

                var indexName = store.CreateTemporaryIndex<Product>();

                store.WaitForNonStaleResults(indexName);

                store.ChangeRavenClrTypeForDocument(product.Id, "fake Raven-Clr-Type");
                store.ChangeRavenClrTypeForDocument(person.Id, "fake Raven-Clr-Type");
                
                // When
                store.CorrectRavenClrTypeForCollection(typeof(Product), indexName);

                // Then
                string expectedRavenClrTypeForProduct = string.Format("{0}, {1}", product.GetType().FullName, product.GetType().Assembly.GetName().Name);
                string expectedRavenClrTypeForPerson = "fake Raven-Clr-Type";

                string actualRavenClrTypeForProduct;
                string actualRavenClrTypeForPerson;

                // Get values to perform tests on
                using (var session = store.OpenSession())
                {
                    product = session.Query<Product>().Single();

                    actualRavenClrTypeForProduct = session.Advanced.GetMetadataFor(product).Value<string>(RavenConstants.Metadata.RavenClrType);

                    person = session.Query<Person>().Single();

                    actualRavenClrTypeForPerson = session.Advanced.GetMetadataFor(person).Value<string>(RavenConstants.Metadata.RavenClrType);
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
        public class DeleteIndex
        {
            [TestMethod]
            public void ShouldThrowArgumentNullExceptionWhen_documentStore_IsNull()
            {
                // When
                Action action = () => IDocumentStoreExtensions.DeleteIndex(documentStore: null, indexName: "fake");

                // Then
                action.ShouldThrow<ArgumentNullException>().WithMessage("Value cannot be null.\r\nParameter name: documentStore");
            }

            [TestMethod]
            public void ShouldThrowArgumentExceptionWhen_indexName_IsNull()
            {
                // Given
                var store = new MemoryDocumentStore();

                // When
                Action action = () => store.DeleteIndex(indexName: null);

                // Then
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: indexName");
            }

            [TestMethod]
            public void ShouldThrowArgumentExceptionWhen_indexName_IsWhiteSpace()
            {
                // Given
                var store = new MemoryDocumentStore();

                // When
                Action action = () => store.DeleteIndex(indexName: "");

                // Then
                action
                    .ShouldThrow<ArgumentException>()
                    .WithMessage("Value cannot be whitespace.\r\nParameter name: indexName");
            }

            [TestMethod]
            public void ShouldDeleteIndex()
            {
                // Given
                var store = new MemoryDocumentStore(createDocumentsByEntityNameIndex: false);
                var indexName = store.CreateTemporaryIndex<Product>();

                // When
                store.DeleteIndex(indexName);

                // Then
                var indexes = store.DatabaseCommands.GetIndexes(0, 1024);

                indexes.Count().Should().Be(0);
            }
        }

        [TestClass]
        public class IsIndexStale : BaseTestClass
        {
            [TestMethod]
            public void ShouldThrowArgumentNullExceptionWhen_documentStore_IsNull()
            {
                // When
                Action action = () => IDocumentStoreExtensions.IsIndexStale(documentStore: null, indexName: "fake");

                // Then
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: documentStore");
            }

            [TestMethod]
            public void ShouldThrowArgumentExceptionWhen_indexName_IsNull()
            {
                // Given
                var store = new MemoryDocumentStore();

                // When
                Action action = () => store.IsIndexStale(indexName: null);

                // Then
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: indexName");
            }

            [TestMethod]
            public void ShouldThrowArgumentExceptionWhen_indexName_IsWhiteSpace()
            {
                // Given
                var store = new MemoryDocumentStore();

                // When
                Action action = () => store.IsIndexStale(indexName: "");

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

                // When
                var isStale = store.IsIndexStale(indexName);

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

                // When
                var isStale = store.IsIndexStale(indexName);

                // Then
                isStale.Should().BeFalse();
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
                IDocumentStore store = new MemoryDocumentStore();

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
                IDocumentStore store = new MemoryDocumentStore();

                // When
                var collections = store.QueryCollections();

                // Then
                collections.Any().Should().BeFalse();
            }
        }

        [TestClass]
        public class WaitForNonStaleResults : BaseTestClass
        {
            [TestMethod]
            public void ShouldThrowArgumentNullExceptionWhen_documentStore_IsNull()
            {
                // When
                Action action = () => IDocumentStoreExtensions.WaitForNonStaleResults(documentStore: null, indexName: "fake");

                // Then
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: documentStore");
            }

            [TestMethod]
            public void ShouldThrowArgumentExceptionWhen_indexName_IsNull()
            {
                // Given
                var fakeStore = new MemoryDocumentStore();

                // When
                Action action = () => IDocumentStoreExtensions.WaitForNonStaleResults(documentStore: fakeStore, indexName: null);

                // Then
                action
                    .ShouldThrow<ArgumentNullException>()
                    .WithMessage("Value cannot be null.\r\nParameter name: indexName");
            }

            [TestMethod]
            public void ShouldThrowArgumentExceptionWhen_indexName_IsWhiteSpace()
            {
                // Given
                var fakeStore = new MemoryDocumentStore();

                // When
                Action action = () => IDocumentStoreExtensions.WaitForNonStaleResults(documentStore: fakeStore, indexName: "");

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

                // When
                store.WaitForNonStaleResults(indexName);

                // Then
                store.IsIndexStale(indexName).Should().BeFalse();
            }
        }
    }
}
