using System;
using System.Globalization;

namespace BusinessLayer.Common
{
    /// <summary>
    /// Com es diuen els fitxers de còpia: <c>EAPRecull-BaseDeDades-2026-09-04_1215.zip</c>.
    /// </summary>
    /// <remarks>
    /// Data i hora al nom perquè s'ordenin sols i perquè l'usuari sàpiga què té sense
    /// obrir res. I el patró és el que <strong>delimita què és nostre</strong>: la carpeta
    /// que tria l'usuari pot tenir-hi qualsevol altra cosa, i cap adaptador no ha de
    /// llistar-la ni esborrar-la mai.
    /// </remarks>
    public static class NomDeCopia
    {
        public const string Prefix = "EAPRecull-BaseDeDades-";
        public const string Extensio = ".zip";

        /// <summary>El comodí per buscar-les al destí.</summary>
        public const string Patro = Prefix + "*" + Extensio;

        /// <summary>Sufix del fitxer a mig escriure, que el <see cref="Patro"/> no veu.</summary>
        public const string ExtensioTemporal = ".tmp";

        public static string Per(DateTime quan)
            => Prefix + quan.ToString("yyyy-MM-dd_HHmm", CultureInfo.InvariantCulture) + Extensio;

        /// <summary>És un fitxer nostre? Sense mirar carpetes ni majúscules.</summary>
        public static bool EsNostre(string nom)
            => nom.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase)
            && nom.EndsWith(Extensio, StringComparison.OrdinalIgnoreCase);
    }
}
