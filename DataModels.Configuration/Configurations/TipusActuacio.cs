using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DM = DataModels.Models;

namespace DataModels.Configuration.Configurations
{
    public class TipusActuacio : IEntityTypeConfiguration<DM.TipusActuacio>
    {
        public void Configure(EntityTypeBuilder<DM.TipusActuacio> builder)
        {
        }
    }
}
