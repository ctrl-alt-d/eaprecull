using DataLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using BusinessLayer.DI;
using BusinessLayer.Abstract.Services;
using System.IO;
using System;
using System.Linq;

namespace BusinessLayer.Integration.Test
{
    public class FKest
    {


        [Fact]
        public async void Test()
        {
            // arrange
            var dataSource = Path.Combine(Path.GetTempPath(), "Esborrar" + Guid.NewGuid().ToString().Substring(4, 4) + ".db");
            var ConnectionString = $"Data Source={dataSource}";

            var services = new ServiceCollection();
            services.AddDbContextFactory<AppDbContext>(opt => opt.UseSqlite(ConnectionString));
            services.BusinessLayerConfigureServices();

            var serviceProvider = services.BuildServiceProvider();

            var centreCreate = serviceProvider.GetRequiredService<ICentreCreate>();
            var cursCreate = serviceProvider.GetRequiredService<ICursAcademicCreate>();
            var alumneCreate = serviceProvider.GetRequiredService<IAlumneCreate>();
            var alumnes = serviceProvider.GetRequiredService<IAlumneSet>();

            serviceProvider
                .GetRequiredService<IDbContextFactory<AppDbContext>>()
                .CreateDbContext()
                .Database
                .Migrate();


            // act
            var createcentreparms = new DTO.i.DTOs.CentreCreateParms(
                codi: "123",
                nom: "Pepe",
                esActiu: true
            );
            var createcentreresult = await centreCreate.Create(createcentreparms);

            var createcursparms = new DTO.i.DTOs.CursAcademicCreateParms(
                2021,
                true
            );
            var createcursresult = await cursCreate.Create(createcursparms);


            var createalumneparms = new DTO.i.DTOs.AlumneCreateParms(
                nom: "coco",
                cognoms: "cocino",
                dataNaixement: DateTime.Today,
                centreActualId: createcentreresult.Data.Id,
                cursDarreraActualitacioDadesId: createcursresult.Data.Id,
                etapaActualId: null,
                nivellActual: "1r",
                dataInformeNESENEE: null,
                observacionsNESENEE: string.Empty,
                dataInformeNESENoNEE: null,
                observacionsNESENoNEE: string.Empty,
                tags: "#costabrava"

            );
            var createalumeresult = await alumneCreate.Create(createalumneparms);

            var searchParms = new DTO.i.DTOs.AlumneSearchParms();
            var results = await alumnes.FromPredicate(searchParms);
            var result = results.Data.First();


            // assert
            var expected = createcentreresult.Data.Etiqueta;
            Assert.Equal(expected, result.CentreActual.Etiqueta);

        }



    }
}
