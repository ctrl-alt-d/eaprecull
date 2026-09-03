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
    /// <c>FuncionsSql.Conte</c>, que treu els accents dels dos costats de
    /// la comparació. Aquests tests fixen les dues direccions, que el LIKE segueixi
    /// ignorant majúscules i minúscules, i que els comodins escrits al cercador no
    /// arribin al patró.
    /// </summary>
    public class ClauDeCercaTest
    {
        [Theory]
        [InlineData("Martí")]      // tal com està desat
        [InlineData("marti")]      // sense accent i en minúscula
        [InlineData("MARTI")]      // sense accent i en majúscula
        [InlineData("MARTÍ")]      // amb accent i en majúscula
        [InlineData("martí")]
        public async Task TrobaLAlumneAmbAccentSiguiComSigui(string cercat)
        {
            using var provider = EntornDeTest.Nou();
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
            using var provider = EntornDeTest.Nou();
            await UnAlumne(provider, nom: "Martí", cognoms: "Peñíscola Çabater");

            var trobats = await provider.GetRequiredService<IAlumneSet>()
                .FromPredicate(new DTO.i.DTOs.AlumneSearchParms(nomCognomsTagCentre: cercat));

            Assert.Single(trobats.Data!);
        }

        [Theory]
        [InlineData("l'Anna", "l'Anna")]      // apòstrof recte, el del teclat
        [InlineData("l'Anna", "l\u2019Anna")]  // el tipogràfic, el que hi posa el Word
        [InlineData("l\u2019Anna", "l'Anna")]
        [InlineData("l\u00B4Anna", "l'Anna")]  // accent agut fet servir d'apòstrof
        [InlineData("lAnna", "l'Anna")]       // sense apòstrof
        public async Task LApostrofSEscriuDeMoltesManeresIVolDirElMateix(string cercat, string desat)
        {
            using var provider = EntornDeTest.Nou();
            await UnAlumne(provider, nom: desat, cognoms: "Puig");

            var trobats = await provider.GetRequiredService<IAlumneSet>()
                .FromPredicate(new DTO.i.DTOs.AlumneSearchParms(nomCognomsTagCentre: cercat));

            Assert.Single(trobats.Data!);
        }

        [Theory]
        [InlineData("Parallel", "Paral\u00B7lel")]   // punt volat, ben escrit
        [InlineData("Paral.lel", "Paral\u00B7lel")]
        [InlineData("Paral\u00B7lel", "Para\u0140lel")] // ela geminada precomposada
        [InlineData("Vilareal", "Vila-real")]        // guions
        [InlineData("Vila-real", "Vila\u2013real")]  // guionet i guió mitjà
        [InlineData("Marti", "Mart\u00ADi")]         // guionet tou: invisible, ve de PDF
        [InlineData("Marti", "Mart\u200Bi")]         // amplada zero, igual d'invisible
        public async Task ElsSeparadorsInvisiblesIElsQueSEscriuenDeDuesManeres(string cercat, string desat)
        {
            using var provider = EntornDeTest.Nou();
            await UnAlumne(provider, nom: "Nom", cognoms: desat);

            var trobats = await provider.GetRequiredService<IAlumneSet>()
                .FromPredicate(new DTO.i.DTOs.AlumneSearchParms(nomCognomsTagCentre: cercat));

            Assert.Single(trobats.Data!);
        }

        [Theory]
        [InlineData("Lukasz", "\u0141ukasz")]   // la ela polonesa amb traç
        [InlineData("Walesa", "Wa\u0142\u0119sa")] // traç i ogonek alhora
        [InlineData("Strasse", "Stra\u00DFe")]  // ess-zet alemanya
        [InlineData("Oresund", "\u00D8resund")] // o nòrdica amb traç
        [InlineData("Encyclopaedia", "Encyclop\u00E6dia")]
        public async Task LesLletresAmbTracILesLigaturesTambe(string cercat, string desat)
        {
            using var provider = EntornDeTest.Nou();
            await UnAlumne(provider, nom: "Nom", cognoms: desat);

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
            using var provider = EntornDeTest.Nou();
            await UnAlumne(provider, nom: "Martí", cognoms: "Peñíscola Çabater");

            var trobats = await provider.GetRequiredService<IAlumneSet>()
                .FromPredicate(new DTO.i.DTOs.AlumneSearchParms(nomCognomsTagCentre: cercat));

            Assert.Empty(trobats.Data!);
        }

        [Fact]
        public async Task NoAfluixaLaCercaFinsAFerQueTotHiLligui()
        {
            using var provider = EntornDeTest.Nou();
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
            using var provider = EntornDeTest.Nou();
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

    }
}
