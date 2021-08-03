using Microsoft.Extensions.DependencyInjection;

namespace DataLayer.DI
{
    public static class Injection
    {
        public static IServiceCollection DataLayerConfigureServices(this IServiceCollection services)
        {
            services
            .AddDbContextFactory<AppDbContext>(
                options =>
                    options.ConfigureAppDbContext()
            );

            return services;
        }        
    }
}