using System;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace DataLayer
{
    public static class AppOptionsBuilderConf
    {
        private static string? _path;
        public static string dataSource
        {
            get
            {
                if (_path != null) return _path;

                var directori = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

#if (DEBUG)
                // En mode debug a la carpeta de documents.
                directori = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "EapRecullData");
#endif

                if (!Directory.Exists(directori))
                {
                    Directory.CreateDirectory(directori);
                    var recordatoriCopies = Path.Combine(directori, "@@ Recorda Fer Copies Periodiques Del Fitxer BaseDeDades @@");
                    File.Create(recordatoriCopies);
                }
                _path = Path.Combine(directori, "BaseDeDades.db");
                return _path;
            }
        }

        public static string ConnectionString => $"Data Source={dataSource}";
        public static DbContextOptionsBuilder<AppDbContext> ConfigureAppDbContext(this DbContextOptionsBuilder<AppDbContext> optionsBuilder)
        {
            return
                optionsBuilder
                .UseSqlite(ConnectionString);
        }
        public static DbContextOptionsBuilder ConfigureAppDbContext(this DbContextOptionsBuilder optionsBuilder)
        {
            return
                optionsBuilder
                .UseSqlite(ConnectionString);
        }
    }
}




