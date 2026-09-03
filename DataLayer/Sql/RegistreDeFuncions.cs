using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace DataLayer.Sql
{
    /// <summary>
    /// Ensenya a EF com traduir les funcions SQL pròpies.
    /// </summary>
    public static class RegistreDeFuncions
    {
        public static ModelBuilder RegistraFuncionsSql(this ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasDbFunction(typeof(FuncionsSql).GetMethod(nameof(FuncionsSql.ConteSenseAccents))!)
                .HasTranslation(arguments => new LikeExpression(
                    match: Crida(FuncionsSql.SenseAccentsNom, arguments[0]),
                    pattern: Crida(FuncionsSql.PatroConteNom, arguments[1]),
                    escapeChar: Escapada(arguments[1].TypeMapping),
                    typeMapping: null));

            return modelBuilder;
        }

        /// <summary>
        /// Una crida d'un sol argument, de text a text, que segueix el mapatge de
        /// tipus de l'argument.
        /// </summary>
        private static SqlExpression Crida(string funcio, SqlExpression argument)
            => new SqlFunctionExpression(
                functionName: funcio,
                arguments: new[] { argument },
                nullable: true,
                argumentsPropagateNullability: new[] { true },
                type: typeof(string),
                typeMapping: argument.TypeMapping);

        /// <summary>La clàusula <c>ESCAPE</c> que acompanya el patró.</summary>
        private static SqlExpression Escapada(Microsoft.EntityFrameworkCore.Storage.RelationalTypeMapping? typeMapping)
            => new SqlConstantExpression(
                value: PatroLike.Escapada.ToString(),
                type: typeof(string),
                typeMapping: typeMapping);
    }
}
