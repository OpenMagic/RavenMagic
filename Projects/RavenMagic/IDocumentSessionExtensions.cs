using System;
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

        /// <summary>
        /// Waits for <see cref="indexName"/> to be up to date.
        /// </summary>
        /// <param name="documentSession">The document session that contains the index.</param>
        /// <param name="indexName">Name of the index to get up to date.</param>
        /// <param name="maximumAttempts">The maximum number of attempts at waiting for index to be up to date.</param>
        public static void WaitForNonStaleResults(this IDocumentSession documentSession, string indexName, int maximumAttempts = 1)
        {
            documentSession.MustNotBeNull("documentSession");
            indexName.MustNotBeNullOrWhiteSpace("indexName");
            maximumAttempts.MustBeGreaterThan(0, "maximumAttempts");

            int attempt = 0;
            TimeoutException timeoutException = null;

            while (documentSession.IsIndexStale(indexName))
            {
                attempt += 1;

                if (attempt > maximumAttempts)
                {
                    if (maximumAttempts == 1)
                    {
                        throw timeoutException;
                    }
                    else
                    {
                        throw new TimeoutException(string.Format("After {0} attempts {1} index is still stale.", maximumAttempts, indexName), timeoutException);
                    }
                }

                try
                {
                    documentSession
                        .Query<object>(indexName)
                        .Customize(x => x.WaitForNonStaleResults())
                        .Any();
                }
                catch (TimeoutException ex)
                {
                    timeoutException = ex;
                }
            }
        }
    }
}
