using System;
using System.IO;
using System.Threading.Tasks;
using BusinessLayer.Abstract.Generic;
using BusinessLayer.Common;
using BusinessLayer.DI;
using DataLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BusinessLayer.Integration.Test
{
    /// <summary>
    /// Les dades de l'usuari, exercitades sobre una carpeta temporal i sense contenidor:
    /// el servei no toca la base de dades i no en necessita cap.
    /// </summary>
    public class DadesUsuariTest
    {
        private static readonly DadesUsuari Valides =
            new("Dani", "Herrera", "dh@xtec.cat");

        [Fact]
        public void SenseFitxerNoHiHaDadesINoEsCreaRes()
        {
            using var carpeta = new CarpetaTemporal();
            var servei = new DadesDeLusuari(carpeta.Cami);

            Assert.False(servei.EstaInformat);
            Assert.Equal(DadesUsuari.Buides, servei.Actuals);

            // Un fitxer buit no aportaria res i es confondria amb un d'informat.
            Assert.False(File.Exists(servei.Ubicacio));
        }

        [Fact]
        public async Task DesarDeixaLesDadesLlestesPerALaProperaArrencada()
        {
            using var carpeta = new CarpetaTemporal();
            var servei = new DadesDeLusuari(carpeta.Cami);

            var resultat = await servei.Desa(Valides);

            Assert.Empty(resultat.BrokenRules);
            Assert.Equal(Valides, resultat.Data);
            Assert.True(servei.EstaInformat);
            Assert.Equal(Valides, servei.Actuals);

            // Anada i tornada: una instància nova sobre la mateixa carpeta ha de veure
            // el mateix, que és el cas real de la propera arrencada.
            var altra = new DadesDeLusuari(carpeta.Cami);

            Assert.True(altra.EstaInformat);
            Assert.Equal(Valides, altra.Actuals);
        }

        [Fact]
        public async Task ElsAccentsILaElaGeminadaSobreviuenLAnadaILaTornada()
        {
            using var carpeta = new CarpetaTemporal();
            var dades = new DadesUsuari("Núria", "Pallàs i Cel·lera", "npallas@edu.gencat.cat");

            await new DadesDeLusuari(carpeta.Cami).Desa(dades);

            Assert.Equal(dades, new DadesDeLusuari(carpeta.Cami).Actuals);
        }

        [Theory]
        [InlineData("", "Herrera", "dh@xtec.cat")]
        [InlineData("Dani", "", "dh@xtec.cat")]
        [InlineData("Dani", "Herrera", "")]
        [InlineData("Dani", "Herrera", "això no és una adreça")]
        public async Task UnaDadaInvalidaNoCanviaNiElDiscNiLaMemoria(
            string nom, string cognoms, string adreca)
        {
            using var carpeta = new CarpetaTemporal();
            var servei = new DadesDeLusuari(carpeta.Cami);

            await servei.Desa(Valides);
            var abans = File.ReadAllText(servei.Ubicacio);

            var resultat = await servei.Desa(new DadesUsuari(nom, cognoms, adreca));

            Assert.NotEmpty(resultat.BrokenRules);
            Assert.Null(resultat.Data);
            Assert.Equal(Valides, servei.Actuals);
            Assert.Equal(abans, File.ReadAllText(servei.Ubicacio));
        }

        [Fact]
        public async Task DesarConservaLesSeccionsILesClausAlienes()
        {
            using var carpeta = new CarpetaTemporal();
            var servei = new DadesDeLusuari(carpeta.Cami);

            File.WriteAllText(servei.Ubicacio,
                """
                [Usuari]
                Nom=Vell
                ClauQueNoConeixem=La conservem

                [SeccioDUnAltre]
                Quelcom=42
                """);

            await servei.Desa(Valides);

            var escrit = File.ReadAllText(servei.Ubicacio);

            Assert.Contains("ClauQueNoConeixem=La conservem", escrit);
            Assert.Contains("[SeccioDUnAltre]", escrit);
            Assert.Contains("Quelcom=42", escrit);
            Assert.Contains("Nom=Dani", escrit);
            Assert.DoesNotContain("Nom=Vell", escrit);
        }

        [Fact]
        public void UnFitxerEscombrariaNoImpedeixArrencar()
        {
            using var carpeta = new CarpetaTemporal();
            var servei = new DadesDeLusuari(carpeta.Cami);

            File.WriteAllText(servei.Ubicacio,
                "no és un ini\n[ mig obert\n=sense clau\n\0\0\0");

            Assert.False(servei.EstaInformat);
            Assert.Equal(DadesUsuari.Buides, servei.Actuals);
        }

        [Fact]
        public void ElServeiEsSingletonIElContenidorElResol()
        {
            // El registre és a BusinessLayerConfigureServices, i per tant les dues eines
            // de línia d'ordres també el tenen sense tocar-les.
            var serveis = new ServiceCollection();
            serveis.AddDbContextFactory<AppDbContext>(opt => opt.UseSqlite("Data Source=:memory:"));
            serveis.BusinessLayerConfigureServices();

            var descriptor = Assert.Single(serveis, d => d.ServiceType == typeof(IDadesDeLusuari));
            Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);

            using var provider = serveis.BuildServiceProvider(validateScopes: true);

            var servei = provider.GetRequiredService<IDadesDeLusuari>();

            Assert.IsType<DadesDeLusuari>(servei);
            Assert.Same(servei, provider.GetRequiredService<IDadesDeLusuari>());
        }

        /// <summary>Una carpeta per a un sol test, esborrada en acabar.</summary>
        private sealed class CarpetaTemporal : IDisposable
        {
            public CarpetaTemporal()
            {
                Cami = Path.Combine(Path.GetTempPath(), $"eaprecull-{Guid.NewGuid():N}");
                Directory.CreateDirectory(Cami);
            }

            public string Cami { get; }

            public void Dispose()
            {
                try
                {
                    Directory.Delete(Cami, recursive: true);
                }
                catch (IOException)
                {
                    // Que no es pugui esborrar la carpeta no ha de fer fallar cap test.
                }
            }
        }
    }
}
