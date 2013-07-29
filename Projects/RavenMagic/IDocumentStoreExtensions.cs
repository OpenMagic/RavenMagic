using System;
using System.Collections.Generic;
using System.Linq;
using OpenMagic;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Indexes;
using Raven.Json.Linq;

namespace RavenMagic
{
    public static class IDocumentStoreExtensions
    {

        /// <summary>
        /// Deletes the collection for <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The document type of the collection to delete.</typeparam>
        /// <param name="documentStore">The document store to remove collection from.</param>
        /// <exception cref="System.ArgumentNullException">When <paramref name="documentStore"/> is null.</exception>
        public static void ClearCollection<T>(this IDocumentStore documentStore)
        {
            var indexName = documentStore.CreateTemporaryIndex<T>();

            try
            {
                documentStore.ClearCollection<T>(indexName);
            }
            finally
            {
                documentStore.DeleteIndex(indexName);
            }
        }

        /// <summary>
        /// Deletes the collection for <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The document type of the collection to delete.</typeparam>
        /// <param name="documentStore">The document store to remove collection from.</param>
        /// <param name="indexName">The index to use for deleting the collection.</param>
        /// <remarks>
        /// <see cref="ClearCollection<T>"/> requires the index to be fresh. This method will
        /// call <see cref="WaitForNonStaleResults"/> but if there is any doubt the index
        /// will still be stale you should ensure the index is fresh before calling this method.
        /// </remarks>
        public static void ClearCollection<T>(this IDocumentStore documentStore, string indexName)
        {
            documentStore.MustNotBeNull("documentStore");
            indexName.MustNotBeNullOrWhiteSpace("indexName");

            // Wait for indexing to complete. This method will probably only be used in unit tests so shouldn't be a problem.
            documentStore.WaitForNonStaleResults(indexName);

            documentStore.DatabaseCommands
                .DeleteByIndex(
                    indexName,
                    new IndexQuery(),
                    allowStale: false
                );
        }

        /// <summary>
        /// Changes the Raven-Clr-Type meta data for a document.
        /// </summary>
        /// <param name="documentStore">The document store containing the document.</param>
        /// <param name="id">The id of document to change.</param>
        /// <param name="newRavenClrType">The value to change meta data to.</param>
        /// <remarks>
        /// This extension method is probably only useful in unit tests.
        /// </remarks>
        public static void ChangeRavenClrTypeForDocument(this IDocumentStore documentStore, string id, string newRavenClrType)
        {
            documentStore.MustNotBeNull("documentStore");
            id.MustNotBeNullOrWhiteSpace("id");
            newRavenClrType.MustNotBeNullOrWhiteSpace("newRavenClrType");

            var jsonDocument = documentStore.DatabaseCommands.Get(id);

            if (jsonDocument == null)
            {
                throw new Exception(string.Format("{0} cannot be found.", id));
            }

            // Change the metadata.
            jsonDocument.Metadata[RavenConstants.Metadata.RavenClrType] = newRavenClrType;

            // Save the changed metadata.
            documentStore.DatabaseCommands.Put(id, jsonDocument.Metadata.Value<Guid?>(RavenConstants.Metadata.etag), jsonDocument.DataAsJson, jsonDocument.Metadata);
        }

        /// <summary>
        /// Corrects metadata's Raven-Clr-Type for all <typeparamref name="T"/> documents.
        /// </summary>
        /// <typeparam name="T">
        /// The type of document to correct.
        /// </typeparam>
        /// <param name="documentStore">
        /// The <see cref="IDocumentStore"/> that contains <typeparamref name="T"/> documents.
        /// </param>
        /// <remarks>
        /// See CorrectRavenClrTypeForCollection<T>(this IDocumentStore documentStore, string indexName)
        /// for more details.
        /// </remarks>
        public static void CorrectRavenClrTypeForCollection<T>(this IDocumentStore documentStore)
        {
            documentStore.MustNotBeNull("documentStore");

            var indexName = CreateTemporaryIndex<T>(documentStore);

            try
            {
                documentStore.CorrectRavenClrTypeForCollection<T>(indexName);
            }
            finally
            {
                documentStore.DeleteIndex(indexName);
            }
        }

        /// <summary>
        /// Corrects metadata's Raven-Clr-Type for all <typeparamref name="T"/> documents.
        /// </summary>
        /// <typeparam name="T">
        /// The type of document to correct.
        /// </typeparam>
        /// <param name="documentStore">
        /// The <see cref="IDocumentStore"/> that contains <typeparamref name="T"/> documents.
        /// </param>
        /// <param name="indexName">
        /// The index to be used by the operation.
        /// </param>
        /// <remarks>
        /// RavenDB stores metadata with every document. One of those values is Raven-Clr-Type.
        /// 
        /// Raven-Clr-Type is the .Net type of the document at time of storing. The format is 
        /// type(T).FullName, assembly name. e.g. CroquetScores.Models.Competitions.Block, CroquetScores.Models.
        /// 
        /// I created this extension method because I moved a collection of documents from one
        /// assembly to another. The type remained exactly the same just the assembly changed.
        /// RavenDB copes with this fine until you attempt to IDocumentStore.Load() abstract class.
        /// See https://groups.google.com/forum/#!topic/ravendb/6CzbTidXEb0 for discussion why 
        /// RavenDB cannot handle this situation.
        /// 
        /// The solution is to correct Raven-Clr-Type for affected documents.
        /// </remarks>
        public static void CorrectRavenClrTypeForCollection<T>(this IDocumentStore documentStore, string indexName)
        {
            documentStore.MustNotBeNull("documentStore");
            indexName.MustNotBeNullOrWhiteSpace("indexName");

            // The call to documentStore.DatabaseCommands.UpdateByIndex() expects the index will not be stale.
            documentStore.WaitForNonStaleResults(indexName);

            var newRavenClrType = string.Format("{0}, {1}", typeof(T).FullName, typeof(T).Assembly.GetName().Name);

            documentStore.DatabaseCommands.UpdateByIndex(
                indexName,
                new IndexQuery(),
                new[]
                {
                    new PatchRequest()
                    {
                        Type = PatchCommandType.Modify,
                        Name="@metadata",
                        Nested = new []
                        {
                            new PatchRequest()
                            {
                                Type = PatchCommandType.Set,
                                Name = RavenConstants.Metadata.RavenClrType,
                                Value = new RavenJValue(newRavenClrType)
                            }
                        }
                    }
                }
            );
        }

        /// <summary>
        /// Non-generic version of <see cref="CorrectRavenClrTypeForCollection"/>. See its' documentation for more details.
        /// </summary>
        public static void CorrectRavenClrTypeForCollection(this IDocumentStore documentStore, Type documentType)
        {
            documentStore.MustNotBeNull("documentStore");
            documentType.MustNotBeNull("documentType");

            var method = typeof(IDocumentStoreExtensions).GetMethods().Single(m => m.Name == "CorrectRavenClrTypeForCollection" && m.GetParameters().Count() == 1);
            var generic = method.MakeGenericMethod(documentType);

            generic.Invoke(null, new object[] { documentStore });
        }

        /// <summary>
        /// Create an index that allows to tag entities by their entity name.
        /// </summary>
        public static void CreateDocumentsByEntityNameIndex(this IDocumentStore documentStore, bool waitForNonStaleResults = true)
        {
            documentStore.MustNotBeNull("documentStore");

            var documentsIndex = new RavenDocumentsByEntityName();

            documentStore.ExecuteIndex(documentsIndex);

            if (waitForNonStaleResults)
            {
                documentStore.WaitForNonStaleResults(RavenConstants.Indexes.DocumentsByEntityName);
            }
        }

        /// <summary>
        /// Creates a temporary index for a collection.
        /// </summary>
        /// <typeparam name="T">The document type to create the index for.</typeparam>
        /// <param name="documentStore">The document store to create the index for.</param>
        /// <returns>
        /// The name of the created index.
        /// </returns>
        public static string CreateTemporaryIndex<T>(this IDocumentStore documentStore)
        {
            documentStore.MustNotBeNull("documentStore");

            var indexName = Guid.NewGuid().ToString();

            documentStore.DatabaseCommands.PutIndex(indexName, new IndexDefinitionBuilder<T>
            {
                Map = documents => documents.Select(entity => new { })
            });

            return indexName;
        }

        /// <summary>
        /// Creates a temporary index for nominated document type.
        /// </summary>
        /// <param name="documentStore">The document store to create the index for.</param>
        /// <param name="documentType">The document type to create the index for.</param>
        /// <returns>
        /// The name of the created index.
        /// </returns>
        public static string CreateTemporaryIndex(this IDocumentStore documentStore, Type documentType)
        {
            documentStore.MustNotBeNull("documentStore");
            documentType.MustNotBeNull("documentType");

            var method = typeof(IDocumentStoreExtensions).GetMethods().Single(m => m.Name == "CreateTemporaryIndex" && m.GetParameters().Count() == 1);
            var generic = method.MakeGenericMethod(documentType);

            var indexName = generic.Invoke(null, new object[] { documentStore });

            return indexName.ToString();
        }

        /// <summary>
        /// Deletes the specified index.
        /// </summary>
        /// <param name="documentStore">The document store that contains the index.</param>
        /// <param name="indexName">Name of the index to delete.</param>
        public static void DeleteIndex(this IDocumentStore documentStore, string indexName)
        {
            documentStore.MustNotBeNull("documentStore");
            indexName.MustNotBeNullOrWhiteSpace("indexName");

            documentStore.DatabaseCommands.DeleteIndex(indexName);
        }

        /// <summary>
        /// Determines if an index is stale.
        /// </summary>
        /// <param name="documentStore">The document store that contains the index.</param>
        /// <param name="indexName">Name of the index to test.</param>
        public static bool IsIndexStale(this IDocumentStore documentStore, string indexName)
        {
            documentStore.MustNotBeNull("documentStore");
            indexName.MustNotBeNullOrWhiteSpace("indexName");

            using (var session = documentStore.OpenSession())
            {
                return session.IsIndexStale(indexName);
            }
        }

        /// <summary>
        /// Query <see cref="IDocumentStore"/> for the collections it holds.
        /// </summary>
        /// <param name="documentStore">The document store to query.</param>
        /// <exception cref="System.ArgumentNullException">When <paramref name="documentStore"/> is null.</exception>
        /// <remarks>
        /// Method does not support more than 1024 collections. If you have more than 1024 collections you don't need this library :-)
        /// </remarks>
        public static IEnumerable<string> QueryCollections(this IDocumentStore documentStore)
        {
            documentStore.MustNotBeNull("documentStore");

            documentStore.CreateDocumentsByEntityNameIndex();

            // Get the collection names.
            var collections = documentStore.DatabaseCommands.GetTerms(RavenConstants.Indexes.DocumentsByEntityName, "Tag", "", 1024);

            return collections;
        }

        /// <summary>
        /// Waits for <see cref="indexName"/> to be up to date.
        /// </summary>
        /// <param name="documentStore">The document store that contains the index.</param>
        /// <param name="indexName">Name of the index to get up to date.</param>
        /// <param name="maximumAttempts">The maximum number of attempts at waiting for index to be up to date.</param>
        public static void WaitForNonStaleResults(this IDocumentStore documentStore, string indexName, int maximumAttempts = 1)
        {
            documentStore.MustNotBeNull("documentStore");
            indexName.MustNotBeNullOrWhiteSpace("indexName");
            maximumAttempts.MustBeGreaterThan(0, "maximumAttempts");

            using (IDocumentSession session = documentStore.OpenSession())
            {
                session.WaitForNonStaleResults(indexName, maximumAttempts);
            }
        }
    }
}
