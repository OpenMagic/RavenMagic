using System;
using System.Linq;
using Raven.Client;
using Raven.Client.Indexes;
using RavenMagic.Tests.TestHelpers.Models;

namespace RavenMagic.Tests
{
    public class BaseTestClass
    {
        protected void StoreFakeProducts(IDocumentStore store, int count)
        {
            using (var session = store.OpenSession())
            {
                for (int i = 0; i < count; i++)
                {
                    session.Store(new Product());
                }
                session.SaveChanges();
            }
        }

        protected void CreateProductsIndex(MemoryDocumentStore store, string indexName)
        {
            store.DatabaseCommands.PutIndex(indexName, new IndexDefinitionBuilder<Product> { Map = documents => documents.Select(p => new { p.Name }) });
        }

        protected void ValidateIndexIsStale(MemoryDocumentStore store, string indexName)
        {
            if (store.IsIndexStale(indexName))
            {
                return;
            }

            throw new Exception("Test cannot continue because the index is not stale. Try adding more fake products to the collection.");
        }
    }
}
