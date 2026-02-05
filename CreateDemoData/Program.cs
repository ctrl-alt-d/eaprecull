using System;
using System.Linq;
using BusinessLayer.Abstract.Services;
using Microsoft.Extensions.DependencyInjection;

using BusinessLayer.DI;
using BusinessLayer.Abstract.Generic;
using DataLayer.DI;
using System.Threading.Tasks;
using Parms = DTO.i.DTOs;

namespace CreateDemoData
{
    class Program
    {
        private static ServiceProvider? _ServiceProvider;
        public static ServiceProvider GetServiceProvider()
        {
            _ServiceProvider = _ServiceProvider ??
                new ServiceCollection()
                .DataLayerConfigureServices()
                .BusinessLayerConfigureServices()
                .BuildServiceProvider();
            return _ServiceProvider;
        }

        public static T GetBLOperation<T>()
            where T : IBLOperation
        {
            return
                GetServiceProvider()
                .GetRequiredService<T>();
        }

        static async Task Main(string[] args)
        {
            await CreaCentre();
            await CreaCursAcademic();
            await CreateAlumne();
        }

        private static async Task CreaCentre()
        {
            Console.WriteLine("Creant Centres!");

            using var blset = GetBLOperation<ICentreSet>();
            var nhiha = (await blset.FromPredicate(new Parms.EsActiuParms(null))).Data!.Any();

            if (!nhiha)
            {
                using var crea = GetBLOperation<ICentreCreate>();
                await crea.Create(new Parms.CentreCreateParms("M1", "Les Melies", true));
                await crea.Create(new Parms.CentreCreateParms("M2", "Cendrassos", true));
                for (var i = 0; i < 20; i++)
                {
                    await crea.Create(new Parms.CentreCreateParms("Centre: " + Guid.NewGuid().ToString().Substring(2, 3), Guid.NewGuid().ToString().Substring(2, 8), true));
                }
            }
        }

        private static async Task CreaCursAcademic()
        {
            Console.WriteLine("Creant Cursos Acadèmics!");

            using var blset = GetBLOperation<ICursAcademicSet>();
            var nhiha = (await blset.FromPredicate(new Parms.EsActiuParms(null))).Data!.Any();

            if (!nhiha)
            {
                using var crea = GetBLOperation<ICursAcademicCreate>();
                await crea.Create(new Parms.CursAcademicCreateParms(2020, false));
                await crea.Create(new Parms.CursAcademicCreateParms(2021, true));
                await crea.Create(new Parms.CursAcademicCreateParms(2022, false));
            }
        }


        private static async Task CreateAlumne()
        {
            Console.WriteLine("Creant Alumnes!");

            using var blset = GetBLOperation<IAlumneSet>();
            var nhiha = (await blset.FromPredicate(new Parms.AlumneSearchParms())).Data!.Any();

            using var blcentresset = GetBLOperation<ICentreSet>();
            var centres = await blcentresset.FromPredicate(new Parms.EsActiuParms(null));

            using var blcursos = GetBLOperation<ICursAcademicSet>();
            var cursos = await blcursos.FromPredicate(new Parms.EsActiuParms(null));

            using var crea = GetBLOperation<IAlumneCreate>();

            if (!nhiha)
                for (int i = 0; i < 2500; i++)
                {
                    var createParm =
                        new Parms.AlumneCreateParms(
                            $"Pepe {i}",
                            $"Escobar {i}",
                            DateTime.Now.AddYears(-10),
                            centres.Data![i % centres.Data.Count()].Id,
                            cursos.Data![i % cursos.Data.Count()].Id,
                            null,
                            (i % 5).ToString(),
                            DateTime.Now,
                            "hola",
                            null,
                            "",
                            "#hola"
                        );

                    await crea.Create(createParm);
                }
        }

    }
}
