﻿using System;
using System.Linq;
using OpenMagic;
using Raven.Client;

namespace RavenMagic
{
    public static class IDocumentSessionExtensions
    {
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
            indexName.MustNotBeNullOrWhiteSpace("indexName");

            if (!documentSession.IsIndexStale(indexName))
            {
                return;
            }

            documentSession
                        .Query<object>(indexName)
                        .Customize(x => x.WaitForNonStaleResults(waitTimeout))
                        .Any();
        }
    }
}
