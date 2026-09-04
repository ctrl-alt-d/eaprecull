using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BusinessLayer.Abstract.Services;
using BusinessLayer.Common;
using BusinessLayer.Services;
using DataLayer;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Parms = DTO.i.DTOs;
using Xunit;

namespace BusinessLayer.Integration.Test
{
    /// <summary>
    /// L'operació sencera contra un magatzem fals: bolcat, verificació, xifratge, ordre de
    /// les passes i retenció. <strong>Cap test toca la xarxa ni obre cap navegador.</strong>
    /// </summary>
    public class CopiaDeSeguretatTest
    {
        private const string Password = "una contrasenya prou llarga";

        [Fact]
        public async Task LaCopiaAcabaBeIElMagatzemRepUnFitxer()
        {
            using var banc = await Banc.Nou(actuacions: 3);

            var resultat = await banc.Operacio.Run(Password);

            Assert.Empty(resultat.BrokenRules);
            Assert.NotNull(resultat.Data);
            Assert.Equal(3, resultat.Data!.Actuacions);
            Assert.Null(resultat.Data.Avis);

            Assert.Single(banc.Magatzem.Fitxers);
            Assert.Equal(resultat.Data.Nom, banc.Magatzem.Fitxers[0]);
        }

        /// <summary>
        /// El test que val per tots: prova el bolcat, el xifratge i la restauració alhora.
        /// Si aquest és verd, una còpia de debò es pot obrir i tornar a posar.
        /// </summary>
        [Fact]
        public async Task ElZipEsXifratIElBolcatDeDinsTeLesMateixesActuacions()
        {
            using var banc = await Banc.Nou(actuacions: 5);

            var resultat = await banc.Operacio.Run(Password);
            Assert.Empty(resultat.BrokenRules);

            var zip = Path.Combine(banc.Magatzem.Carpeta, resultat.Data!.Nom);

            // Sense contrasenya no s'ha de poder llegir res de dins.
            Assert.ThrowsAny<Exception>(() => ZipXifrat.Extreu(zip, string.Empty, banc.Extraccio));

            ZipXifrat.Extreu(zip, Password, banc.Extraccio);

            var restaurada = Path.Combine(banc.Extraccio, "BaseDeDades.db");
            Assert.True(File.Exists(restaurada));

            Assert.Equal(5, ActuacionsDe(restaurada));
        }

        [Fact]
        public async Task AmbLaContrasenyaEquivocadaLaLecturaFalla()
        {
            using var banc = await Banc.Nou(actuacions: 1);

            var resultat = await banc.Operacio.Run(Password);
            Assert.Empty(resultat.BrokenRules);

            var zip = Path.Combine(banc.Magatzem.Carpeta, resultat.Data!.Nom);

            Assert.ThrowsAny<Exception>(() => ZipXifrat.Extreu(zip, "una altra de ben diferent", banc.Extraccio));
        }

        [Fact]
        public async Task ElNomSegueixElPatroCarpetaDeDadesData()
        {
            using var banc = await Banc.Nou(actuacions: 1);

            var resultat = await banc.Operacio.Run(Password);

            Assert.Matches(
                new Regex(@"^EAPRecull-BaseDeDades-\d{4}-\d{2}-\d{2}_\d{4}\.zip$"),
                resultat.Data!.Nom);
        }

        [Fact]
        public async Task ElZipDuElLlegeixMeAmbLesInstruccionsDeRestauracio()
        {
            using var banc = await Banc.Nou(actuacions: 2);

            var resultat = await banc.Operacio.Run(Password);
            ZipXifrat.Extreu(
                Path.Combine(banc.Magatzem.Carpeta, resultat.Data!.Nom), Password, banc.Extraccio);

            var llegeixMe = Path.Combine(banc.Extraccio, "LLEGEIX-ME.txt");
            Assert.True(File.Exists(llegeixMe));

            var text = File.ReadAllText(llegeixMe);
            Assert.Contains("COM RESTAURAR-LA", text);
            Assert.Contains("BaseDeDades.db", text);
        }

        [Fact]
        public async Task LaQuartaCopiaEnDeixaTresIRetiraLaMesAntiga()
        {
            using var banc = await Banc.Nou(actuacions: 1);

            var velles = new[] { "2020-01-01_0900", "2021-01-01_0900", "2022-01-01_0900" };

            foreach (var (marca, i) in velles.Select((m, i) => (m, i)))
            {
                var cami = Path.Combine(banc.Magatzem.Carpeta, $"EAPRecull-BaseDeDades-{marca}.zip");
                File.WriteAllText(cami, "còpia vella");
                File.SetLastWriteTime(cami, new DateTime(2020 + i, 1, 1, 9, 0, 0));
            }

            var resultat = await banc.Operacio.Run(Password);

            Assert.Empty(resultat.BrokenRules);
            Assert.Null(resultat.Data!.Avis);

            var queden = banc.Magatzem.Fitxers;

            Assert.Equal(3, queden.Count);
            Assert.Contains(resultat.Data.Nom, queden);
            Assert.DoesNotContain("EAPRecull-BaseDeDades-2020-01-01_0900.zip", queden);
            Assert.Contains("EAPRecull-BaseDeDades-2022-01-01_0900.zip", queden);
        }

        [Fact]
        public async Task SiElDesatFallaNoEsRetiraRes()
        {
            using var banc = await Banc.Nou(actuacions: 1);

            banc.Magatzem.FallaAlDesar = "el llapis no hi és";

            var resultat = await banc.Operacio.Run(Password);

            Assert.Null(resultat.Data);
            Assert.Contains(resultat.BrokenRules, r => r.Message.Contains("el llapis no hi és"));
            Assert.Equal(0, banc.Magatzem.VegadesQueSHaRetirat);
        }

        [Fact]
        public async Task UnaFallidaDeRetencioNoFaFallarLaCopia()
        {
            using var banc = await Banc.Nou(actuacions: 1);

            banc.Magatzem.FallaAlRetirar = "el fitxer està obert";

            var resultat = await banc.Operacio.Run(Password);

            Assert.Empty(resultat.BrokenRules);
            Assert.NotNull(resultat.Data);
            Assert.NotNull(resultat.Data!.Avis);
            Assert.Single(banc.Magatzem.Fitxers);
        }

        [Fact]
        public async Task SiElMagatzemNoEsPotPrepararNoEsDesaRes()
        {
            using var banc = await Banc.Nou(actuacions: 1);

            banc.Magatzem.FallaAlPreparar = "No es troba la carpeta E:\\Copies.";

            var resultat = await banc.Operacio.Run(Password);

            Assert.Null(resultat.Data);
            Assert.Contains(resultat.BrokenRules, r => r.Message.Contains("No es troba la carpeta"));
            Assert.Equal(0, banc.Magatzem.VegadesQueSHaDesat);
            Assert.Empty(banc.Magatzem.Fitxers);
        }

        [Fact]
        public async Task LesBrokenRulesDelMagatzemArribenTalQualINoEsLlancaRes()
        {
            using var banc = await Banc.Nou(actuacions: 1);

            banc.Magatzem.FallaAlDesar = "No hi ha prou espai per desar la còpia.";

            var resultat = await banc.Operacio.Run(Password);

            Assert.Equal("No hi ha prou espai per desar la còpia.",
                Assert.Single(resultat.BrokenRules).Message);
        }

        [Fact]
        public async Task SenseContrasenyaNoEsFaCapCopia()
        {
            using var banc = await Banc.Nou(actuacions: 1);

            var resultat = await banc.Operacio.Run(string.Empty);

            Assert.Null(resultat.Data);
            Assert.NotEmpty(resultat.BrokenRules);
            Assert.Empty(banc.Magatzem.Fitxers);
        }

        [Fact]
        public async Task UnTokenJaCancelatAcabaEnBrokenRuleISenseFitxersPelMig()
        {
            using var banc = await Banc.Nou(actuacions: 1);

            using var cancelat = new CancellationTokenSource();
            await cancelat.CancelAsync();

            var resultat = await banc.Operacio.Run(Password, null, cancelat.Token);

            Assert.Null(resultat.Data);
            Assert.NotEmpty(resultat.BrokenRules);
            Assert.Empty(banc.Magatzem.Fitxers);
        }

        [Fact]
        public async Task LaCarpetaTemporalNoDeixaCapBaseDeDadesEnClar()
        {
            using var banc = await Banc.Nou(actuacions: 1);

            var abans = TemporalsDEapRecull();

            await banc.Operacio.Run(Password);
            Assert.Equal(abans, TemporalsDEapRecull());

            // I igual quan peta: el finally va abans que el retorn de l'error.
            banc.Magatzem.FallaAlDesar = "peta";
            await banc.Operacio.Run(Password);
            Assert.Equal(abans, TemporalsDEapRecull());
        }

        [Fact]
        public async Task ElProgresExplicaQueEstaPassant()
        {
            using var banc = await Banc.Nou(actuacions: 1);

            var passes = new List<string>();
            await banc.Operacio.Run(Password, new Progress<string>(passes.Add));

            // El Progress<T> del .NET publica al context de sincronització i pot arribar
            // tard; el que es comprova és que n'hi hagi arribat alguna, no quantes.
            await Task.Delay(50);

            Assert.NotEmpty(passes);
        }

        [Fact]
        public async Task LaDataDeLaDarreraCopiaQuedaDesadaALUsuariIni()
        {
            using var banc = await Banc.Nou(actuacions: 1);

            Assert.Null(banc.Operacio.DarreraCopia);

            await banc.Operacio.Run(Password);

            Assert.NotNull(banc.Operacio.DarreraCopia);
            Assert.Contains("[CopiaDeSeguretat]", File.ReadAllText(banc.Usuari.Ubicacio));
        }

        [Fact]
        public async Task AmbUnSolDestiNoCalTriarNeCap()
        {
            using var banc = await Banc.Nou(actuacions: 1);

            Assert.Single(banc.Operacio.Destins);
            Assert.Equal(banc.Magatzem.Clau, banc.Operacio.ClauDelDestiActual);
            Assert.True(banc.Operacio.DestiActual.Configurat);
        }

        [Fact]
        public async Task ConfiguraUnDestiQueNoExisteixNoLlanca()
        {
            using var banc = await Banc.Nou(actuacions: 0);

            var resultat = await banc.Operacio.ConfiguraDesti("un-que-no-hi-es", null);

            Assert.Null(resultat.Data);
            Assert.NotEmpty(resultat.BrokenRules);
        }

        [Fact]
        public async Task LlistaTornaLesCopiesDeMesNovaAMesVella()
        {
            using var banc = await Banc.Nou(actuacions: 1);

            foreach (var (marca, any) in new[] { ("2021-01-01_0900", 2021), ("2023-01-01_0900", 2023) })
            {
                var cami = Path.Combine(banc.Magatzem.Carpeta, $"EAPRecull-BaseDeDades-{marca}.zip");
                File.WriteAllText(cami, "x");
                File.SetLastWriteTime(cami, new DateTime(any, 1, 1, 9, 0, 0));
            }

            var llistat = await banc.Operacio.Llista();

            Assert.Empty(llistat.BrokenRules);
            Assert.Equal(
                new[] { "EAPRecull-BaseDeDades-2023-01-01_0900.zip", "EAPRecull-BaseDeDades-2021-01-01_0900.zip" },
                llistat.Data!.Items.Select(c => c.Nom));
        }

        // -- Quan es proposa una còpia en arrencar ------------------------------------

        [Fact]
        public async Task SenseActuacionsNoEsProposaCapCopia()
        {
            using var banc = await Banc.Nou(actuacions: 0);

            var proposta = await banc.Operacio.CalFerCopia();

            Assert.False(proposta.Data!.Cal);
        }

        [Fact]
        public async Task SiNoSHaFetMaiCapCopiaIHiHaActuacionsEsProposa()
        {
            using var banc = await Banc.Nou(actuacions: 4);

            var proposta = await banc.Operacio.CalFerCopia();

            Assert.True(proposta.Data!.Cal);
            Assert.True(proposta.Data.MaiSHaFetCap);
            Assert.Equal(4, proposta.Data.ActuacionsNoves);
            Assert.Contains("Encara no has fet cap còpia", proposta.Data.Descripcio);
            Assert.Contains("4 actuacions noves", proposta.Data.Descripcio);
        }

        [Fact]
        public async Task AcabadaDeFerLaCopiaJaNoEsProposaCapAltra()
        {
            using var banc = await Banc.Nou(actuacions: 4);

            await banc.Operacio.Run(Password);

            var proposta = await banc.Operacio.CalFerCopia();

            Assert.False(proposta.Data!.Cal);
        }

        [Fact]
        public async Task PassadesDuesSetmanesSenseActuacionsNovesTampocEsProposa()
        {
            using var banc = await Banc.Nou(actuacions: 4);

            await banc.Operacio.Run(Password);
            banc.FesVeureQueLaCopiaEsDe(DateTime.Now.AddDays(-40));

            var proposta = await banc.Operacio.CalFerCopia();

            // Aquest és el cas que el fa suportable: sense feina nova no hi ha res a
            // perdre, i una proposta que surt sense motiu ensenya a ignorar-la.
            Assert.False(proposta.Data!.Cal);
        }

        [Fact]
        public async Task PassadesDuesSetmanesIAmbActuacionsNovesEsProposa()
        {
            using var banc = await Banc.Nou(actuacions: 4);

            await banc.Operacio.Run(Password);
            banc.FesVeureQueLaCopiaEsDe(DateTime.Now.AddDays(-23));

            await Banc.AfegeixActuacions(banc.Entorn, quantes: 3);

            var proposta = await banc.Operacio.CalFerCopia();

            Assert.True(proposta.Data!.Cal);
            Assert.False(proposta.Data.MaiSHaFetCap);
            Assert.Equal(3, proposta.Data.ActuacionsNoves);
            Assert.Equal(23, proposta.Data.Dies);
            Assert.Contains("Han passat 23 dies", proposta.Data.Descripcio);
            Assert.Contains("3 actuacions noves", proposta.Data.Descripcio);
        }

        [Fact]
        public async Task AmbActuacionsNovesPeroDinsDeLesDuesSetmanesNoEsProposa()
        {
            using var banc = await Banc.Nou(actuacions: 4);

            await banc.Operacio.Run(Password);
            banc.FesVeureQueLaCopiaEsDe(DateTime.Now.AddDays(-13));

            await Banc.AfegeixActuacions(banc.Entorn, quantes: 5);

            var proposta = await banc.Operacio.CalFerCopia();

            Assert.False(proposta.Data!.Cal);
        }

        [Fact]
        public async Task ElSingularIElPluralEstanBe()
        {
            using var banc = await Banc.Nou(actuacions: 1);

            await banc.Operacio.Run(Password);
            banc.FesVeureQueLaCopiaEsDe(DateTime.Now.AddDays(-15));

            await Banc.AfegeixActuacions(banc.Entorn, quantes: 1);

            var descripcio = (await banc.Operacio.CalFerCopia()).Data!.Descripcio;

            Assert.Contains("1 actuació nova", descripcio);
            Assert.DoesNotContain("1 actuacions", descripcio);
            Assert.Contains("15 dies", descripcio);
        }

        [Fact]
        public async Task ElRecompteDeLaDarreraCopiaQuedaDesatALUsuariIni()
        {
            using var banc = await Banc.Nou(actuacions: 6);

            await banc.Operacio.Run(Password);

            Assert.Contains("ActuacionsDeLaDarreraCopia=6", File.ReadAllText(banc.Usuari.Ubicacio));
        }

        /// <summary>
        /// El camí sencer de debò: la base de dades en un fitxer, l'adaptador de carpeta
        /// real i el zip obert amb la contrasenya. Els altres tests d'aquest fitxer usen el
        /// doble per poder fer fallar el destí; aquest no en fa servir cap.
        /// </summary>
        [Fact]
        public async Task DeCapAPeuAmbLAdaptadorDeCarpetaDeDebo()
        {
            using var entorn = EntornDeTest.NouEnFitxer();
            using var carpetes = new CarpetesDeTest();

            await Banc.Sembra(entorn, actuacions: 7);

            var usuari = new DadesDeLusuari(carpetes.Dades);
            var magatzem = new MagatzemDeCarpeta(usuari);

            using var operacio = new CopiaDeSeguretat(
                entorn.GetRequiredService<IDbContextFactory<AppDbContext>>(), [magatzem], usuari);

            var configurat = await operacio.ConfiguraDesti(MagatzemDeCarpeta.ClauDelDesti, carpetes.Copies);
            Assert.Empty(configurat.BrokenRules);

            var resultat = await operacio.Run(Password);

            Assert.Empty(resultat.BrokenRules);
            Assert.Equal(7, resultat.Data!.Actuacions);

            var zip = Path.Combine(carpetes.Copies, resultat.Data.Nom);
            Assert.True(File.Exists(zip));

            ZipXifrat.Extreu(zip, Password, carpetes.Extraccio);
            Assert.Equal(7, ActuacionsDe(Path.Combine(carpetes.Extraccio, "BaseDeDades.db")));

            // I l'Usuari.ini hi va a dins: la còpia ha de completar la restauració.
            Assert.True(File.Exists(Path.Combine(carpetes.Extraccio, "Usuari.ini")));

            var llistat = await operacio.Llista();
            Assert.Equal(resultat.Data.Nom, Assert.Single(llistat.Data!.Items).Nom);
        }

        // -- Utillatge ----------------------------------------------------------------

        private static int ActuacionsDe(string baseDeDades)
        {
            var cadena = new SqliteConnectionStringBuilder
            {
                DataSource = baseDeDades,
                Mode = SqliteOpenMode.ReadOnly,
            }.ToString();

            using var connexio = new SqliteConnection(cadena);
            connexio.Open();

            using var ordre = connexio.CreateCommand();
            ordre.CommandText = "SELECT COUNT(*) FROM Actuacions;";

            return Convert.ToInt32(ordre.ExecuteScalar());
        }

        private static int TemporalsDEapRecull()
            => Directory.EnumerateDirectories(Path.GetTempPath(), "EapRecull-*").Count();

        /// <summary>Les carpetes que necessita el test de cap a peu.</summary>
        private sealed class CarpetesDeTest : IDisposable
        {
            private readonly string _arrel;

            public CarpetesDeTest()
            {
                _arrel = Path.Combine(Path.GetTempPath(), $"eaprecull-e2e-{Guid.NewGuid():N}");

                Dades = Path.Combine(_arrel, "Dades");
                Copies = Path.Combine(_arrel, "Copies");
                Extraccio = Path.Combine(_arrel, "extret");

                foreach (var carpeta in new[] { Dades, Copies, Extraccio })
                    Directory.CreateDirectory(carpeta);
            }

            public string Dades { get; }
            public string Copies { get; }
            public string Extraccio { get; }

            public void Dispose()
            {
                try
                {
                    Directory.Delete(_arrel, recursive: true);
                }
                catch (IOException)
                {
                    // Un temporal que es queda no ha de fer vermell cap test.
                }
            }
        }

        /// <summary>
        /// Una base de dades amb dades, un magatzem fals i l'operació muntada a sobre. Es
        /// construeix a mà i no pel contenidor perquè el magatzem és el doble, no el de
        /// debò; que el contenidor la sàpiga resoldre ho comprova <c>InjeccioTest</c>.
        /// </summary>
        private sealed class Banc : IDisposable
        {
            private readonly EntornDeTest _entorn;
            private readonly string _carpetaDeLIni;

            private Banc(EntornDeTest entorn, MagatzemFals magatzem, DadesDeLusuari usuari,
                         ICopiaDeSeguretat operacio, string carpetaDeLIni, string extraccio)
            {
                _entorn = entorn;
                _carpetaDeLIni = carpetaDeLIni;
                Magatzem = magatzem;
                Usuari = usuari;
                Operacio = operacio;
                Extraccio = extraccio;
            }

            public EntornDeTest Entorn => _entorn;
            public MagatzemFals Magatzem { get; }
            public DadesDeLusuari Usuari { get; }
            public ICopiaDeSeguretat Operacio { get; }
            public string Extraccio { get; }

            public static async Task<Banc> Nou(int actuacions)
            {
                // En fitxer i no en memòria: el bolcat es fa amb VACUUM INTO, i el
                // VACUUM de SQLite no fa res sobre una base de dades en memòria.
                var entorn = EntornDeTest.NouEnFitxer();

                await Sembra(entorn, actuacions);

                var carpetaDeLIni = Path.Combine(Path.GetTempPath(), $"eaprecull-ini-{Guid.NewGuid():N}");
                var extraccio = Path.Combine(Path.GetTempPath(), $"eaprecull-zip-{Guid.NewGuid():N}");
                Directory.CreateDirectory(carpetaDeLIni);
                Directory.CreateDirectory(extraccio);

                var magatzem = new MagatzemFals();
                var usuari = new DadesDeLusuari(carpetaDeLIni);

                var operacio = new CopiaDeSeguretat(
                    entorn.GetRequiredService<IDbContextFactory<AppDbContext>>(),
                    [magatzem],
                    usuari);

                return new Banc(entorn, magatzem, usuari, operacio, carpetaDeLIni, extraccio);
            }

            /// <summary>
            /// Envelleix la data de l'última còpia a l'<c>Usuari.ini</c>, que és l'única
            /// manera de provar el llindar de dues setmanes sense esperar-les.
            /// </summary>
            public void FesVeureQueLaCopiaEsDe(DateTime quan)
            {
                var ini = FitxerIni.Llegeix(Usuari.Ubicacio);
                ini.Assigna("CopiaDeSeguretat", "DarreraCopia",
                    quan.ToString("s", CultureInfo.InvariantCulture));
                ini.Desa(Usuari.Ubicacio);
            }

            /// <summary>Les sis entitats que calen per poder crear una actuació.</summary>
            public static async Task Sembra(EntornDeTest entorn, int actuacions)
            {
                if (actuacions == 0)
                    return;

                using var creaCentre = entorn.GetRequiredService<ICentreCreate>();
                using var creaCurs = entorn.GetRequiredService<ICursAcademicCreate>();
                using var creaEtapa = entorn.GetRequiredService<IEtapaCreate>();
                using var creaTipus = entorn.GetRequiredService<ITipusActuacioCreate>();
                using var creaAlumne = entorn.GetRequiredService<IAlumneCreate>();
                using var creaActuacio = entorn.GetRequiredService<IActuacioCreate>();

                var centre = (await creaCentre.Create(new Parms.CentreCreateParms("M1", "Les Melies", true))).Data!;
                var curs = (await creaCurs.Create(new Parms.CursAcademicCreateParms(2026, true))).Data!;
                var etapa = (await creaEtapa.Create(new Parms.EtapaCreateParms("PRI", "Primària", true, true))).Data!;
                var tipus = (await creaTipus.Create(new Parms.TipusActuacioCreateParms("AL", "Altres", true))).Data!;

                var alumne = (await creaAlumne.Create(new Parms.AlumneCreateParms(
                    "Aïda", "Vall·llobera", DateTime.Now.AddYears(-10), centre.Id, curs.Id,
                    etapa.Id, "3r", null, string.Empty, null, string.Empty, string.Empty))).Data!;

                for (var i = 0; i < actuacions; i++)
                    await creaActuacio.Create(new Parms.ActuacioCreateParms(
                        alumne.Id, tipus.Id, string.Empty, DateTime.Now.AddDays(-i), curs.Id,
                        centre.Id, etapa.Id, "3r", 30, $"Actuació {i}"));
            }

            /// <summary>
            /// Actuacions noves sobre les entitats que ja hi ha. <c>Sembra</c> no es pot
            /// cridar dues vegades —tornaria a crear el mateix centre i el mateix alumne—,
            /// i el que aquests tests necessiten és feina nova, no dades noves.
            /// </summary>
            public static async Task AfegeixActuacions(EntornDeTest entorn, int quantes)
            {
                using var centres = entorn.GetRequiredService<ICentreSet>();
                using var cursos = entorn.GetRequiredService<ICursAcademicSet>();
                using var etapes = entorn.GetRequiredService<IEtapaSet>();
                using var tipus = entorn.GetRequiredService<ITipusActuacioSet>();
                using var alumnes = entorn.GetRequiredService<IAlumneSet>();
                using var crea = entorn.GetRequiredService<IActuacioCreate>();

                var centre = (await centres.FromPredicate(new Parms.EsActiuParms(null))).Data!.First();
                var curs = (await cursos.FromPredicate(new Parms.EsActiuParms(null))).Data!.First();
                var etapa = (await etapes.FromPredicate(new Parms.EsActiuParms(null))).Data!.First();
                var tipusActuacio = (await tipus.FromPredicate(new Parms.EsActiuParms(null))).Data!.First();
                var alumne = (await alumnes.FromPredicate(new Parms.AlumneSearchParms())).Data!.First();

                for (var i = 0; i < quantes; i++)
                {
                    var resultat = await crea.Create(new Parms.ActuacioCreateParms(
                        alumne.Id, tipusActuacio.Id, string.Empty, DateTime.Now, curs.Id,
                        centre.Id, etapa.Id, "3r", 30, $"Actuació nova {Guid.NewGuid():N}"));

                    Assert.Empty(resultat.BrokenRules);
                }
            }

            public void Dispose()
            {
                Operacio.Dispose();
                Magatzem.Dispose();
                _entorn.Dispose();

                Esborra(_carpetaDeLIni);
                Esborra(Extraccio);
            }

            private static void Esborra(string carpeta)
            {
                try
                {
                    if (Directory.Exists(carpeta))
                        Directory.Delete(carpeta, recursive: true);
                }
                catch (IOException)
                {
                    // Un temporal que es queda no ha de fer vermell cap test.
                }
            }
        }
    }
}
