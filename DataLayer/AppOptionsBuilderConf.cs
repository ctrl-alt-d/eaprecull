using System;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace DataLayer
{
    public static class AppOptionsBuilderConf 
    {
        public static string dataSource => Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "siteDB.db");
        public static string ConnectionString => $"Data Source={dataSource}";

        public static DbContextOptionsBuilder<AppDbContext> ConfigureAppDbContext(this DbContextOptionsBuilder<AppDbContext> optionsBuilder )
        {
            System.Console.WriteLine(  "Base de dades creada a: " + dataSource );
            return 
                optionsBuilder
                .UseSqlite(ConnectionString);
        }

        public static DbContextOptionsBuilder ConfigureAppDbContext(this DbContextOptionsBuilder optionsBuilder )
        {
            System.Console.WriteLine(  "Base de dades creada a: " + dataSource );
            return 
                optionsBuilder
                .UseSqlite(ConnectionString);
        }


    }
}




