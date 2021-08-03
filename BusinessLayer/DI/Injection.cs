using BusinessLayer.Abstract.Services;
using BusinessLayer.Common;
using BusinessLayer.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessLayer.DI
{
    public static class Injection
    {
        public static IServiceCollection BusinessLayerConfigureServices(this IServiceCollection services)
        {
            // Services (ToDo: per comprensió)

            // centres
            services.AddTransient<ICentres, Centres>();
            services.AddTransient<ICentreCreate, CentreCreate>();
            services.AddTransient<ICentreUpdate, CentreUpdate>();
   
            // alumnes
            services.AddTransient<IAlumnes, Alumnes>();
            services.AddTransient<IAlumneCreate, AlumneCreate>();

            // curs acadèmic
            services.AddTransient<ICursAcademicCreate, CursAcademicCreate>();

            return services;
        }        
    }
}