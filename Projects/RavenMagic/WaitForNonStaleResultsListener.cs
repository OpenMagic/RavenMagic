using System;
using OpenMagic;
using Raven.Client;
using Raven.Client.Listeners;

namespace RavenMagic
{
    /// <summary>
    /// Customizes all queries with <see cref="IDocumentQueryCustomization.WaitForNonStaleResults"/>.
    /// </summary>
    public class WaitForNonStaleResultsListener : IDocumentQueryListener
    {
        public TimeSpan? WaitTimeout;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaitForNonStaleResultsListener"/> class.
        /// </summary>
        public WaitForNonStaleResultsListener()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WaitForNonStaleResultsListener"/> class.
        /// </summary>
        /// <param name="waitTimeout">The wait timeout.</param>
        public WaitForNonStaleResultsListener(TimeSpan? waitTimeout)
        {
            this.WaitTimeout = waitTimeout;
        }

        /// <summary>
        /// Customizes the query to wait for non stale results.
        /// </summary>
        public void BeforeQueryExecuted(IDocumentQueryCustomization queryCustomization)
        {
            if (WaitTimeout.HasValue)
            {
                queryCustomization.WaitForNonStaleResults(WaitTimeout.Value);
            }
            else
            {
                queryCustomization.WaitForNonStaleResults();
            }
        }
    }
}
