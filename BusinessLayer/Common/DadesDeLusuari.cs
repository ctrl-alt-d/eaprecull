using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BusinessLayer.Abstract;
using BusinessLayer.Abstract.Exceptions;
using BusinessLayer.Abstract.Generic;
using DataLayer;
using Serilog;

namespace BusinessLayer.Common
{
    /// <summary>
    /// Implementació d'<see cref="IDadesDeLusuari"/> sobre un <c>Usuari.ini</c> a la
    /// carpeta de dades. Singleton: el desat n'actualitza la còpia en memòria i tothom
    /// la veu.
    /// </summary>
    /// <remarks>
    /// El constructor no toca el disc —ni tan sols per saber quina és la carpeta, que
    /// crear-la ja és tocar-lo—: la lectura es fa al primer accés a <see cref="Actuals"/>.
    /// És l'invariant que permet que <c>RegistreDITest</c> i <c>InjeccioTest</c>
    /// construeixin el contenidor sencer sense efectes secundaris.
    /// </remarks>
    public sealed class DadesDeLusuari : IDadesDeLusuari
    {
        public const string NomDelFitxer = "Usuari.ini";

        private const string Seccio = "Usuari";

        private const string Capcalera =
            "Dades de l'usuari d'EAP Recull.\n"
            + "Aquest fitxer el manté el programa; es pot editar a mà amb cura.";

        // Forma de correu i prou: no s'exigeix el domini xtec.cat perquè també hi ha
        // adreces @edu.gencat.cat i similars.
        private static readonly Regex FormaDeCorreu =
            new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        private readonly object _pany = new();
        private readonly string? _carpetaExplicita;

        private DadesUsuari? _actuals;

        /// <param name="carpeta">
        /// Només per als tests, que treballen sobre una carpeta temporal. En producció es
        /// deixa a null i s'agafa <see cref="AppOptionsBuilderConf.CarpetaDeDades"/>.
        /// </param>
        public DadesDeLusuari(string? carpeta = null) => _carpetaExplicita = carpeta;

        public string Ubicacio
            => Path.Combine(_carpetaExplicita ?? AppOptionsBuilderConf.CarpetaDeDades, NomDelFitxer);

        public DadesUsuari Actuals
        {
            get
            {
                lock (_pany)
                    return _actuals ??= Llegeix();
            }
        }

        /// <summary>
        /// «Informat» vol dir, exactament, «les dades llegides passen les validacions».
        /// Així no hi ha dues definicions que puguin divergir.
        /// </summary>
        public bool EstaInformat => Valida(Actuals).Count == 0;

        public Task<OperationResult<DadesUsuari>> Desa(DadesUsuari dades)
        {
            var netes = new DadesUsuari(
                (dades.Nom ?? string.Empty).Trim(),
                (dades.Cognoms ?? string.Empty).Trim(),
                (dades.AdrecaXtec ?? string.Empty).Trim());

            var trencades = Valida(netes);
            if (trencades.Count > 0)
                return Task.FromResult(new OperationResult<DadesUsuari>(trencades));

            try
            {
                lock (_pany)
                {
                    // Es rellegeix abans d'escriure per conservar seccions i claus que no
                    // són nostres: el fitxer és de l'usuari, no només del programa.
                    var ini = FitxerIni.Llegeix(Ubicacio);

                    ini.Assigna(Seccio, nameof(DadesUsuari.Nom), netes.Nom);
                    ini.Assigna(Seccio, nameof(DadesUsuari.Cognoms), netes.Cognoms);
                    ini.Assigna(Seccio, nameof(DadesUsuari.AdrecaXtec), netes.AdrecaXtec);

                    ini.Desa(Ubicacio, Capcalera);

                    _actuals = netes;
                }

                return Task.FromResult(new OperationResult<DadesUsuari>(netes));
            }
            catch (Exception e)
            {
                // Com qualsevol operació de negoci: cap excepció crua cap amunt.
                Log.Error(e, "Error desant les dades de l'usuari a {Ubicacio}", Ubicacio);

                return Task.FromResult(new OperationResult<DadesUsuari>(
                    new List<BrokenRule>
                    {
                        new($"No s'han pogut desar les dades al fitxer {Ubicacio}: {e.Message}"),
                    }));
            }
        }

        /// <summary>
        /// Les regles de negoci de les dades de l'usuari. Viuen aquí i no al ViewModel
        /// perquè valguin igual per a la UI i per a qualsevol altre consumidor.
        /// </summary>
        private static List<BrokenRule> Valida(DadesUsuari dades)
        {
            var trencades = new List<BrokenRule>();

            if (string.IsNullOrWhiteSpace(dades.Nom))
                trencades.Add(new(nameof(DadesUsuari.Nom), "Cal informar el nom."));

            if (string.IsNullOrWhiteSpace(dades.Cognoms))
                trencades.Add(new(nameof(DadesUsuari.Cognoms), "Cal informar els cognoms."));

            if (string.IsNullOrWhiteSpace(dades.AdrecaXtec))
                trencades.Add(new(nameof(DadesUsuari.AdrecaXtec), "Cal informar l'adreça xtec."));
            else if (!FormaDeCorreu.IsMatch(dades.AdrecaXtec))
                trencades.Add(new(nameof(DadesUsuari.AdrecaXtec),
                    "L'adreça xtec no té un format vàlid."));

            return trencades;
        }

        /// <summary>
        /// Qualsevol problema de lectura és <see cref="DadesUsuari.Buides"/> i una línia
        /// al log: un <c>.ini</c> escombraria no pot impedir arrencar, i el pitjor cas és
        /// que el programa torni a demanar les dades.
        /// </summary>
        private DadesUsuari Llegeix()
        {
            try
            {
                var ini = FitxerIni.Llegeix(Ubicacio);

                return new DadesUsuari(
                    ini.Valor(Seccio, nameof(DadesUsuari.Nom)) ?? string.Empty,
                    ini.Valor(Seccio, nameof(DadesUsuari.Cognoms)) ?? string.Empty,
                    ini.Valor(Seccio, nameof(DadesUsuari.AdrecaXtec)) ?? string.Empty);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error llegint les dades de l'usuari de {Ubicacio}", Ubicacio);
                return DadesUsuari.Buides;
            }
        }
    }
}
