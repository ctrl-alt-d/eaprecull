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
using System.Threading.Tasks;

namespace BusinessLayer.Integration.Test
{
    public class ReadWriteTest
    {
        [Fact]
        public async Task WriteTest()
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
            var Parms = new DTO.i.DTOs.CentreCreateParms(
                codi: "123",
                nom: "Pepe",
                esActiu: true
            );
            var result = await centreCreate.Create(Parms);

            // assert
            var expected = Parms.Nom;
            Assert.Equal(expected, result.Data.Nom);

        }


        [Fact]
        public async Task ReadTest()
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

            var Parms = new DTO.i.DTOs.EsActiuParms(esActiu: true);
            var results = await centres.FromPredicate(Parms);
            var result = results.Data.First();


            // assert
            var expected = createresult.Data.Nom;
            Assert.Equal(expected, result.Nom);

        }



    }
}
