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
            services.AddTransient<ICentres, Centres>();
            services.AddTransient<ICentreCreate, CentreCreate>();
            services.AddTransient<IAlumnes, Alumnes>();
            services.AddTransient<IAlumneCreate, AlumneCreate>();
        }        
    }
}