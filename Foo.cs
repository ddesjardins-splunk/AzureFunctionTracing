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
using SignalFx.Tracing.OpenTracing;
using OpenTracing.Propagation;
//using OpenTracing.Util;
using System.Collections.Generic;


namespace Shabuhabs.Function
{
    public static class Foo
    {



         // All configurations for the SignalFxTracer are being captured from environment
        // variables defined in local.settings.json. This static is just used to trigger
        // its initialization. It is recommended that the instance is used in order to
        // avoid its initialization to be optimized away (which will result in the
        // OpenTracing.Util.GlobalTracer.Instance being a no-op ITracer).
        private static readonly OpenTracing.ITracer SignalFxTracer = OpenTracingTracerFactory.WrapTracer(
            SignalFx.Tracing.Tracer.Instance
        );

        [FunctionName("Foo")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {

            OpenTracing.ITracer tracer = SignalFxTracer;
           
            var startTime = DateTimeOffset.Now;
            
            var headerDictionary = new Dictionary<string, string>();
            var headerKeys =req.Headers.Keys;
            foreach (var headerKey in headerKeys)
            {
                string headerValue = req.Headers[headerKey];
                headerDictionary.Add(headerKey, headerValue);
            }

        
            OpenTracing.ISpanBuilder spanBuilder = tracer.BuildSpan($"{req.Method} {req.HttpContext.Request.Path}");
            var requestContext = tracer.Extract(BuiltinFormats.HttpHeaders, new TextMapExtractAdapter(headerDictionary));
            using var scope = spanBuilder
                            .AsChildOf(requestContext)
                            .WithStartTimestamp(startTime)
                            .WithTag("span.kind", "server")
                            .StartActive();
            
            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name ??= data?.name;
         
            log.LogInformation("C# HTTP trigger function processed a request.");

            scope.Span.SetTag("query.name", name ?? "<null>");

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
                scope.Span.SetTag("error.message", e.Message);
                throw;
            }
            finally
            {
                scope.Span.SetTag("http.status_code", result?.StatusCode ?? 500);
                scope.Span.SetTag("error", true);
            }

            scope.Span.Finish();
            return result;

        }
    }
}
