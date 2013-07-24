using Raven.Client.Embedded;

namespace RavenMagic
{
    /// <summary>
    /// Creates a RavenDB database that runs in memory.
    /// </summary>
    public static class MemoryDocumentStore
    {
        /// <summary>
        /// Creates a RavenDB database that runs in memory.
        /// </summary>
        /// <param name="waitForNonStaleResults">
        /// if set to <c>true</c> all queries wait for non stale results.
        /// </param>
        public static EmbeddableDocumentStore Create(bool waitForNonStaleResults)
        {
            var store = new EmbeddableDocumentStore() { RunInMemory = true };

            if (!waitForNonStaleResults)
            {
                store.RegisterListener(new WaitForNonStaleResultsListener());
            }

            return store;
        }

        /// <summary>
        /// Creates an initialized RavenDB database that runs in memory.
        /// </summary>
        /// <param name="waitForNonStaleResults">
        /// if set to <c>true</c> all queries wait for non stale results.
        /// </param>
        public static EmbeddableDocumentStore Initialize(bool waitForNonStaleResults)
        {
            return (EmbeddableDocumentStore)Create(waitForNonStaleResults).Initialize();
        }
    }
}
