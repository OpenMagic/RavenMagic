using System;
using System.Linq;
using Common.Logging;
using OpenMagic;
using Raven.Client;

namespace RavenMagic
{
    public static class IDocumentSessionExtensions
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
        public static TimeSpan DefaultWaitTimeout { get; set; }

        static IDocumentSessionExtensions()
        {
            DefaultWaitTimeout = TimeSpan.FromSeconds(15);
        }

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

        /// <summary>
        /// Waits for <see cref="indexName"/> to be up to date.
        /// </summary>
        /// <param name="documentSession">The document session that contains the index.</param>
        /// <param name="indexName">Name of the index to get up to date.</param>
        public static void WaitForNonStaleResults(this IDocumentSession documentSession, string indexName)
        {
            documentSession.MustNotBeNull("documentSession");
            indexName.MustNotBeNullOrWhiteSpace("indexName");

            documentSession.WaitForNonStaleResults(indexName, DefaultWaitTimeout);
        }

        /// <summary>
        /// Waits for <see cref="indexName"/> to be up to date.
        /// </summary>
        /// <param name="documentSession">The document session that contains the index.</param>
        /// <param name="indexName">Name of the index to get up to date.</param>
        /// <param name="waitTimeout">Maximum time to wait before throwing timeout exception.</param>
        public static void WaitForNonStaleResults(this IDocumentSession documentSession, string indexName, TimeSpan waitTimeout)
        {
            documentSession.MustNotBeNull("documentSession");
            indexName.MustNotBeNullOrWhiteSpace("indexName");
            waitTimeout.MustNotBeNull("waitTimeout");

            if (!documentSession.IsIndexStale(indexName))
            {
                Log.Debug(string.Format("{0} index is not stale.", indexName, waitTimeout));
                return;
            }

            Log.Debug(string.Format("Waiting {1} for {0} to have no stale results...", indexName, waitTimeout));

            documentSession
                        .Query<object>(indexName)
                        .Customize(x => x.WaitForNonStaleResults(waitTimeout))
                        .Any();

            if (documentSession.IsIndexStale(indexName))
            {
                Log.Debug(string.Format("After waiting {1} {0} index is still stale..", indexName, waitTimeout));
            }
            else
            {
                Log.Debug(string.Format("{0} index is now fresh.", indexName));
            }
        }
    }
}
