using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Services;

namespace ProductManagement.API.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IProductService, ProductService>();

            return services;
        }
    }
}