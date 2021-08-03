using Microsoft.EntityFrameworkCore;
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

            var serviceProvider = services.BuildServiceProvider();

            serviceProvider
                .GetRequiredService<IDbContextFactory<AppDbContext>>()
                .CreateDbContext()
                .Database
                .Migrate();

            return services;
        }        
    }
}