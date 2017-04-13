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



        public async Task<ActionResult> Index()
        {
            var client = new DocumentClient(new Uri(EndpointUri), AccessKey);
            var collection = await GetCollection(client);
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
            using (var client = new DocumentClient(new Uri(EndpointUri), AccessKey))
            {
                var collection = await GetCollection(client);
                ResourceResponse<Document> createdItem = await client.CreateDocumentAsync(collection.DocumentsLink, movie);

                return RedirectToAction("Index");
            }
        }

        public async Task<ActionResult> MarkWatched()
        {
            using (var client = new DocumentClient(new Uri(EndpointUri), AccessKey))
            {
                var collection = await GetCollection(client);

                string storedProceduresLink = collection.StoredProceduresLink;
                const string StoredProcedureName = "MarkWatched";

                StoredProcedure storedProcedure = client.CreateStoredProcedureQuery(storedProceduresLink)
                    .Where(sp => sp.Id == StoredProcedureName)
                    .AsEnumerable()
                    .FirstOrDefault();

                if(storedProcedure == null)
                {
                    storedProcedure = new StoredProcedure
                    {
                        Id = StoredProcedureName,
                        Body = System.IO.File.ReadAllText(Server.MapPath(@"..\Stored Procedures\MarkWatched.js"))
                    };
                    storedProcedure = await client.CreateStoredProcedureAsync(storedProceduresLink, storedProcedure);
                }
                await client.ExecuteStoredProcedureAsync<dynamic>(storedProcedure.SelfLink);

                return RedirectToAction("Index");
            }
        }


        //////////////////////////////////

        private async Task<DocumentCollection> GetCollection(DocumentClient client)
        {
            Database database = client.CreateDatabaseQuery()
                .Where(db => db.Id == DatabaseName)
                .AsEnumerable()
                .FirstOrDefault();

            if (database == null)
            {
                database = await client.CreateDatabaseAsync(new Database { Id = DatabaseName });
            }

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
    }
}