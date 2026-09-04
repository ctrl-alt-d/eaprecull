using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BusinessLayer.Common;
using Xunit;

namespace BusinessLayer.Integration.Test
{
    /// <summary>
    /// La peça petita i autònoma de la còpia: empaquetar amb contrasenya i tornar-ho a
    /// treure. Sense base de dades i sense magatzem.
    /// </summary>
    public class ZipXifratTest
    {
        private const string Password = "una contrasenya prou llarga";

        [Fact]
        public void AnadaITornadaAmbAccentsIElaGeminada()
        {
            using var banc = new Banc();

            var noms = new[] { "BaseDeDades.db", "Usuari.ini", "Aïda Vall·llobera.txt", "informació.txt" };
            var entrades = new Dictionary<string, string>();

            foreach (var nom in noms)
                entrades[nom] = banc.Fitxer(nom, $"contingut de {nom}");

            ZipXifrat.Escriu(banc.Zip, Password, entrades);
            ZipXifrat.Extreu(banc.Zip, Password, banc.Extraccio);

            foreach (var nom in noms)
                Assert.Equal($"contingut de {nom}",
                    File.ReadAllText(Path.Combine(banc.Extraccio, nom)));
        }

        [Fact]
        public void SenseLaContrasenyaBonaNoSEnTreuRes()
        {
            using var banc = new Banc();

            ZipXifrat.Escriu(banc.Zip, Password,
                new Dictionary<string, string> { ["dades.txt"] = banc.Fitxer("dades.txt", "secret") });

            Assert.ThrowsAny<Exception>(() => ZipXifrat.Extreu(banc.Zip, "una altra", banc.Extraccio));
            Assert.ThrowsAny<Exception>(() => ZipXifrat.Extreu(banc.Zip, string.Empty, banc.Extraccio));
        }

        [Fact]
        public void ElContingutNoEsLlegeixEnClarDinsDelZip()
        {
            using var banc = new Banc();

            const string secret = "NESE-informe-confidencial-alumne";

            ZipXifrat.Escriu(banc.Zip, Password,
                new Dictionary<string, string> { ["dades.txt"] = banc.Fitxer("dades.txt", secret) });

            var cru = Encoding.UTF8.GetString(File.ReadAllBytes(banc.Zip));

            Assert.DoesNotContain(secret, cru);
        }

        [Fact]
        public void UnZipDeCopiaSempreVaXifrat()
        {
            using var banc = new Banc();

            Assert.Throws<ArgumentException>(() => ZipXifrat.Escriu(
                banc.Zip, string.Empty,
                new Dictionary<string, string> { ["dades.txt"] = banc.Fitxer("dades.txt", "x") }));
        }

        [Fact]
        public void ElLlegeixMeDiuComRestaurarIQueLaContrasenyaNoEsRecupera()
        {
            var text = ZipXifrat.LlegeixMe("EAPRecull-BaseDeDades-2026-09-04_1215.zip",
                new DateTime(2026, 9, 4, 12, 15, 0), 1234);

            Assert.Contains("EAPRecull-BaseDeDades-2026-09-04_1215.zip", text);
            Assert.Contains("04/09/2026", text);
            Assert.Contains("1.234", text);
            Assert.Contains("Substitueix el fitxer BaseDeDades.db", text);
            Assert.Contains("7-Zip", text);
            Assert.Contains("Si perds la contrasenya", text);
        }

        private sealed class Banc : IDisposable
        {
            private readonly string _arrel;

            public Banc()
            {
                _arrel = Path.Combine(Path.GetTempPath(), $"eaprecull-zip-{Guid.NewGuid():N}");
                Extraccio = Path.Combine(_arrel, "extret");

                Directory.CreateDirectory(_arrel);
                Directory.CreateDirectory(Extraccio);

                Zip = Path.Combine(_arrel, "copia.zip");
            }

            public string Zip { get; }
            public string Extraccio { get; }

            public string Fitxer(string nom, string contingut)
            {
                var cami = Path.Combine(_arrel, "origen-" + nom);
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
