using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DataLayer.Sql
{
    /// <summary>
    /// Dóna d'alta les funcions SQL pròpies a cada connexió que s'obre.
    /// </summary>
    /// <remarks>
    /// SQLite no desa les funcions d'usuari enlloc: viuen a la connexió, i cada
    /// connexió nova neix sense elles. D'aquí que calgui un interceptor i no
    /// pas un registre fet un sol cop a l'arrencada.
    /// </remarks>
    public sealed class FuncionsSqliteInterceptor : DbConnectionInterceptor
    {
        /// <summary>
        /// Instància única. EF fa servir el joc d'interceptors per decidir si pot
        /// reaprofitar el seu proveïdor de serveis intern; una instància nova a
        /// cada context li ompliria la memòria cau de rèpliques.
        /// </summary>
        public static readonly FuncionsSqliteInterceptor Instancia = new();

        private FuncionsSqliteInterceptor()
        {
        }

        public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
            => Registra(connection);

        public override Task ConnectionOpenedAsync(
            DbConnection connection,
            ConnectionEndEventData eventData,
            CancellationToken cancellationToken = default)
        {
            Registra(connection);
            return Task.CompletedTask;
        }

        private static void Registra(DbConnection connection)
        {
            if (connection is not SqliteConnection sqlite)
                return;

            sqlite.CreateFunction<string?, string?>(
                FuncionsSql.SenseAccentsNom,
                Accents.Treu,
                isDeterministic: true);

            sqlite.CreateFunction<string?, string>(
                FuncionsSql.PatroConteNom,
                PatroLike.Conte,
                isDeterministic: true);
        }
    }
}
