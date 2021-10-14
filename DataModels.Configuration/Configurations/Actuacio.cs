using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DM = DataModels.Models;

namespace DataModels.Configuration.Configurations
{
    public class Actuacio : IEntityTypeConfiguration<DM.Actuacio>
    {
        public void Configure(EntityTypeBuilder<DM.Actuacio> builder)
        {

            builder
                .HasOne(m => m.Alumne!)
                .WithMany(r => r.Actuacions);

            builder
                .HasOne(m => m.CursActuacio!)
                .WithMany(r => r.Actuacions);

            builder
                .HasOne(m => m.TipusActuacio!)
                .WithMany(r => r.Actuacions);

        }
    }
}
