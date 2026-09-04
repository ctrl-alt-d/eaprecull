using System;
using System.Globalization;
using BusinessLayer.Abstract.Generic;
using Serilog;

namespace BusinessLayer.Common
{
    /// <summary>
    /// La secció <c>[CopiaDeSeguretat]</c> de l'<c>Usuari.ini</c>: quin destí s'ha triat,
    /// amb quin paràmetre i quan es va fer l'última còpia.
    /// </summary>
    /// <remarks>
    /// És exactament el que diu la taula «Com afegir coses» d'<c>ARQUITECTURA.md</c> §12
    /// per a una preferència desada a disc: una secció nova amb <see cref="FitxerIni"/>,
    /// que en llegir i en escriure ja conserva el que no coneix.
    /// <para>
    /// La ubicació surt d'<see cref="IDadesDeLusuari.Ubicacio"/> i no d'un
    /// <c>Path.Combine</c> propi: així el fitxer és un de sol i la lògica de quina és la
    /// carpeta de dades continua vivint en un únic lloc. Tocar-la és tocar el disc, i per
    /// això aquesta classe només s'usa dins d'un mètode, mai des d'un constructor.
    /// </para>
    /// </remarks>
    internal sealed class ConfiguracioDeCopies
    {
        public const string Seccio = "CopiaDeSeguretat";

        public const string ClauDesti = "Desti";
        public const string ClauDarreraCopia = "DarreraCopia";

        /// <summary>
        /// Quantes actuacions hi havia a la darrera còpia. És el que permet saber si des
        /// d'aleshores s'ha fet feina nova, i per tant si val la pena proposar-ne una altra.
        /// </summary>
        public const string ClauActuacionsDeLaDarreraCopia = "ActuacionsDeLaDarreraCopia";

        private const string Capcalera =
            "Dades de l'usuari d'EAP Recull.\n"
            + "Aquest fitxer el manté el programa; es pot editar a mà amb cura.";

        private readonly IDadesDeLusuari _usuari;

        public ConfiguracioDeCopies(IDadesDeLusuari usuari) => _usuari = usuari;

        /// <summary>El valor d'una clau, o null si no hi és o el fitxer no es pot llegir.</summary>
        public string? Valor(string clau)
        {
            try
            {
                var valor = FitxerIni.Llegeix(_usuari.Ubicacio).Valor(Seccio, clau);
                return string.IsNullOrWhiteSpace(valor) ? null : valor;
            }
            catch (Exception e)
            {
                // Un .ini il·legible no pot impedir fer una còpia: el pitjor cas és que el
                // programa torni a demanar on la vol desar.
                Log.Error(e, "Error llegint [{Seccio}] de l'Usuari.ini", Seccio);
                return null;
            }
        }

        /// <summary>
        /// Escriu una clau. Rellegeix el fitxer abans, com fa <see cref="DadesDeLusuari"/>:
        /// és de l'usuari, no només d'aquesta secció.
        /// </summary>
        public void Assigna(string clau, string? valor)
        {
            var ini = FitxerIni.Llegeix(_usuari.Ubicacio);
            ini.Assigna(Seccio, clau, valor ?? string.Empty);
            ini.Desa(_usuari.Ubicacio, Capcalera);
        }

        public DateTime? Data(string clau)
            => DateTime.TryParse(Valor(clau), CultureInfo.InvariantCulture,
                                 DateTimeStyles.None, out var data)
                ? data
                : null;

        public void AssignaData(string clau, DateTime valor)
            => Assigna(clau, valor.ToString("s", CultureInfo.InvariantCulture));

        public int? Sencer(string clau)
            => int.TryParse(Valor(clau), NumberStyles.Integer,
                            CultureInfo.InvariantCulture, out var valor)
                ? valor
                : null;

        public void AssignaSencer(string clau, int valor)
            => Assigna(clau, valor.ToString(CultureInfo.InvariantCulture));
    }
}
