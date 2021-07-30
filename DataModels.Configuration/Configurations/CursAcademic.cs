using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DM = DataModels.Models;

namespace DataModels.Configuration.Configurations
{
    public class CursAcademic : IEntityTypeConfiguration<DM.CursAcademic>
    {
        public void Configure(EntityTypeBuilder<DM.CursAcademic> builder)
        {
        }
    }
}
