using BusinessLayer.Abstract.Services;
using BusinessLayer.Common;
using BusinessLayer.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessLayer.DI
{
    public static class Injection
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            // Basic operations
            services.AddScoped(typeof(BLGetItem<,>));
            services.AddScoped(typeof(BLGetItems<,>));

            // Services (ToDo: per comprensió)
            services.AddScoped<ICentres, Centres>();
        }        
    }
}