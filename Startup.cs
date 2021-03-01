
using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Configuration;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Context.Propagation;

//using Grpc.Core;
//using System.Diagnostics;



[assembly: FunctionsStartup(typeof(Shabuhabs.AzureFunctions.Startup))]

namespace Shabuhabs.AzureFunctions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {

         // AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

             var openTelemetry = Sdk.CreateTracerProviderBuilder()
                .AddSource("Shabuhabs.AzureFunctions")
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Foo"))
                .AddHttpClientInstrumentation()
                .AddConsoleExporter()
                .Build();

            builder.Services.AddSingleton(openTelemetry);
        }
    }
    
}

