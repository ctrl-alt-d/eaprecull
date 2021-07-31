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
            var dataSource = Path.Combine(Path.GetTempPath(), "Esborrar" + Guid.NewGuid().ToString().Substring(4,4) + ".db");
            var ConnectionString = $"Data Source={dataSource}";

            var services = new ServiceCollection();
            services.AddDbContextFactory<AppDbContext>(opt => opt.UseSqlite(ConnectionString));
            services.BusinessLayerConfigureServices();

            var serviceProvider = services.BuildServiceProvider();

            var centreCreate = serviceProvider.GetRequiredService<ICentreCreate>();
            var alumneCreate = serviceProvider.GetRequiredService<IAlumneCreate>();
            var alumnes = serviceProvider.GetRequiredService<IAlumnes>();

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

            var createalumneparms = new DTO.i.DTOs.AlumneCreateParms(
                nom: "coco",
                cognoms: "cocino",
                dataNaixement: DateTime.Today,
                centreActualId: createcentreresult.Data.Id,
                cursDarreraActualitacioDadesId: 1,  // ToDO
                etapaActualId: null,
                dataInformeNESENEE: null,
                observacionsNESENEE: null,
                dataInformeNESENoNEE: null,
                observacionsNESENoNEE:null

            );
            var createalumeresult = await alumneCreate.Create(createalumneparms);

            var results = await alumnes.GetItems(new DTO.i.DTOs.EmptyParms());
            var result = results.Data.First();


            // assert
            var expected = createcentreresult.Data.Etiqueta;
            Assert.Equal(expected, result.CentreActual.Etiqueta);

        }



    }
}
