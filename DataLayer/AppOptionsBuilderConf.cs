using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DataLayer
{
    public static class AppOptionsBuilderConf
    {
        private static string? _path;
        private static string? _carpeta;

        /// <summary>
        /// La carpeta on l'aplicació desa el que ha de sobreviure a l'executable: la base
        /// de dades i el fitxer <c>Usuari.ini</c>. La crea la primera vegada.
        /// </summary>
        /// <remarks>
        /// Toca el sistema de fitxers, i per això no la pot cridar cap
        /// <c>*ConfigureServices</c> ni cap constructor de servei: només s'hi arriba en el
        /// primer ús real de la carpeta.
        /// </remarks>
        public static string CarpetaDeDades
        {
            get
            {
                if (_carpeta != null) return _carpeta;

                var directori = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

#if (DEBUG)
                // En mode debug a la carpeta de documents.
                directori = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "EapRecullData");
#endif

                if (!Directory.Exists(directori))
                {
                    Directory.CreateDirectory(directori);
                    // El programa ja les sap fer: el recordatori apunta al menú en comptes
                    // de deixar l'usuari sol davant del fitxer.
                    var recordatoriCopies = Path.Combine(directori, "@@ Fes Copies Des Del Menu - Copia De Seguretat @@");
                    File.Create(recordatoriCopies).Dispose();
                }
                _carpeta = directori;
                return _carpeta;
            }
        }

        public static string dataSource
        {
            get
            {
                if (_path != null) return _path;

                _path = Path.Combine(CarpetaDeDades, "BaseDeDades.db");
                return _path;
            }
        }

        public static string ConnectionString => $"Data Source={dataSource}";
        public static DbContextOptionsBuilder<AppDbContext> ConfigureAppDbContext(this DbContextOptionsBuilder<AppDbContext> optionsBuilder)
        {
            return
                optionsBuilder
                .UseSqlite(ConnectionString)
                .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        }
        public static DbContextOptionsBuilder ConfigureAppDbContext(this DbContextOptionsBuilder optionsBuilder)
        {
            return
                optionsBuilder
                .UseSqlite(ConnectionString)
                .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        }
    }
}




