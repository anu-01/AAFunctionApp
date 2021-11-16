using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AAFunctionApp
{
    public static class GetRatings
    {
        [FunctionName("GetRatings")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string userId = req.Query["userId"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            // connect to cosmos db and fetch all the query string values matching userId
            CosmosHelper cosmos = new CosmosHelper();
            await cosmos.GetStartedDemoAsync();
            var responseMessage = await cosmos.QueryItemsAsync($"SELECT * FROM c WHERE c.userId = '{userId}'");

            return new OkObjectResult(responseMessage);
        }
    }
}
