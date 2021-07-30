using System;
using Microsoft.EntityFrameworkCore;
using DataModels.Configuration;
using DataModels.Models;

namespace DataLayer
{
    public class AppDbContext : DbContext
    {
        
        public virtual DbSet<Actuacio> Actuacions  => Set<Actuacio>();
        public virtual DbSet<Alumne> Alumnes  => Set<Alumne>();
        public virtual DbSet<Centre> Centres  => Set<Centre>();
        public virtual DbSet<CursAcademic> CursosAcademics  => Set<CursAcademic>();
        public virtual DbSet<Etapa> Etapes  => Set<Etapa>();
        public virtual DbSet<TipusActuacio> TipusActuacions  => Set<TipusActuacio>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .ApplyConfigurationsFromAssembly(typeof(ConfigurationAssembly).Assembly);
        }

        public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
        {
        }

    }
}
