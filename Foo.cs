using System;
using System.IO;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;


namespace Shabuhabs.AzureFunctions
{
    public static class Foo
    {

         private static readonly ActivitySource MyActivitySource = new ActivitySource("Shabuhabs.AzureFunctions");


        [FunctionName("Foo")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {

        using (var activity = MyActivitySource.StartActivity("Foo"))
        {
            activity?.SetTag("span.kind", "SERVER");
            activity?.SetTag("Foo-Fun", "OT .NET Rocks!");
        }
            var startTime = DateTimeOffset.Now;
            
            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name ??= data?.name;
         
            log.LogInformation("C# HTTP trigger function processed a request.");

          //  scope.Span.SetTag("query.name", name ?? "<null>");

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";
            
            ObjectResult result = null;
            try
            {
                if (name == "crash")
                {
                   throw new ArgumentException("CRASHED, 'crash' is invalid arg!");
                }

                result = new OkObjectResult(responseMessage);
            }
            catch (Exception e)
            {
                result = new ExceptionResult(e, includeErrorDetail: true);
              //  scope.Span.SetTag("error.message", e.Message);
                throw;
            }
            finally
            {
               // scope.Span.SetTag("http.status_code", result?.StatusCode ?? 500);
              //  scope.Span.SetTag("error", true);
            }

           // scope.Span.Finish();
            return result;

        }
    }
}
