using BusinessLayer.Abstract.Services;
using BusinessLayer.Common;
using BusinessLayer.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessLayer.DI
{
    public static class Injection
    {
        public static void BusinessLayerConfigureServices(this IServiceCollection services)
        {
            // Services (ToDo: per comprensió)
            services.AddScoped<ICentres, Centres>();
            services.AddScoped<ICentreCreate, CentreCreate>();
        }        
    }
}