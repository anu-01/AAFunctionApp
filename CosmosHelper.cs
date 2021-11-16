using Microsoft.Azure.Cosmos;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AAFunctionApp
{
    public class CosmosHelper
    {
        private static readonly string EndpointUri = "https://ratingsdb.documents.azure.com:443/";
        // The primary key for the Azure Cosmos account.
        private static readonly string PrimaryKey = "hJoimnN2tSwcFIIQOgUpFN2Ttu7G87EpMye7kluJcIpW3YaiuliBJAroNLCp6EM1ZHP5Nz4ECmBIVA6spCQNIQ==";

        // The Cosmos client instance
        private CosmosClient cosmosClient;

        // The database we will create
        private Database database;

        // The container we will create.
        private Container container;

        // The name of the database and container we will create
        private string databaseId = "ratingsdb";
        private string containerId = "items";

        public async Task GetStartedDemoAsync()
        {
            // Create a new instance of the Cosmos Client
            this.cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
            this.database = await this.cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
            Console.WriteLine("Created Database: {0}\n", this.database.Id);
            this.container = await this.database.CreateContainerIfNotExistsAsync(containerId, "/userId");
            Console.WriteLine("Created Container: {0}\n", this.container.Id);
        }

        public async Task AddItemsToContainerAsync(Rating rating)
        {

            ItemResponse<Rating> response = await this.container.CreateItemAsync<Rating>(rating, new PartitionKey(rating.userId));
            Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", response.Resource.id, response.RequestCharge);

        }
        public async Task <List<Rating>> QueryItemsAsync(string query)
        {
           // var sqlQueryText = "SELECT * FROM c WHERE c.LastName = 'Andersen'";

            Console.WriteLine("Running query: {0}\n", query);

            QueryDefinition queryDefinition = new QueryDefinition(query);
            FeedIterator<Rating> queryResultSetIterator = this.container.GetItemQueryIterator<Rating>(queryDefinition);

            List<Rating> ratings = new List<Rating>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Rating> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (Rating rating in currentResultSet)
                {
                    ratings.Add(rating);
                    Console.WriteLine("\tRead {0}\n", rating);
                }
            }

            return ratings;
        }
    }
}
