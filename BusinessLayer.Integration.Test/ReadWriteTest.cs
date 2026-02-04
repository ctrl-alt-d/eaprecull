using DataLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using BusinessLayer.DI;
using BusinessLayer.Abstract.Services;
using System.IO;
using System;
using System.Linq;

namespace BusinessLayer.Integration.Test
{
    public class ReadWriteTest
    {
        [Fact]
        public async void WriteTest()
        {
            // arrange
            var dataSource = Path.Combine(Path.GetTempPath(), "Esborrar" + Guid.NewGuid().ToString().Substring(4, 4) + ".db");
            var ConnectionString = $"Data Source={dataSource}";

            var services = new ServiceCollection();
            services.AddDbContextFactory<AppDbContext>(opt => 
                opt.UseSqlite(ConnectionString)
                   .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));
            services.BusinessLayerConfigureServices();

            var serviceProvider = services.BuildServiceProvider();

            var centreCreate = serviceProvider.GetRequiredService<ICentreCreate>();

            serviceProvider
                .GetRequiredService<IDbContextFactory<AppDbContext>>()
                .CreateDbContext()
                .Database
                .Migrate();


            // act
            var parms = new DTO.i.DTOs.CentreCreateParms(
                codi: "123",
                nom: "Pepe",
                esActiu: true
            );
            var result = await centreCreate.Create(parms);

            // assert
            var expected = parms.Nom;
            Assert.Equal(expected, result.Data.Nom);

        }


        [Fact]
        public async void ReadTest()
        {
            // arrange
            var dataSource = Path.Combine(Path.GetTempPath(), "Esborrar" + Guid.NewGuid().ToString().Substring(4, 4) + ".db");
            var ConnectionString = $"Data Source={dataSource}";

            var services = new ServiceCollection();
            services.AddDbContextFactory<AppDbContext>(opt => 
                opt.UseSqlite(ConnectionString)
                   .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));
            services.BusinessLayerConfigureServices();

            var serviceProvider = services.BuildServiceProvider();

            var centreCreate = serviceProvider.GetRequiredService<ICentreCreate>();
            var centres = serviceProvider.GetRequiredService<ICentreSet>();

            serviceProvider
                .GetRequiredService<IDbContextFactory<AppDbContext>>()
                .CreateDbContext()
                .Database
                .Migrate();


            // act
            var createparms = new DTO.i.DTOs.CentreCreateParms(
                codi: "123",
                nom: "Pepe",
                esActiu: true
            );
            var createresult = await centreCreate.Create(createparms);

            var parms = new DTO.i.DTOs.EsActiuParms(esActiu: true);
            var results = await centres.FromPredicate(parms);
            var result = results.Data.First();


            // assert
            var expected = createresult.Data.Nom;
            Assert.Equal(expected, result.Nom);

        }



    }
}
