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
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;

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
            ObjectResult result = null;
        
            using (var activity = MyActivitySource.StartActivity("Foo", ActivityKind.Server, req.Headers["traceparent"] ))
            {
                if (activity.ParentId != null) 
                {
                    
                    Console.WriteLine("Parentid is" + activity.ParentId);
                }
             
                activity?.SetTag("enviroment", "Shabuhabs.AzureFunctions-central-sub1");
                activity?.SetTag("Fooz-Functionz", "OTel .NET Rocks!");
            
                var startTime = DateTimeOffset.Now;
                
                string name = req.Query["name"];

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                name ??= data?.name;
            
                log.LogInformation("Foo Function is Running . .. . HttpTrigger");
                
                activity?.SetTag("query.name", name ?? "no-name");

                string responseMessage = string.IsNullOrEmpty(name)
                    ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                    : $"Hello, {name}. This HTTP triggered function executed successfully.";
                
                try
                {

                    // Used to simulate errors only !! ( send in name=crash or name=slow)
                    if (name == "crash")
                    {
                    throw new ArgumentException("CRASHED, 'crash' is invalid arg!");
                    }else if(name == "slow") {
                        System.Threading.Thread.Sleep(4000);
                    }    

                    result = new OkObjectResult(responseMessage);
                    activity?.SetTag("http.status_code", 200);
                    activity?.SetTag("error", false);
                }
                catch (Exception e)
                {
                    result = new ExceptionResult(e, includeErrorDetail: true);
                    activity?.SetTag("error.message", e.Message);
                    activity?.SetTag("http.status_code", 500);
                    activity?.SetTag("error", true);
                    log.LogError("Foo Function FAIL!");
                }
                finally
                {
                
                }
            } 
            return result;
        }
    }
}
