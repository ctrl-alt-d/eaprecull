using System;
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
            Console.WriteLine("Creant Centres!");
            var centreCreate = GetBLOperation<ICentreCreate>();   
            await centreCreate.Create(new parms.CentreCreateParms("M1", "Les Melies",true));
            await centreCreate.Create(new parms.CentreCreateParms("M2", "Cendrassos",true));
        }
    }
}
