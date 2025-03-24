using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Services;
using Shared.Logging;

namespace ProductManagement.API.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IProductService, ProductService>();
            return services;
        }

        public static IServiceCollection AddLoggingServices(this IServiceCollection services)
        {
            services.AddScoped(typeof(ILoggerService<>), typeof(LoggerService<>));
            return services;
        }
    }
}