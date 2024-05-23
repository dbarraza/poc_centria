using System;
using Backend.Common.Interfaces.DataAccess;
using Backend.Common.Interfaces.Services;
using Backend.Common.Interfaces;
using Backend.Common.Middleware;
using Backend.Common.Providers;
using Backend.Functions.Configuration;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.OpenApi.Models;
using Backend.Service.BusinessLogic;

namespace Backend.Functions
{
    /// <summary>
    /// Azure Function entry point
    /// </summary>
    public class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
              .ConfigureFunctionsWebApplication(builder =>
              {
                  builder.UseMiddleware<SessionMiddleware>();
                  builder.UseNewtonsoftJson();
              })
              .ConfigureOpenApi()
                .ConfigureAppConfiguration(builder =>
                {
                    var connectionString = Environment.GetEnvironmentVariable("AppConfiguration");
                    if (!string.IsNullOrEmpty(connectionString))
                    {
                        builder.AddAzureAppConfiguration(connectionString);
                    }
                })
                .ConfigureAppConfiguration(config => config
                  .SetBasePath(Environment.CurrentDirectory)
                   .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                  .AddEnvironmentVariables())
              .ConfigureServices(services =>
              {
                  services.Configure<KestrelServerOptions>(options =>
                  {
                      options.AllowSynchronousIO = true;
                  });
                  services.AddSingleton<IOpenApiConfigurationOptions>(_ =>
                  {
                      var options = new OpenApiConfigurationOptions()
                      {
                          Info = new OpenApiInfo()
                          {
                              Version = "1.0.0",
                              Title = "API",
                              Description = "API",
                              Contact = new OpenApiContact()
                              {
                                  Name = "Soporte",
                                  Email = "soporte@soporte.com",
                              },
                          },
                          ForceHttps = false,
                          ForceHttp = false,
                      };

                      return options;
                  });
                  services.AddOpenAPIConfigure();
                  services.AddServices();
                  services.AddScoped<ISessionProvider, SessionProvider>();
                  services.AddScoped<ICvService, CvService>();
                  services.AddScoped<IDataAccess, Backend.DataAccess.DataAccess>();
              })
              .Build();

            host.Run();
        }
    }
}