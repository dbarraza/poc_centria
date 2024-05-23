using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;

namespace Backend.Functions.Configuration
{
    /// <summary>
    /// Static class providing extension methods for configuring application settings.
    /// Allows for adding configuration from local settings file and environment variables, and optionally from Azure App Configuration.
    /// </summary>
    public static class AppConfiguration
    {
        /// <summary>
        /// Extension method for IHostBuilder that configures application settings.
        /// </summary>
        /// <param name="hostBuilder">The host builder to configure.</param>
        /// <returns>The configured host builder.</returns>
        public static IHostBuilder AddAppConfiguration(this IHostBuilder hostBuilder)
        {
            hostBuilder
                .ConfigureAppConfiguration(config => config
                    .SetBasePath(Environment.CurrentDirectory)
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables())
                .ConfigureAppConfiguration(config =>
                {
                    var connectionString = Environment.GetEnvironmentVariable("AppConfiguration");
                    if (!string.IsNullOrEmpty(connectionString))
                    {
                        config.AddAzureAppConfiguration(options =>
                        {
                            options.Connect(connectionString)
                                    .ConfigureKeyVault(kv =>
                                    {
                                        var clientId = Environment.GetEnvironmentVariable("ClientId");
                                        var clientSecret = Environment.GetEnvironmentVariable("ClientSecret");
                                        var directoryId = Environment.GetEnvironmentVariable("DirectoryId");
                                        kv.SetCredential(new ClientSecretCredential(directoryId, clientId, clientSecret));
                                    });
                        });
                    }
                });

            return hostBuilder;
        }
    }
}
