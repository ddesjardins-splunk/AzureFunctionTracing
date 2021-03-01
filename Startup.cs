
using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.IO;
using System.Collections;



[assembly: FunctionsStartup(typeof(Shabuhabs.AzureFunctions.Startup))]

namespace Shabuhabs.AzureFunctions
{
    public class Startup : FunctionsStartup
    {

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        { 
            FunctionsHostBuilderContext context = builder.GetContext();

            builder.ConfigurationBuilder
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, "appsettings.json"), optional: true, reloadOnChange: false)
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, $"appsettings.{context.EnvironmentName}.json"), optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();

        }
        public override void Configure(IFunctionsHostBuilder builder)
        {

          // AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

             var openTelemetry = Sdk.CreateTracerProviderBuilder()
                .AddSource("Shabuhabs.AzureFunctions")
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Foo"))
              //  .AddHttpClientInstrumentation()
              .AddJaegerExporter(jaegerOptions =>
                        {
                            jaegerOptions.AgentHost = "192.168.0.110";
                            jaegerOptions.AgentPort =6831;
                        })
                .AddConsoleExporter()
                .Build();

            builder.Services.AddSingleton(openTelemetry);
        }
    }
    
}

