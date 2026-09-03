using DataLayer;
using Microsoft.EntityFrameworkCore;
using Xunit;
using BusinessLayer.DI;
using Microsoft.Extensions.DependencyInjection;
using BusinessLayer.Abstract.Services;
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
            using var serviceProvider = EntornDeTest.Nou();

            var centreCreate = serviceProvider.GetRequiredService<ICentreCreate>();


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
            using var serviceProvider = EntornDeTest.Nou();

            var centreCreate = serviceProvider.GetRequiredService<ICentreCreate>();
            var centres = serviceProvider.GetRequiredService<ICentreSet>();


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
