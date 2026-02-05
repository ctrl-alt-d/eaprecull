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

            // centre
            services.AddTransient<ICentreSet, CentreSet>();
            services.AddTransient<ICentreCreate, CentreCreate>();
            services.AddTransient<ICentreUpdate, CentreUpdate>();
            services.AddTransient<ICentreActivaDesactiva, CentreActivaDesactiva>();

            // TipusActuacio
            services.AddTransient<ITipusActuacioSet, TipusActuacioSet>();
            services.AddTransient<ITipusActuacioCreate, TipusActuacioCreate>();
            services.AddTransient<ITipusActuacioUpdate, TipusActuacioUpdate>();
            services.AddTransient<ITipusActuacioActivaDesactiva, TipusActuacioActivaDesactiva>();

            // Etapa
            services.AddTransient<IEtapaSet, EtapaSet>();
            services.AddTransient<IEtapaCreate, EtapaCreate>();
            services.AddTransient<IEtapaUpdate, EtapaUpdate>();
            services.AddTransient<IEtapaActivaDesactiva, EtapaActivaDesactiva>();

            // alumnes
            services.AddTransient<IAlumneSet, AlumneSet>();
            services.AddTransient<IAlumneSyncActiuByCentre, AlumneSyncActiuByCentre>();
            services.AddTransient<IAlumneCreate, AlumneCreate>();
            services.AddTransient<IAlumneUpdate, AlumneUpdate>();
            services.AddTransient<IAlumneActivaDesactiva, AlumneActivaDesactiva>();

            // alumnes - reports
            services.AddTransient<IAlumneInforme, AlumneInforme>();

            // curs acadèmic
            services.AddTransient<ICursAcademicCreate, CursAcademicCreate>();
            services.AddTransient<ICursAcademicUpdate, CursAcademicUpdate>();
            services.AddTransient<ICursAcademicSet, CursAcademicSet>();
            services.AddTransient<ICursAcademicSetAmbActuacions, CursAcademicSetAmbActuacions>();
            services.AddTransient<ICursAcademicActivaDesactiva, CursAcademicActivaDesactiva>();

            // actuacio
            services.AddTransient<IActuacioSet, ActuacioSet>();
            services.AddTransient<IActuacioCreate, ActuacioCreate>();
            services.AddTransient<IActuacioUpdate, ActuacioUpdate>();

            // altres
            services.AddTransient<IImportAll, ImportAll>();
            services.AddTransient<IPivotActuacions, PivotActuacions>();
            

            return services;
        }
    }
}