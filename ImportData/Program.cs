using System;
using System.Linq;
using BusinessLayer.Abstract.Services;
using Microsoft.Extensions.DependencyInjection;

using BusinessLayer.DI;
using BusinessLayer.Abstract.Generic;
using DataLayer.DI;
using System.Threading.Tasks;
using System.IO;

namespace ImportData
{
    class Program
    {
        private static string FILENAME = "Importacio.xlsx";
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
            using var blImport = GetBLOperation<IImportAll>();

            var path = CalculaPath();
            var resultat = await blImport.Run(path);
            System.Console.WriteLine($@"
*** RESULTAT IMPORTACIO ***

Alumnes importants: {resultat.Data!.NumAlumnes}

Actuacions importades: {resultat.Data!.NumActuacions}

                ");

        }

        private static string CalculaPath()
        {
            var directori = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

#if (DEBUG)
            // En mode debug a la carpeta de documents.
            directori = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "EapRecullData");
#endif

            if (!Directory.Exists(directori))
            {
                throw new Exception($"No trobo la carpeta: {directori}");
            }
            var path = Path.Combine(directori, FILENAME);

            if (!File.Exists(path))
            {
                throw new Exception($"No trobat el fitxer {path}");
            }

            return path;
        }
    }
}
