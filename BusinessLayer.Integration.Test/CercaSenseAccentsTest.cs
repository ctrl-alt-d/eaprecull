using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BusinessLayer.Abstract.Services;
using BusinessLayer.DI;
using DataLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BusinessLayer.Integration.Test
{
    /// <summary>
    /// SQLite compara el LIKE lletra a lletra, i «í» i «i» són lletres diferents: qui
    /// escriu «marti» al cercador no troba en «Martí». Ho resol
    /// <c>FuncionsSql.ConteSenseAccents</c>, que treu els accents dels dos costats de
    /// la comparació. Aquests tests fixen les dues direccions, que el LIKE segueixi
    /// ignorant majúscules i minúscules, i que els comodins escrits al cercador no
    /// arribin al patró.
    /// </summary>
    public class CercaSenseAccentsTest
    {
        [Theory]
        [InlineData("Martí")]      // tal com està desat
        [InlineData("marti")]      // sense accent i en minúscula
        [InlineData("MARTI")]      // sense accent i en majúscula
        [InlineData("MARTÍ")]      // amb accent i en majúscula
        [InlineData("martí")]
        public async Task TrobaLAlumneAmbAccentSiguiComSigui(string cercat)
        {
            using var provider = Provider();
            await UnAlumne(provider, nom: "Martí", cognoms: "Peñíscola Çabater");

            var trobats = await provider.GetRequiredService<IAlumneSet>()
                .FromPredicate(new DTO.i.DTOs.AlumneSearchParms(nomCognomsTagCentre: cercat));

            Assert.Single(trobats.Data!);
        }

        [Theory]
        [InlineData("peniscola")]  // ñ → n
        [InlineData("cabater")]    // ç → c
        [InlineData("Peñíscola")]
        public async Task TreuTambeLaEnyaILaCedella(string cercat)
        {
            using var provider = Provider();
            await UnAlumne(provider, nom: "Martí", cognoms: "Peñíscola Çabater");

            var trobats = await provider.GetRequiredService<IAlumneSet>()
                .FromPredicate(new DTO.i.DTOs.AlumneSearchParms(nomCognomsTagCentre: cercat));

            Assert.Single(trobats.Data!);
        }

        [Theory]
        [InlineData("%")]
        [InlineData("_")]
        [InlineData("Mart%")]
        public async Task ElsComodinsEscritsAlCercadorSonTextILlest(string cercat)
        {
            using var provider = Provider();
            await UnAlumne(provider, nom: "Martí", cognoms: "Peñíscola Çabater");

            var trobats = await provider.GetRequiredService<IAlumneSet>()
                .FromPredicate(new DTO.i.DTOs.AlumneSearchParms(nomCognomsTagCentre: cercat));

            Assert.Empty(trobats.Data!);
        }

        [Fact]
        public async Task NoAfluixaLaCercaFinsAFerQueTotHiLligui()
        {
            using var provider = Provider();
            await UnAlumne(provider, nom: "Martí", cognoms: "Peñíscola Çabater");

            var trobats = await provider.GetRequiredService<IAlumneSet>()
                .FromPredicate(new DTO.i.DTOs.AlumneSearchParms(nomCognomsTagCentre: "Ferran"));

            Assert.Empty(trobats.Data!);
        }

        [Theory]
        [InlineData("descripcio")]
        [InlineData("DESCRIPCIÓ")]
        public async Task LaCercaDActuacionsTambeIgnoraElsAccents(string cercat)
        {
            using var provider = Provider();
            await UnaActuacio(provider);

            var trobats = await provider.GetRequiredService<IActuacioSet>()
                .FromPredicate(new DTO.i.DTOs.ActuacioSearchParms(searchString: cercat));

            Assert.Single(trobats.Data!);
        }

        // --- Bastida ---------------------------------------------------------------

        private static async Task UnAlumne(IServiceProvider provider, string nom, string cognoms)
        {
            var (centre, curs, etapa, _) = await Mestres(provider);

            await provider.GetRequiredService<IAlumneCreate>()
                .Create(new DTO.i.DTOs.AlumneCreateParms(
                    nom, cognoms, null, centre, curs, etapa, "1r", null, "", null, "", ""));
        }

        private static async Task UnaActuacio(IServiceProvider provider)
        {
            var (centre, curs, etapa, tipus) = await Mestres(provider);

            var alumne = (await provider.GetRequiredService<IAlumneCreate>()
                .Create(new DTO.i.DTOs.AlumneCreateParms(
                    "Nom", "Cognoms", null, centre, curs, etapa, "1r",
                    null, "", null, "", ""))).Data!;

            await provider.GetRequiredService<IActuacioCreate>()
                .Create(new DTO.i.DTOs.ActuacioCreateParms(
                    alumne.Id, tipus, "", DateTime.Today, curs, centre, etapa,
                    "1r", 30, "Una descripció"));
        }

        private static async Task<(int Centre, int Curs, int Etapa, int Tipus)> Mestres(
            IServiceProvider provider)
        {
            var centre = (await provider.GetRequiredService<ICentreCreate>()
                .Create(new DTO.i.DTOs.CentreCreateParms("C", "Centre", true))).Data!;
            var curs = (await provider.GetRequiredService<ICursAcademicCreate>()
                .Create(new DTO.i.DTOs.CursAcademicCreateParms(2024, true))).Data!;
            var etapa = (await provider.GetRequiredService<IEtapaCreate>()
                .Create(new DTO.i.DTOs.EtapaCreateParms("ESO", "ESO", true, true))).Data!;
            var tipus = (await provider.GetRequiredService<ITipusActuacioCreate>()
                .Create(new DTO.i.DTOs.TipusActuacioCreateParms("T", "Tipus", true))).Data!;

            return (centre.Id, curs.Id, etapa.Id, tipus.Id);
        }

        private static ServiceProvider Provider()
        {
            var fitxer = Path.Combine(Path.GetTempPath(), "Esborrar" + Guid.NewGuid().ToString("N")[..8] + ".db");

            var services = new ServiceCollection();
            services.AddDbContextFactory<AppDbContext>(opt =>
                opt.UseSqlite($"Data Source={fitxer}")
                   .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));
            services.BusinessLayerConfigureServices();

            var provider = services.BuildServiceProvider();

            provider.GetRequiredService<IDbContextFactory<AppDbContext>>()
                .CreateDbContext()
                .Database
                .Migrate();

            return provider;
        }
    }
}
