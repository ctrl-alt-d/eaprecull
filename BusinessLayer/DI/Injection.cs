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
            services.AddTransient<ICentreSet, CentreSet>();
            services.AddTransient<ICentreCreate, CentreCreate>();
            services.AddTransient<ICentreUpdate, CentreUpdate>();
            services.AddTransient<ICentreActivaDesactiva, CentreActivaDesactiva>();

            // Etapas
            services.AddTransient<IEtapaSet, EtapaSet>();
            services.AddTransient<IEtapaCreate, EtapaCreate>();
            services.AddTransient<IEtapaUpdate, EtapaUpdate>();
            services.AddTransient<IEtapaActivaDesactiva, EtapaActivaDesactiva>();
   
            // alumnes
            services.AddTransient<IAlumneSet, AlumneSet>();
            services.AddTransient<IAlumneCreate, AlumneCreate>();

            // curs acadèmic
            services.AddTransient<ICursAcademicCreate, CursAcademicCreate>();
            services.AddTransient<ICursAcademicUpdate, CursAcademicUpdate>();
            services.AddTransient<ICursAcademicSet, CursAcademicSet>();
            services.AddTransient<ICursAcademicActivaDesactiva, CursAcademicActivaDesactiva>();
            

            return services;
        }        
    }
}