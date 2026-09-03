using System;
using Microsoft.EntityFrameworkCore;
using DataModels.Configuration;
using DataModels.Models;
using DataLayer.Sql;

namespace DataLayer
{
    public class AppDbContext : DbContext
    {

        public virtual DbSet<Actuacio> Actuacions => Set<Actuacio>();
        public virtual DbSet<Alumne> Alumnes => Set<Alumne>();
        public virtual DbSet<Centre> Centres => Set<Centre>();
        public virtual DbSet<CursAcademic> CursosAcademics => Set<CursAcademic>();
        public virtual DbSet<Etapa> Etapes => Set<Etapa>();
        public virtual DbSet<TipusActuacio> TipusActuacions => Set<TipusActuacio>();



        /// <remarks>
        /// L'interceptor s'enganxa aquí, i no pas al costat del <c>UseSqlite</c>,
        /// perquè les funcions pròpies són una condició per fer anar el context:
        /// sense elles les cerques peten. Posat aquí el tenen totes les maneres
        /// de construir el context, tests inclosos, sense haver-se'n de recordar.
        /// </remarks>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.AddInterceptors(FuncionsSqliteInterceptor.Instancia);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .ApplyConfigurationsFromAssembly(typeof(ConfigurationAssembly).Assembly);

            modelBuilder
                .RegistraFuncionsSql();
        }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

    }
}
