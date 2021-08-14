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
            services.AddTransient<ICentreGetSet, CentreGetSet>();
            services.AddTransient<ICentreCreate, CentreCreate>();
            services.AddTransient<ICentreUpdate, CentreUpdate>();
            services.AddTransient<ICentreActivaDesactiva, CentreActivaDesactiva>();

            // Etapas
            services.AddTransient<IEtapaGetSet, EtapaGetSet>();
            services.AddTransient<IEtapaCreate, EtapaCreate>();
            services.AddTransient<IEtapaUpdate, EtapaUpdate>();
            services.AddTransient<IEtapaActivaDesactiva, EtapaActivaDesactiva>();
   
            // alumnes
            services.AddTransient<IAlumneGetSet, AlumneGetSet>();
            services.AddTransient<IAlumneCreate, AlumneCreate>();

            // curs acadèmic
            services.AddTransient<ICursAcademicCreate, CursAcademicCreate>();
            services.AddTransient<ICursAcademicGetSet, CursAcademicGetSet>();
            services.AddTransient<ICursAcademicActivaDesactiva, CursAcademicActivaDesactiva>();
            

            return services;
        }        
    }
}