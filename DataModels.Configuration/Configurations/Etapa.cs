using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DM = DataModels.Models;

namespace DataModels.Configuration.Configurations
{
    public class Etapa : IEntityTypeConfiguration<DM.Etapa>
    {
        public void Configure(EntityTypeBuilder<DM.Etapa> builder)
        {
        }
    }
}
