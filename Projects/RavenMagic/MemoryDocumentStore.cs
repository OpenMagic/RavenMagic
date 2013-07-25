using Raven.Client.Embedded;
using System;

namespace RavenMagic
{
    /// <summary>
    /// Creates a RavenDB database that runs in memory.
    /// </summary>
    public class MemoryDocumentStore : EmbeddableDocumentStore
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryDocumentStore"/> class.
        /// </summary>
        /// <remarks>
        /// This class if perfect for unit tests. New MemoryDocumentStore() will give you a
        /// document store that runs in memory, never returns stale results & is initialized.
        /// </remarks>
        /// <param name="waitForNonStaleResults">
        /// if set to <c>true</c> all queries wait for non stale results. Default is <c>true</c>.
        /// </param>
        /// <param name="initialize">
        /// if set to <c>true</c> the document store is initialized. Default is <c>true</c>.
        /// </param>
        /// <param name="createDocumentsByEntityNameIndex">
        /// if set to <c>true</c> Raven/DocumentsByEntityName index is created. 
        /// <see cref="Raven.Client.Embedded.EmbeddableDocumentStore">EmbeddableDocumentStore's</see> default 
        /// is to not create this index. However, I some methods require it and I can't see a real cost to 
        /// creating it by default. Default is <c>true</c>.
        /// </param>
        /// <exception cref="ArgumentException">
        /// initialize cannot be false when createDocumentsByEntityNameIndex is true.
        /// </exception>
        public MemoryDocumentStore(
            bool waitForNonStaleResults = true,
            bool initialize = true,
            bool createDocumentsByEntityNameIndex = true)
            : base()
        {
            if (!initialize && createDocumentsByEntityNameIndex)
            {
                throw new ArgumentException("initialize cannot be false when createDocumentsByEntityNameIndex is true.");
            }

            this.RunInMemory = true;

            if (waitForNonStaleResults)
            {
                this.RegisterListener(new WaitForNonStaleResultsListener());
            }

            if (initialize)
            {
                this.Initialize();
            }

            if (createDocumentsByEntityNameIndex)
            {
                this.CreateDocumentsByEntityNameIndex();
            }
        }
    }
}
