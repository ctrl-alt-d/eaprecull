using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BusinessLayer.Common
{
    /// <summary>
    /// Un fitxer INI llegit a memòria. Prou per a les tres claus de
    /// <see cref="DadesDeLusuari"/> i prou per no haver d'afegir cap paquet a la solució.
    /// </summary>
    /// <remarks>
    /// Conserva l'ordre de seccions i claus tal com venien del disc, i conserva també les
    /// que no coneix: qui desa una clau no n'esborra cap altra. Els comentaris, en canvi,
    /// no se salven: el fitxer es reescriu sencer amb la capçalera que li dona qui el desa.
    /// </remarks>
    public sealed class FitxerIni
    {
        // Ordenat i sense distingir majúscules: així «[usuari]» i «[Usuari]» són la
        // mateixa secció, i el fitxer que surt s'assembla al que ha entrat.
        private readonly List<(string Nom, List<KeyValuePair<string, string>> Claus)> _seccions = new();

        /// <summary>Un fitxer buit, que és el que es té quan encara no s'ha desat res.</summary>
        public static FitxerIni Buit() => new();

        /// <summary>
        /// Llegeix el fitxer. Si no existeix torna un <see cref="FitxerIni"/> buit; les
        /// línies que no s'entenen es descarten en silenci, que és el que fa que un
        /// fitxer editat a mà amb una línia rara no impedeixi arrencar.
        /// </summary>
        public static FitxerIni Llegeix(string cami)
        {
            var ini = new FitxerIni();

            if (!File.Exists(cami))
                return ini;

            var seccio = string.Empty;

            foreach (var linia in File.ReadAllLines(cami, Encoding.UTF8))
            {
                var text = linia.Trim();

                if (text.Length == 0 || text[0] == ';' || text[0] == '#')
                    continue;

                if (text[0] == '[' && text[^1] == ']')
                {
                    seccio = text[1..^1].Trim();
                    continue;
                }

                // Pel primer '=': un valor pot dur-ne més (una adreça no, però una ruta sí).
                var igual = text.IndexOf('=');
                if (igual <= 0)
                    continue;

                ini.Assigna(seccio, text[..igual].Trim(), text[(igual + 1)..].Trim());
            }

            return ini;
        }

        public string? Valor(string seccio, string clau)
            => Claus(seccio, crea: false)?
                .FirstOrDefault(x => Igual(x.Key, clau)).Value;

        public void Assigna(string seccio, string clau, string valor)
        {
            var claus = Claus(seccio, crea: true)!;
            var i = claus.FindIndex(x => Igual(x.Key, clau));

            if (i < 0)
                claus.Add(new(clau, valor));
            else
                claus[i] = new(claus[i].Key, valor);
        }

        /// <summary>
        /// Escriu el fitxer sencer. L'escriptura és atòmica —fitxer temporal i
        /// <see cref="File.Move(string, string, bool)"/>— perquè un tall enmig del desat
        /// no deixi un <c>.ini</c> a mitges.
        /// </summary>
        public void Desa(string cami, string capcalera = "")
        {
            var text = new StringBuilder();

            if (capcalera.Length > 0)
            {
                foreach (var linia in capcalera.Split('\n'))
                    text.AppendLine($"; {linia.TrimEnd('\r')}");

                text.AppendLine();
            }

            foreach (var (nom, claus) in _seccions)
            {
                if (nom.Length > 0)
                    text.AppendLine($"[{nom}]");

                foreach (var (clau, valor) in claus)
                    text.AppendLine($"{clau}={valor}");

                text.AppendLine();
            }

            var carpeta = Path.GetDirectoryName(cami);
            if (!string.IsNullOrEmpty(carpeta))
                Directory.CreateDirectory(carpeta);

            // Al costat del destí, no al temporal del sistema: File.Move entre volums
            // diferents deixa de ser atòmic.
            var temporal = cami + ".tmp";
            File.WriteAllText(temporal, text.ToString(), new UTF8Encoding(false));
            File.Move(temporal, cami, overwrite: true);
        }

        private List<KeyValuePair<string, string>>? Claus(string seccio, bool crea)
        {
            var i = _seccions.FindIndex(x => Igual(x.Nom, seccio));

            if (i >= 0)
                return _seccions[i].Claus;

            if (!crea)
                return null;

            var claus = new List<KeyValuePair<string, string>>();
            _seccions.Add((seccio, claus));
            return claus;
        }

        private static bool Igual(string a, string b)
            => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }
}
