using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Movies.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Movies.Controllers
{
    public class MovieController : Controller
    {
        private static readonly string DatabaseName = ConfigurationManager.AppSettings["DatabaseId"];
        private static readonly string EndpointUri = ConfigurationManager.AppSettings["EndpointUri"];
        private static readonly string AccessKey = ConfigurationManager.AppSettings["AccessKey"];
        private static readonly string CollectionName = "movies";

        private async Task<Database> GetDatabaseAsync(DocumentClient client)
        {
            Database database = client.CreateDatabaseQuery()
                .Where(db => db.Id == DatabaseName)
                .AsEnumerable()
                .FirstOrDefault();

            if (database == null)
            {
                database = await client.CreateDatabaseAsync(new Database { Id = DatabaseName });
            }

            return database;
        }

        private async Task<DocumentCollection> GetCollectionAsync(DocumentClient client, Database database)
        {
            DocumentCollection collection = client.CreateDocumentCollectionQuery(database.CollectionsLink)
                .Where(c => c.Id == CollectionName)
                .AsEnumerable()
                .FirstOrDefault();

            if (collection == null)
            {
                collection = await client.CreateDocumentCollectionAsync(database.CollectionsLink, new DocumentCollection { Id = CollectionName });
            }

            return collection;
        }



        public async Task<ActionResult> Index()
        {
            var client = new DocumentClient(new Uri(EndpointUri), AccessKey);
            Database database = await GetDatabaseAsync(client);
            DocumentCollection collection = await GetCollectionAsync(client, database);
            IEnumerable<Movie> documents = client.CreateDocumentQuery<Movie>(collection.DocumentsLink).AsEnumerable();

            return View(documents);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(Movie movie)
        {
            var client = new DocumentClient(new Uri(EndpointUri), AccessKey);
            Database database = await GetDatabaseAsync(client);
            DocumentCollection collection = await GetCollectionAsync(client, database);
            ResourceResponse<Document> createdItem = await client.CreateDocumentAsync(collection.DocumentsLink, movie);

            return View();
        }
    }
}