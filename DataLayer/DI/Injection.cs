using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DataLayer.DI
{
    public static class Injection
    {
        /// <summary>
        /// Registra l'accés a dades. No obre la base de dades ni toca el disc: aplicar
        /// les migracions és feina de <see cref="MigraBaseDeDades{TProvider}"/>, un cop
        /// construït el contenidor.
        /// </summary>
        public static IServiceCollection DataLayerConfigureServices(this IServiceCollection services)
        {
            services
            .AddDbContextFactory<AppDbContext>(
                options =>
                    options
                    .ConfigureAppDbContext()
            // .LogTo(Console.WriteLine)
            // .EnableSensitiveDataLogging()
            );

            return services;
        }

        /// <summary>
        /// Aplica les migracions pendents. Va encadenat just després del
        /// <c>BuildServiceProvider()</c>.
        /// </summary>
        /// <remarks>
        /// Això vivia dins de <see cref="DataLayerConfigureServices"/>, que per arribar
        /// a la factory feia un <c>BuildServiceProvider()</c> a mig registre: un segon
        /// contenidor, amb els seus propis singletons i el seu propi pool de connexions,
        /// que es llençava tot seguit. Separar-ho també deixa el registre lliure
        /// d'efectes secundaris, que és el que els tests poden exercitar.
        /// </remarks>
        /// <returns>El mateix proveïdor, per encadenar.</returns>
        public static TProvider MigraBaseDeDades<TProvider>(this TProvider provider)
            where TProvider : IServiceProvider
        {
            using var context = provider
                .GetRequiredService<IDbContextFactory<AppDbContext>>()
                .CreateDbContext();

            context.Database.Migrate();

            return provider;
        }
    }
}
