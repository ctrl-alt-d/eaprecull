using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DM = DataModels.Models;

namespace DataModels.Configuration.Configurations
{
    public class Centre : IEntityTypeConfiguration<DM.Centre>
    {
        public void Configure(EntityTypeBuilder<DM.Centre> builder)
        {
        }
    }
}
