using System;

namespace DataLayer.Sql
{
    /// <summary>
    /// Funcions SQL pròpies, per fer servir dins de consultes EF.
    /// </summary>
    public static class FuncionsSql
    {
        /// <summary>Nom SQL de la funció que redueix una columna a clau de cerca.</summary>
        internal const string ClauDeCercaNom = "clau_cerca";

        /// <summary>Nom SQL de la funció que fa el patró a partir del text cercat.</summary>
        internal const string PatroConteNom = "patro_conte";

        /// <summary>
        /// Si la columna conté el text, comparant-ho tot per clau de cerca: sense mirar
        /// prim amb majúscules, ni amb accents, ni amb la manera d'escriure apòstrofs,
        /// cometes, guions ni el punt volat. Es tradueix a
        /// <c>clau_cerca(text) LIKE patro_conte(cercat) ESCAPE '\'</c>.
        /// </summary>
        /// <param name="text">La columna on es busca.</param>
        /// <param name="cercat">
        /// El text tal com l'ha escrit l'usuari: de normalitzar-lo i d'escapar-ne els
        /// comodins ja se n'encarrega la funció.
        /// </param>
        /// <remarks>
        /// Aquest cos no s'executa mai: el mètode només existeix per poder-lo anomenar
        /// dins d'un arbre d'expressió. Què es dóna per equivalent, a
        /// <see cref="ClauDeCerca"/>.
        /// </remarks>
        public static bool Conte(string? text, string cercat)
            => throw new NotSupportedException(
                $"{nameof(Conte)} només es pot fer servir dins d'una consulta EF.");
    }
}
