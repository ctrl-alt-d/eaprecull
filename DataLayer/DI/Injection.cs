using Microsoft.Extensions.DependencyInjection;

namespace DataLayer.DI
{
    public static class Injection
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextFactory<AppDbContext>(
                options =>
                    options.ConfigureAppDbContext()
            );
        }        
    }
}