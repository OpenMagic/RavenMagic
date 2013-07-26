using System.Linq;
using OpenMagic;
using Raven.Client;

namespace RavenMagic
{
    public static class IDocumentSessionExtensions
    {
        /// <summary>
        /// Determines if an index is stale.
        /// </summary>
        /// <param name="documentSession">The document session that contains the index.</param>
        /// <param name="indexName">Name of the index to test.</param>
        public static bool IsIndexStale(this IDocumentSession documentSession, string indexName)
        {
            documentSession.MustNotBeNull("documentSession");
            indexName.MustNotBeNullOrWhiteSpace("indexName");

            RavenQueryStatistics stats = null;

            documentSession.Query<object>(indexName).Statistics(out stats).Any();

            return stats.IsStale;
        }
    }
}
