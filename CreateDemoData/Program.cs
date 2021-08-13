using System;
using System.Linq;
using BusinessLayer.Abstract.Services;
using Microsoft.Extensions.DependencyInjection;

using BusinessLayer.DI;
using BusinessLayer.Abstract.Generic;
using DataLayer.DI;
using System.Threading.Tasks;
using parms = DTO.i.DTOs;

namespace CreateDemoData
{
    class Program
    {
        private static ServiceProvider? _ServiceProvider;
        public static ServiceProvider GetServiceProvider() {
            _ServiceProvider = _ServiceProvider ??
                new ServiceCollection()
                .DataLayerConfigureServices()
                .BusinessLayerConfigureServices()
                .BuildServiceProvider();            
            return _ServiceProvider;
        }

        public static T GetBLOperation<T>()
            where T: IBLOperation
        {
            return 
                GetServiceProvider()
                .GetRequiredService<T>();
        }

        static async Task Main(string[] args)
        {
            await CreaCentre();
            await CreaCursAcademic();
        }

        private static async Task CreaCentre()
        {
            Console.WriteLine("Creant Centres!");

            var getset = GetBLOperation<ICentreGetSet>();
            var nhiha = (await getset.FromPredicate(new parms.EsActiuParms(null))).Data!.Any();

            if (!nhiha)
            {
                var crea = GetBLOperation<ICentreCreate>();
                await crea.Create(new parms.CentreCreateParms("M1", "Les Melies", true));
                await crea.Create(new parms.CentreCreateParms("M2", "Cendrassos", true));
                for (var i =0; i<20; i++)
                {
                    await crea.Create(new parms.CentreCreateParms("Centre: "+Guid.NewGuid().ToString().Substring(2,3), Guid.NewGuid().ToString().Substring(2,8), true));
                }
            }
        }

        private static async Task CreaCursAcademic()
        {
            Console.WriteLine("Creant Cursos Acadèmics!");

            var getset = GetBLOperation<ICursAcademicGetSet>();
            var nhiha = (await getset.FromPredicate(new parms.EmptyParms())).Data!.Any();

            if (!nhiha)
            {
                var crea = GetBLOperation<ICursAcademicCreate>();
                await crea.Create(new parms.CursAcademicCreateParms(2020, false));
                await crea.Create(new parms.CursAcademicCreateParms(2021, true));
                await crea.Create(new parms.CursAcademicCreateParms(2022, false));
            }
        }

    }
}
