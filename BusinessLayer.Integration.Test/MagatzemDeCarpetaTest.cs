using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusinessLayer.Common;
using Xunit;

namespace BusinessLayer.Integration.Test
{
    /// <summary>
    /// L'adaptador de carpeta, sense base de dades: no en necessita cap. El que es vigila
    /// aquí és sobretot el que <strong>no</strong> ha de fer —tocar fitxers que no són
    /// seus— i que cap error de disc surti en forma d'excepció.
    /// </summary>
    public class MagatzemDeCarpetaTest
    {
        [Fact]
        public async Task SenseCarpetaConfiguradaNiPreparaNiDesa()
        {
            using var banc = new Banc();

            Assert.False(banc.Magatzem.Desti.Configurat);

            var preparat = await banc.Magatzem.Prepara(CancellationToken.None);
            Assert.NotEmpty(preparat.BrokenRules);

            var desat = await banc.Magatzem.Desa("qualsevol", "x.zip", null, CancellationToken.None);
            Assert.NotEmpty(desat.BrokenRules);
        }

        [Fact]
        public async Task ConfigurarDeixaLaCarpetaDesadaPerALaProperaArrencada()
        {
            using var banc = new Banc();

            var resultat = await banc.Magatzem.Configura(banc.Copies, CancellationToken.None);

            Assert.Empty(resultat.BrokenRules);
            Assert.Equal(banc.Copies, resultat.Data!.Detall);

            // Una instància nova sobre el mateix Usuari.ini ha de veure el mateix: és el
            // cas real de la propera vegada que s'obre el programa.
            var altre = new MagatzemDeCarpeta(banc.Usuari);

            Assert.True(altre.Desti.Configurat);
            Assert.Equal(banc.Copies, altre.Desti.Detall);
        }

        [Fact]
        public async Task UnaCarpetaQueNoExisteixEsBrokenRuleINoExcepcio()
        {
            using var banc = new Banc();

            var inexistent = Path.Combine(banc.Copies, "no-hi-es");

            var resultat = await banc.Magatzem.Configura(inexistent, CancellationToken.None);

            Assert.Null(resultat.Data);
            Assert.Contains(resultat.BrokenRules, r => r.Message.Contains("No es troba la carpeta"));
        }

        [Fact]
        public async Task PrepararUnaCarpetaQueHaDesaparegutEsBrokenRule()
        {
            using var banc = new Banc();
            await banc.Magatzem.Configura(banc.Copies, CancellationToken.None);

            // El llapis que algú ha desendollat.
            Directory.Delete(banc.Copies, recursive: true);

            var resultat = await banc.Magatzem.Prepara(CancellationToken.None);

            Assert.Null(resultat.Data);
            Assert.Contains(resultat.BrokenRules,
                r => r.Message.Contains("Comprova que el llapis o la unitat de xarxa hi siguin"));
        }

        [Fact]
        public async Task DesarDeixaElFitxerSencerINoCapTemporal()
        {
            using var banc = new Banc();
            await banc.Magatzem.Configura(banc.Copies, CancellationToken.None);

            var origen = banc.FitxerDe("contingut de la còpia");
            var progres = 0L;

            var resultat = await banc.Magatzem.Desa(
                origen, "EAPRecull-BaseDeDades-2026-09-04_1215.zip",
                new Progress<long>(b => progres = b), CancellationToken.None);

            Assert.Empty(resultat.BrokenRules);

            var desat = Path.Combine(banc.Copies, "EAPRecull-BaseDeDades-2026-09-04_1215.zip");
            Assert.True(File.Exists(desat));
            Assert.Equal("contingut de la còpia", File.ReadAllText(desat));

            // L'escriptura és atòmica: el .tmp intermedi no s'hi pot quedar.
            Assert.Empty(Directory.EnumerateFiles(banc.Copies, "*" + NomDeCopia.ExtensioTemporal));
        }

        [Fact]
        public async Task LlistaIgnoraElsFitxersQueNoSonNostres()
        {
            using var banc = new Banc();
            await banc.Magatzem.Configura(banc.Copies, CancellationToken.None);

            // La carpeta que tria l'usuari pot tenir-hi el que sigui.
            File.WriteAllText(Path.Combine(banc.Copies, "Fotos de l'estiu.zip"), "x");
            File.WriteAllText(Path.Combine(banc.Copies, "Comptabilitat.xlsx"), "x");
            File.WriteAllText(Path.Combine(banc.Copies, "EAPRecull-BaseDeDades-2026-01-01_0900.zip"), "x");

            var llistat = await banc.Magatzem.Llista(CancellationToken.None);

            Assert.Empty(llistat.BrokenRules);
            Assert.Equal("EAPRecull-BaseDeDades-2026-01-01_0900.zip",
                Assert.Single(llistat.Data!.Items).Nom);
        }

        [Fact]
        public async Task RetiraSobrantsNoEsborraResQueNoHagiDesatEll()
        {
            using var banc = new Banc();
            await banc.Magatzem.Configura(banc.Copies, CancellationToken.None);

            var aliens = new[] { "Fotos de l'estiu.zip", "Comptabilitat.xlsx", "notes.txt" };

            foreach (var alie in aliens)
                File.WriteAllText(Path.Combine(banc.Copies, alie), "x");

            for (var any = 2020; any <= 2024; any++)
            {
                var cami = Path.Combine(banc.Copies, $"EAPRecull-BaseDeDades-{any}-01-01_0900.zip");
                File.WriteAllText(cami, "x");
                File.SetLastWriteTime(cami, new DateTime(any, 1, 1, 9, 0, 0));
            }

            var retirades = await banc.Magatzem.RetiraSobrants(3, CancellationToken.None);

            Assert.Empty(retirades.BrokenRules);
            Assert.Equal(2, retirades.Data!.Items.Count);

            foreach (var alie in aliens)
                Assert.True(File.Exists(Path.Combine(banc.Copies, alie)), $"S'ha tocat {alie}.");

            var queden = (await banc.Magatzem.Llista(CancellationToken.None)).Data!.Items
                .Select(c => c.Nom).ToList();

            Assert.Equal(3, queden.Count);
            Assert.DoesNotContain("EAPRecull-BaseDeDades-2020-01-01_0900.zip", queden);
            Assert.DoesNotContain("EAPRecull-BaseDeDades-2021-01-01_0900.zip", queden);
            Assert.Contains("EAPRecull-BaseDeDades-2024-01-01_0900.zip", queden);
        }

        [Fact]
        public async Task OblidarNoEsborraCapCopia()
        {
            using var banc = new Banc();
            await banc.Magatzem.Configura(banc.Copies, CancellationToken.None);

            var copia = Path.Combine(banc.Copies, "EAPRecull-BaseDeDades-2026-01-01_0900.zip");
            File.WriteAllText(copia, "x");

            var resultat = await banc.Magatzem.Oblida();

            Assert.Empty(resultat.BrokenRules);
            Assert.False(banc.Magatzem.Desti.Configurat);
            Assert.True(File.Exists(copia));
        }

        [Fact]
        public async Task ConfigurarSenseCarpetaHoDiuEnComptesDePetar()
        {
            using var banc = new Banc();

            foreach (var res in new[] { null, string.Empty, "   " })
            {
                var resultat = await banc.Magatzem.Configura(res, CancellationToken.None);

                Assert.Null(resultat.Data);
                Assert.NotEmpty(resultat.BrokenRules);
            }
        }

        // -- Utillatge ----------------------------------------------------------------

        private sealed class Banc : IDisposable
        {
            private readonly string _arrel;

            public Banc()
            {
                _arrel = Path.Combine(Path.GetTempPath(), $"eaprecull-mag-{Guid.NewGuid():N}");
                Copies = Path.Combine(_arrel, "Copies");
                var dades = Path.Combine(_arrel, "Dades");

                Directory.CreateDirectory(Copies);
                Directory.CreateDirectory(dades);

                Usuari = new DadesDeLusuari(dades);
                Magatzem = new MagatzemDeCarpeta(Usuari);
            }

            public string Copies { get; }
            public DadesDeLusuari Usuari { get; }
            public MagatzemDeCarpeta Magatzem { get; }

            public string FitxerDe(string contingut)
            {
                var cami = Path.Combine(_arrel, $"origen-{Guid.NewGuid():N}.zip");
                File.WriteAllText(cami, contingut);
                return cami;
            }

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
    }
}
