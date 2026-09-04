using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusinessLayer.Abstract;
using BusinessLayer.Abstract.Exceptions;
using BusinessLayer.Abstract.Generic;
using BusinessLayer.Abstract.Services;
using BusinessLayer.Common;
using DataLayer;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Dtoo = DTO.o.DTOs;

namespace BusinessLayer.Services
{
    /// <summary>
    /// Bolcat → verificació → zip xifrat → desat al destí → retenció. Tot el que és car és
    /// aquí i és comú a tots els destins; l'únic que canvia segons on van les còpies és
    /// l'<see cref="IMagatzemDeCopies"/> que hi ha darrere.
    /// </summary>
    /// <remarks>
    /// L'<c>IEnumerable&lt;IMagatzemDeCopies&gt;</c> del constructor és el que fa que
    /// afegir un destí nou sigui una línia al registre i cap canvi aquí. Els magatzems es
    /// registren a <c>BusinessLayerConfigureServices()</c> abans del bucle d'operacions, i
    /// per això <c>ActivatorUtilities.CreateInstance</c> els resol.
    /// </remarks>
    public class CopiaDeSeguretat : BLOperation, ICopiaDeSeguretat
    {
        /// <summary>Quantes còpies es conserven al destí (§15, Q5: tres, i com a constant).</summary>
        public const int QuantesEsGuarden = 3;

        /// <summary>El nom del bolcat dins del zip, i el de la base de dades en producció.</summary>
        private const string NomDeLaBaseDeDades = "BaseDeDades.db";

        private const string NomDelLlegeixMe = "LLEGEIX-ME.txt";

        private readonly IReadOnlyList<IMagatzemDeCopies> _magatzems;
        private readonly IDadesDeLusuari _usuari;
        private readonly ConfiguracioDeCopies _configuracio;

        public CopiaDeSeguretat(
            IDbContextFactory<AppDbContext> appDbContextFactory,
            IEnumerable<IMagatzemDeCopies> magatzems,
            IDadesDeLusuari usuari)
            : base(appDbContextFactory)
        {
            _magatzems = magatzems.ToList();
            _usuari = usuari;
            _configuracio = new ConfiguracioDeCopies(usuari);
        }

        public IReadOnlyList<DestiDisponible> Destins
            => _magatzems.Select(m => new DestiDisponible(m.Clau, m.Titol, m.DemanaCarpeta)).ToList();

        public DestiDeCopies DestiActual => Magatzem()?.Desti ?? DestiDeCopies.Cap;

        public string ClauDelDestiActual => Magatzem()?.Clau ?? string.Empty;

        public DateTime? DarreraCopia => _configuracio.Data(ConfiguracioDeCopies.ClauDarreraCopia);

        public int CopiesQueEsGuarden => QuantesEsGuarden;

        /// <summary>
        /// Els dos requisits alhora: temps i feina nova. Si només mirés el temps, un usuari
        /// que no ha tocat res en un mes rebria la proposta cada dia i aprendria a
        /// tancar-la sense llegir-la; el dia que sí que importés, ja no la miraria.
        /// </summary>
        public async Task<OperationResult<PropostaDeCopia>> CalFerCopia(CancellationToken ct = default)
        {
            try
            {
                var actuacions = await GetContext().Actuacions.CountAsync(ct);

                // Sense actuacions no hi ha res a perdre i una còpia no val la pena.
                if (actuacions == 0)
                    return new OperationResult<PropostaDeCopia>(PropostaDeCopia.No);

                var anteriors = _configuracio.Sencer(ConfiguracioDeCopies.ClauActuacionsDeLaDarreraCopia) ?? 0;
                var noves = actuacions - anteriors;

                // Si el compte ha baixat —s'han esborrat actuacions— noves surt negatiu i
                // tampoc es proposa: la regla que ha demanat l'usuari és «actuacions noves».
                if (noves <= 0)
                    return new OperationResult<PropostaDeCopia>(PropostaDeCopia.No);

                if (DarreraCopia is not { } darrera)
                    return new OperationResult<PropostaDeCopia>(
                        new PropostaDeCopia(true, noves, 0, MaiSHaFetCap: true));

                var dies = (int)(DateTime.Now.Date - darrera.Date).TotalDays;

                return new OperationResult<PropostaDeCopia>(
                    dies > ICopiaDeSeguretat.SetmanesSenseCopia * 7
                        ? new PropostaDeCopia(true, noves, dies, MaiSHaFetCap: false)
                        : PropostaDeCopia.No);
            }
            catch (Exception e)
            {
                // Això ho crida el taulell en arrencar: un problema comptant no pot impedir
                // obrir el programa. El pitjor cas és no proposar la còpia aquesta vegada.
                Log.Error(e, "Error comprovant si cal proposar una còpia de seguretat");
                return new OperationResult<PropostaDeCopia>(PropostaDeCopia.No);
            }
        }

        public async Task<OperationResult<DestiDeCopies>> ConfiguraDesti(
            string clau, string? parametre, CancellationToken ct = default)
        {
            var magatzem = _magatzems.FirstOrDefault(m => EsElMateix(m.Clau, clau));

            if (magatzem is null)
                return Trenca<DestiDeCopies>($"El destí «{clau}» no existeix en aquesta versió del programa.");

            try
            {
                var resultat = await magatzem.Configura(parametre, ct);

                if (resultat.BrokenRules.Count > 0)
                    return resultat;

                _configuracio.Assigna(ConfiguracioDeCopies.ClauDesti, magatzem.Clau);

                return resultat;
            }
            catch (Exception e)
            {
                Log.Error(e, "Error configurant el destí de còpies {Clau}", clau);
                return Trenca<DestiDeCopies>($"No s'ha pogut configurar el destí: {e.Message}");
            }
        }

        public async Task<OperationResult<Copies>> Llista(CancellationToken ct = default)
        {
            if (Magatzem() is not { } magatzem)
                return new OperationResult<Copies>(Copies.Cap);

            try
            {
                return await magatzem.Llista(ct);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error llistant les còpies del destí");
                return Trenca<Copies>($"No s'han pogut llegir les còpies del destí: {e.Message}");
            }
        }

        public async Task<OperationResult<DestiDeCopies>> Oblida()
        {
            if (Magatzem() is not { } magatzem)
                return new OperationResult<DestiDeCopies>(DestiDeCopies.Cap);

            try
            {
                var resultat = await magatzem.Oblida();

                if (resultat.BrokenRules.Count == 0)
                    _configuracio.Assigna(ConfiguracioDeCopies.ClauDesti, null);

                return resultat;
            }
            catch (Exception e)
            {
                Log.Error(e, "Error oblidant el destí de còpies");
                return Trenca<DestiDeCopies>($"No s'ha pogut oblidar el destí: {e.Message}");
            }
        }

        public async Task<OperationResult<Dtoo.CopiaResult>> Run(
            string password, IProgress<string>? progres = null, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(password))
                return Trenca<Dtoo.CopiaResult>("Cal una contrasenya per xifrar la còpia.");

            if (Magatzem() is not { } magatzem)
                return Trenca<Dtoo.CopiaResult>("Tria on vols desar les còpies abans de fer-ne cap.");

            // Una carpeta nova per execució: VACUUM INTO exigeix que el destí no existeixi,
            // i el .db en clar que hi passa no s'hi pot quedar mai (el finally de sota).
            var temporal = Path.Combine(Path.GetTempPath(), "EapRecull-" + Guid.NewGuid().ToString("N"));

            try
            {
                ct.ThrowIfCancellationRequested();

                progres?.Report("Preparant la còpia…");
                Directory.CreateDirectory(temporal);

                var bolcat = Path.Combine(temporal, NomDeLaBaseDeDades);
                await Bolca(bolcat, ct);

                ct.ThrowIfCancellationRequested();

                progres?.Report("Comprovant-la…");
                var comprovacio = await Comprova(bolcat, ct);

                if (comprovacio.BrokenRules.Count > 0)
                    return new OperationResult<Dtoo.CopiaResult>(comprovacio.BrokenRules);

                var actuacions = comprovacio.Actuacions;

                ct.ThrowIfCancellationRequested();

                progres?.Report("Xifrant…");
                var quan = DateTime.Now;
                var nom = NomDeCopia.Per(quan);
                var zip = Path.Combine(temporal, nom);

                await Task.Run(() => ZipXifrat.Escriu(zip, password, Entrades(temporal, bolcat, nom, quan, actuacions)), ct);

                ct.ThrowIfCancellationRequested();

                // Prepara() primer: si el destí no està llest —carpeta que ja no existeix,
                // autorització cancel·lada— s'acaba aquí, i sense haver deixat res a mig fer.
                var preparat = await magatzem.Prepara(ct);
                if (preparat.BrokenRules.Count > 0)
                    return new OperationResult<Dtoo.CopiaResult>(preparat.BrokenRules);

                var total = new FileInfo(zip).Length;
                var progresDeBytes = new Progress<long>(fets => progres?.Report(Desant(fets, total)));

                progres?.Report(Desant(0, total));
                var desada = await magatzem.Desa(zip, nom, progresDeBytes, ct);

                if (desada.BrokenRules.Count > 0)
                    return new OperationResult<Dtoo.CopiaResult>(desada.BrokenRules);

                // La retenció, només ara: si s'esborrés abans i la còpia fallés a mitges,
                // l'usuari es quedaria amb dues còpies en comptes de tres i cap de nova.
                progres?.Report("Retirant les còpies antigues…");
                var avis = await Reteny(magatzem, ct);

                // Fora del try general: la còpia ja és al destí, i que no es pugui apuntar
                // la data a l'Usuari.ini no la desfà ni la converteix en un error.
                try
                {
                    _configuracio.AssignaData(ConfiguracioDeCopies.ClauDarreraCopia, quan);

                    // El recompte del bolcat, no el de la base viva: és el que hi ha desat
                    // de debò, i el que després diu quantes actuacions són noves.
                    _configuracio.AssignaSencer(
                        ConfiguracioDeCopies.ClauActuacionsDeLaDarreraCopia, actuacions);
                }
                catch (Exception e)
                {
                    Log.Error(e, "Error desant la data de la darrera còpia a l'Usuari.ini");
                }

                progres?.Report("Fet.");

                return new OperationResult<Dtoo.CopiaResult>(new Dtoo.CopiaResult(
                    nom: desada.Data!.Nom,
                    quan: quan,
                    bytes: desada.Data.Bytes,
                    desti: preparat.Data!.Detall,
                    copiesGuardades: QuantesEsGuarden,
                    actuacions: actuacions,
                    avis: avis));
            }
            catch (OperationCanceledException)
            {
                Log.Information("Còpia de seguretat cancel·lada per l'usuari.");
                return Trenca<Dtoo.CopiaResult>("S'ha cancel·lat la còpia; no s'ha desat res.");
            }
            catch (Exception e)
            {
                Log.Error(e, "Error fent la còpia de seguretat");
                return Trenca<Dtoo.CopiaResult>($"No s'ha pogut preparar la còpia: {e.Message}");
            }
            finally
            {
                // Passi el que passi: aquí hi ha hagut la base de dades en clar.
                EsborraLaCarpeta(temporal);
            }
        }

        /// <summary>
        /// El bolcat consistent. <c>VACUUM INTO</c> és l'API de SQLite pensada exactament
        /// per això: una còpia consistent i compactada d'una base de dades viva, des d'una
        /// connexió qualsevol i sense bloquejar-ne cap altra.
        /// </summary>
        /// <remarks>
        /// Copiar el fitxer amb <c>File.Copy</c> mentre hi ha una transacció oberta dona un
        /// fitxer corrupte, i tancar connexions «a mà» no es pot fer: la
        /// <see cref="IDbContextFactory{TContext}"/> n'obre i en tanca a cada operació i
        /// <c>Microsoft.Data.Sqlite</c> en manté un <em>pool</em>. Efecte lateral: la còpia
        /// surt compactada i sense <c>-wal</c> ni <c>-journal</c>.
        /// </remarks>
        private async Task Bolca(string desti, CancellationToken ct)
        {
            // El camí va literal dins de l'SQL perquè VACUUM INTO no accepta paràmetres;
            // s'escapen les cometes simples, que és l'únic metacaràcter d'un literal SQLite.
            var literal = desti.Replace("'", "''");

            // EF1002 avisa que aquí no hi ha protecció contra injecció. És cert i no hi ha
            // alternativa: VACUUM INTO no accepta paràmetres. El camí no ve de l'usuari
            // —és Path.GetTempPath() + un Guid— i les cometes simples ja van escapades.
#pragma warning disable EF1002
            await GetContext().Database.ExecuteSqlRawAsync($"VACUUM INTO '{literal}'", ct);
#pragma warning restore EF1002

            // VACUUM no fa res sobre una base de dades en memòria, i no ho diu: acaba bé i
            // no escriu el fitxer. En producció la base de dades sempre és un fitxer, però
            // sense aquesta comprovació el símptoma seria un error confús tres passes més
            // enllà, en obrir un bolcat que no existeix.
            if (!File.Exists(desti))
                throw new InvalidOperationException(
                    "el bolcat de la base de dades no ha generat cap fitxer.");
        }

        /// <summary>
        /// Obre el bolcat en només lectura i el comprova abans de xifrar res. Si
        /// <c>integrity_check</c> no diu <c>ok</c>, no es desa res enlloc.
        /// </summary>
        private static async Task<(List<BrokenRule> BrokenRules, int Actuacions)> Comprova(
            string bolcat, CancellationToken ct)
        {
            var cadena = new SqliteConnectionStringBuilder
            {
                DataSource = bolcat,
                Mode = SqliteOpenMode.ReadOnly,
            }.ToString();

            using var connexio = new SqliteConnection(cadena);
            await connexio.OpenAsync(ct);

            using (var integritat = connexio.CreateCommand())
            {
                integritat.CommandText = "PRAGMA integrity_check;";

                var diu = (await integritat.ExecuteScalarAsync(ct))?.ToString();

                if (!string.Equals(diu, "ok", StringComparison.OrdinalIgnoreCase))
                {
                    Log.Error("integrity_check del bolcat ha tornat {Resultat}", diu);

                    return ([new BrokenRule(
                        "La còpia de la base de dades no ha passat la comprovació d'integritat; "
                        + "no s'ha desat res.")], 0);
                }
            }

            using var compte = connexio.CreateCommand();
            compte.CommandText = "SELECT COUNT(*) FROM Actuacions;";

            return ([], Convert.ToInt32(await compte.ExecuteScalarAsync(ct)));
        }

        /// <summary>
        /// Què va dins del zip: el bolcat, l'<c>Usuari.ini</c> si existeix —és minúscul i
        /// completa la restauració— i el <c>LLEGEIX-ME.txt</c> amb les instruccions.
        /// </summary>
        private Dictionary<string, string> Entrades(
            string temporal, string bolcat, string nomDelZip, DateTime quan, int actuacions)
        {
            var entrades = new Dictionary<string, string> { [NomDeLaBaseDeDades] = bolcat };

            try
            {
                if (File.Exists(_usuari.Ubicacio))
                {
                    var copia = Path.Combine(temporal, Path.GetFileName(_usuari.Ubicacio));
                    File.Copy(_usuari.Ubicacio, copia, overwrite: true);
                    entrades[Path.GetFileName(_usuari.Ubicacio)] = copia;
                }
            }
            catch (Exception e)
            {
                // L'Usuari.ini és un extra: que no s'hi pugui llegir no val una còpia menys.
                Log.Warning(e, "No s'ha pogut afegir l'Usuari.ini a la còpia");
            }

            var llegeixMe = Path.Combine(temporal, NomDelLlegeixMe);
            File.WriteAllText(llegeixMe, ZipXifrat.LlegeixMe(nomDelZip, quan, actuacions));
            entrades[NomDelLlegeixMe] = llegeixMe;

            return entrades;
        }

        /// <summary>
        /// La retenció. Una fallada aquí <strong>no fa fallar la còpia</strong>: la nova ja
        /// és a lloc, que és el que importa. Es registra i es torna com a avís.
        /// </summary>
        private static async Task<string?> Reteny(IMagatzemDeCopies magatzem, CancellationToken ct)
        {
            try
            {
                var retirades = await magatzem.RetiraSobrants(QuantesEsGuarden, ct);

                if (retirades.BrokenRules.Count == 0)
                    return null;

                Log.Error("No s'han pogut retirar les còpies antigues: {Motius}",
                    string.Join(" · ", retirades.BrokenRules.Select(r => r.Message)));
            }
            catch (Exception e)
            {
                Log.Error(e, "Error retirant les còpies antigues");
            }

            return "La còpia s'ha desat correctament, però no s'han pogut retirar les còpies antigues.";
        }

        /// <summary>
        /// El magatzem que toca: el que digui l'<c>Usuari.ini</c> i, si no en diu cap i
        /// només n'hi ha un de compilat, aquell. Amb un sol destí no cal fer triar res.
        /// </summary>
        private IMagatzemDeCopies? Magatzem()
        {
            var clau = _configuracio.Valor(ConfiguracioDeCopies.ClauDesti);

            if (clau is not null)
                return _magatzems.FirstOrDefault(m => EsElMateix(m.Clau, clau));

            return _magatzems.Count == 1 ? _magatzems[0] : null;
        }

        private static string Desant(long fets, long total)
            => $"Desant ({fets / 1024d / 1024d:N1} de {total / 1024d / 1024d:N1} MB)…";

        private static bool EsElMateix(string a, string b)
            => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

        private static void EsborraLaCarpeta(string carpeta)
        {
            try
            {
                if (Directory.Exists(carpeta))
                    Directory.Delete(carpeta, recursive: true);
            }
            catch (Exception e)
            {
                Log.Error(e, "No s'ha pogut esborrar la carpeta temporal {Carpeta}", carpeta);
            }
        }

        private static OperationResult<T> Trenca<T>(string missatge)
            where T : CommonInterfaces.IEtiquetaDescripcio
            => new(new List<BrokenRule> { new(missatge) });
    }
}
