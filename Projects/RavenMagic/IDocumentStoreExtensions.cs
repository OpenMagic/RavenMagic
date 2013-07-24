using System;
using System.Collections.Generic;
using System.Linq;
using OpenMagic;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Indexes;

namespace RavenMagic
{
    public static class IDocumentStoreExtensions
    {
        public const string RavenDocumentsByEntityName_IndexName = "Raven/DocumentsByEntityName";

        /// <summary>
        /// Deletes the collection for <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The document type of the collection to delete.</typeparam>
        /// <param name="documentStore">The document store to remove collection from.</param>
        /// <exception cref="System.ArgumentNullException">When <paramref name="documentStore"/> is null.</exception>
        public static void ClearCollection<T>(this IDocumentStore documentStore)
        {
            documentStore.MustNotBeNull("documentStore");

            //var indexName = "ClearCollection_" + typeof(T).Name;
            var indexName = Guid.NewGuid().ToString();

            documentStore.DatabaseCommands.PutIndex(indexName, new IndexDefinitionBuilder<T>
            {
                Map = documents => documents.Select(entity => new { })
            });

            // Wait for indexing to complete. This method will probably only be used in unit tests so shouldn't be a problem.
            documentStore.WaitForNonStaleResults(indexName);

            documentStore
                .DatabaseCommands
                .DeleteByIndex(
                    indexName,
                    new IndexQuery(),
                    allowStale: false
                );
        }

        private static void CreateDocumentsByEntityNameIndex(this IDocumentStore documentStore, bool waitForNonStaleResults)
        {
            var documentsIndex = new RavenDocumentsByEntityName();

            // Ensure the required index exists.
            documentStore.ExecuteIndex(documentsIndex);

            // Wait for indexing to finish.
            if (waitForNonStaleResults)
            {
                documentStore.WaitForNonStaleResults(RavenDocumentsByEntityName_IndexName);
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

            documentStore.CreateDocumentsByEntityNameIndex(waitForNonStaleResults: true);

            // Get the collection names.
            var collections = documentStore.DatabaseCommands.GetTerms(RavenDocumentsByEntityName_IndexName, "Tag", "", 1024);

            return collections;
        }

        private static void WaitForNonStaleResults(this IDocumentStore documentStore, string indexName)
        {
            using (IDocumentSession session = documentStore.OpenSession())
            {
                session
                    .Query<object>(indexName)
                    .Customize(x => x.WaitForNonStaleResults())
                    .Any();
            }
        }
    }
}
