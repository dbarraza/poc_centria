using Backend.Common.Interfaces;
using Backend.Common.Interfaces.DataAccess;
using Backend.Common.Interfaces.Services;
using Backend.Common.Providers;
using Backend.Service.BusinessLogic;
using Backend.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Functions.Configuration
{
    /// <summary>
    /// Static class providing extension methods for configuring services in the application.
    /// Allows for registering service interface implementations into the ASP.NET Core service container.
    /// </summary>
    public static class ServiceConfiguration
    {
        /// <summary>
        /// Extension method for IServiceCollection that registers service implementations into the service container.
        /// </summary>
        /// <param name="services">The collection of services where implementations will be registered.</param>
        /// <returns>The collection of services with the registered implementations.</returns>
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            // Services
            services.AddScoped<ISessionProvider, SessionProvider>();
            services.AddScoped<IExcelService, ExcelService>();
            services.AddScoped<IApplicationService, ApplicationService>();
            services.AddScoped<ICvService, CvService>();
            services.AddScoped<IFileStorage, FileStorage>();

            // Repositories
            services.AddScoped<IDataAccess, DataAccess.DataAccess>();

            return services;
        }
    }

}
