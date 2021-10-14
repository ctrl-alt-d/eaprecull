using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DM = DataModels.Models;

namespace DataModels.Configuration.Configurations
{
    public class Alumne : IEntityTypeConfiguration<DM.Alumne>
    {
        public void Configure(EntityTypeBuilder<DM.Alumne> builder)
        {
            builder
                .HasOne(m => m.CursDarreraActualitacioDades!)
                .WithMany(r => r.AlumnesActualitzats);

            builder
                .HasOne(m => m.EtapaActual!)
                .WithMany(r => r.Alumnes);

            builder
                .HasOne(m => m.CentreActual!)
                .WithMany(r => r.Alumnes);

        }
    }
}
