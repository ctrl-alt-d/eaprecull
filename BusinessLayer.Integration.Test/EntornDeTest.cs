using System;
using BusinessLayer.DI;
using DataLayer;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessLayer.Integration.Test
{
    /// <summary>
    /// Una base de dades per a un sol test: en memòria, amb les migracions posades i
    /// amb el BusinessLayer registrat a sobre.
    /// </summary>
    /// <remarks>
    /// Abans cada test es feia un fitxer al directori temporal amb quatre caràcters
    /// d'atzar al nom i no l'esborrava mai. Dos tests podien topar, i de fet hi topaven
    /// de tant en tant. En memòria no hi ha ni fitxer ni topada possible.
    /// <para>
    /// Es passa per <see cref="IServiceProvider"/> perquè els tests el facin servir tal
    /// com feien servir el contenidor: <c>entorn.GetRequiredService&lt;...&gt;()</c>.
    /// </para>
    /// </remarks>
    internal sealed class EntornDeTest : IServiceProvider, IDisposable
    {
        private readonly SqliteConnection _guardia;
        private readonly ServiceProvider _serveis;

        private EntornDeTest(SqliteConnection guardia, ServiceProvider serveis)
        {
            _guardia = guardia;
            _serveis = serveis;
        }

        public static EntornDeTest Nou()
        {
            // El nom fa la base de dades única; «Cache=Shared» és el que permet que
            // totes les connexions que obri la factory vagin a parar a la mateixa.
            var cadena = $"Data Source={Guid.NewGuid():N};Mode=Memory;Cache=Shared";

            // Una base de dades en memòria viu mentre hi hagi alguna connexió oberta, i
            // la factory les obre i les tanca a cada context. Cal, doncs, una connexió
            // de guàrdia oberta de cap a cap del test: si es tanca, la base de dades
            // desapareix amb ella.
            var guardia = new SqliteConnection(cadena);
            guardia.Open();

            var services = new ServiceCollection();
            services.AddDbContextFactory<AppDbContext>(opt =>
                opt.UseSqlite(cadena)
                   .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));
            services.BusinessLayerConfigureServices();

            var serveis = services.BuildServiceProvider();

            using (var context = serveis.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext())
                context.Database.Migrate();

            return new EntornDeTest(guardia, serveis);
        }

        public object? GetService(Type serviceType)
            => _serveis.GetService(serviceType);

        public void Dispose()
        {
            _serveis.Dispose();
            _guardia.Dispose();
        }
    }
}
