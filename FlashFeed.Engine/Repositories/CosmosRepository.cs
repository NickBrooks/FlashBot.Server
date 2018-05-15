using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FlashFeed.Engine.Repositories
{
    public class CosmosRepository<T> where T : class
    {
        private static readonly string Endpoint = Environment.GetEnvironmentVariable("COSMOSDB_ENDPOINT");
        private static readonly string AuthKey = Environment.GetEnvironmentVariable("COSMOSDB_AUTHKEY");
        private static readonly string Database = "FlashFeed";
        private static readonly string Collection = "FlashFeed";
        private static DocumentClient client = new DocumentClient(new Uri(Endpoint), AuthKey, new ConnectionPolicy { EnableEndpointDiscovery = false });

        internal static async Task<T> GetItemByIdAsync(string id)
        {
            try
            {
                Document document = await client.ReadDocumentAsync(UriFactory.CreateDocumentUri(Database, Collection, id));
                return (T)(dynamic)document;
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }

        internal static async Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate)
        {
            IDocumentQuery<T> query = client.CreateDocumentQuery<T>(
                UriFactory.CreateDocumentCollectionUri(Database, Collection),
                new FeedOptions { MaxItemCount = -1 })
                .Where(predicate)
                .AsDocumentQuery();

            List<T> results = new List<T>();
            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<T>());
            }

            return results;
        }

        internal static async Task<List<T>> GetItemsSqlAsync(string queryString, int maxItemCount = 30)
        {
            IDocumentQuery<T> query = client.CreateDocumentQuery<T>(
                UriFactory.CreateDocumentCollectionUri(Database, Collection),
                queryString,
                new FeedOptions { MaxItemCount = maxItemCount })
                .AsDocumentQuery();

            List<T> results = new List<T>();
            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<T>());
            }

            return results.ToList();
        }

        internal static async Task<Document> CreateItemAsync(T item)
        {
            return await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(Database, Collection), item);
        }

        internal static async Task<Document> UpsertItemAsync(string id, T item)
        {
            if (await GetItemByIdAsync(id) == null)
            {
                return await CreateItemAsync(item);
            }
            else
            {
                return await UpdateItemAsync(id, item);
            }
        }

        internal static async Task<Document> UpdateItemAsync(string id, T item)
        {
            return await client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(Database, Collection, id), item);
        }

        internal static async void DeleteItemAsync(string id)
        {
            await client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(Database, Collection, id));
        }

        internal static void Initialize()
        {
            client = new DocumentClient(new Uri(Endpoint), AuthKey, new ConnectionPolicy { EnableEndpointDiscovery = false });
            CreateDatabaseIfNotExistsAsync().Wait();
            CreateCollectionIfNotExistsAsync().Wait();
        }

        private static async Task CreateDatabaseIfNotExistsAsync()
        {
            try
            {
                await client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(Database));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await client.CreateDatabaseAsync(new Database { Id = Database });
                }
                else
                {
                    throw;
                }
            }
        }

        private static async Task CreateCollectionIfNotExistsAsync()
        {
            try
            {
                await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(Database, Collection));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(Database),
                        new DocumentCollection { Id = Collection },
                        new RequestOptions { OfferThroughput = 1000 });
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
