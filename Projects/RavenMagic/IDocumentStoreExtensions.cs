using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Client;
using OpenMagic;
using Raven.Client.Indexes;
using Raven.Client.Document;

namespace RavenMagic
{
    public static class IDocumentStoreExtensions
    {
        private static void CreateDocumentsByEntityNameIndex(this IDocumentStore documentStore, bool waitForNonStaleResults)
        {
            var documentsIndex = new RavenDocumentsByEntityName();

            // Ensure the required index exists.
            documentStore.ExecuteIndex(documentsIndex);

            // Wait for indexing to finish.
            if (waitForNonStaleResults)
            {
                using (IDocumentSession session = documentStore.OpenSession())
                {
                    var query = session.Query<object>(documentsIndex.IndexName).Customize(x => x.WaitForNonStaleResults());
                    var result = query.FirstOrDefault();
                }
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
            var collections = documentStore.DatabaseCommands.GetTerms("Raven/DocumentsByEntityName", "Tag", "", 1024);

            return collections;
        }
    }
}
