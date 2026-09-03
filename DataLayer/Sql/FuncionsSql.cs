using System;

namespace DataLayer.Sql
{
    /// <summary>
    /// Funcions SQL pròpies, per fer servir dins de consultes EF.
    /// </summary>
    public static class FuncionsSql
    {
        /// <summary>Nom SQL de la funció que treu els accents d'una columna.</summary>
        internal const string SenseAccentsNom = "sense_accents";

        /// <summary>Nom SQL de la funció que fa el patró a partir del text cercat.</summary>
        internal const string PatroConteNom = "patro_conte";

        /// <summary>
        /// Si la columna conté el text, sense mirar prim amb accents ni amb
        /// majúscules. Es tradueix a
        /// <c>sense_accents(text) LIKE patro_conte(cercat) ESCAPE '\'</c>.
        /// </summary>
        /// <param name="text">La columna on es busca.</param>
        /// <param name="cercat">
        /// El text tal com l'ha escrit l'usuari: dels accents i dels comodins ja
        /// se n'encarrega la funció.
        /// </param>
        /// <remarks>
        /// Aquest cos no s'executa mai: el mètode només existeix per poder-lo anomenar
        /// dins d'un arbre d'expressió.
        /// </remarks>
        public static bool ConteSenseAccents(string? text, string cercat)
            => throw new NotSupportedException(
                $"{nameof(ConteSenseAccents)} només es pot fer servir dins d'una consulta EF.");
    }
}
