using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Flurl.Http;

namespace AAFunctionApp
{
    public static class CreateRating
    {

        [FunctionName("CreateRating")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

   
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            if (await GetUser(data.userId.Value) && await GetProduct(data.productId.Value) && data.rating >= 5)
            {
                Rating rating = new Rating();
                rating.id = Guid.NewGuid().ToString();
                rating.timestamp = DateTime.Now.ToString();
                rating.userId = data.userId;
                rating.productId = data.productId;
                rating.rating = data.rating;
                rating.locationName = data.locationName;
                rating.userNotes = data.userNotes;
                CosmosHelper cosmos = new CosmosHelper();
                await cosmos.GetStartedDemoAsync();
                await cosmos.AddItemsToContainerAsync(rating);
                Console.WriteLine(await cosmos.QueryItemsAsync($"SELECT * FROM c WHERE c.userId = '{data.userId}'"));
                var responseMessage = JsonConvert.SerializeObject(rating);
                return new OkObjectResult(responseMessage);
            }
            else
            {
                return new BadRequestObjectResult("Please provide the correct userid, productid and ratings within 0 to 5");
            }
        }

        private static async Task<bool> GetUser(string id)
        {
            try
            {
                var response = await $"https://serverlessohapi.azurewebsites.net/api/GetUser?userId={id}".GetAsync().ReceiveString();
                return response.ToString().Contains(id) ? true : false;
            }
            catch(Exception e)
            {
                return false;
            }
        }

        private static async Task<bool> GetProduct(string productId)
        {
            try
            {
                var response = await $"https://serverlessohapi.azurewebsites.net/api/GetProduct?productId={productId}".GetAsync().ReceiveString();
                return response.ToString().Contains(productId) ? true : false;
            }
            catch(Exception e)
            {
                return false;
            }
        }
    }
}
