using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BusinessLayer.Abstract.Generic;
using BusinessLayer.Abstract.Services;
using BusinessLayer.Common;
using BusinessLayer.Services;
using BusinessLayer.DI;
using DataLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BusinessLayer.Integration.Test
{
    /// <summary>
    /// El bus de canvis emet des de les classes base d'escriptura i no des dels punts de
    /// crida. Aquests tests fixen les dues meitats d'aquesta decisió: que les bases emeten,
    /// i que cap operació concreta se salta l'emissió sobreescrivint el mètode que la fa.
    /// </summary>
    public class EmissioTest
    {
        /// <summary>
        /// Les classes base d'escriptura amb el mètode que ha de publicar. Per reflexió
        /// sobre les bases: una operació nova no s'hi ha d'afegir a mà enlloc.
        /// </summary>
        private static readonly (Type Base, string Metode)[] Bases =
        [
            (typeof(BLCreate<,,>), "Create"),
            (typeof(BLUpdate<,,>), "Update"),
            (typeof(BLDelete<,>), "Delete"),
            (typeof(BLActivaDesactiva<,>), "Update"),
            (typeof(BLBatchOperation<>), "ExecuteBatch"),
        ];

        [Fact]
        public void CadaClasseBaseDEscripturaPublicaAlBus()
        {
            var sense = Bases
                .Where(x => !Publica(MetodeDe(x.Base, x.Metode)))
                .Select(x => $"{x.Base.Name}.{x.Metode}")
                .ToList();

            Assert.True(sense.Count == 0,
                "Classes base d'escriptura que no publiquen cap canvi al bus: "
                + string.Join(", ", sense));
        }

        [Fact]
        public void CapOperacioSeSaltaLEmissioDeLaSevaBase()
        {
            // Els mètodes són virtuals: una operació que en sobreescrivís algun deixaria
            // d'emetre sense que res més se n'adonés.
            var problemes =
                (from operacio in Operacions()
                 from x in Bases
                 where HeretaDe(operacio, x.Base)
                 let metode = operacio.GetMethod(x.Metode,
                     BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                     | BindingFlags.DeclaredOnly)
                 where metode is not null
                 select $"{operacio.Name}.{x.Metode}")
                .ToList();

            Assert.True(problemes.Count == 0,
                "Operacions que sobreescriuen el mètode que publica el canvi: "
                + string.Join(", ", problemes));
        }

        [Fact]
        public async Task UnaAltaEmetLesReferenciesDelDtoCreat()
        {
            using var provider = EntornDeTest.Nou();
            var canvis = Escolta(provider);

            var centre = await provider.GetRequiredService<ICentreCreate>()
                .Create(new DTO.i.DTOs.CentreCreateParms(codi: "123", nom: "Pepe", esActiu: true));

            var canvi = Assert.Single(canvis);
            Assert.Equal(MenaDeCanvi.Alta, canvi.Mena);
            Assert.Equal([new Referencia(Entitat.Centre, centre.Data!.Id)], canvi.Afectats);
        }

        [Fact]
        public async Task UnaModificacioEmetLesReferenciesDAbansIDeDespres()
        {
            using var provider = EntornDeTest.Nou();

            var centre = await provider.GetRequiredService<ICentreCreate>()
                .Create(new DTO.i.DTOs.CentreCreateParms(codi: "123", nom: "Pepe", esActiu: true));

            var canvis = Escolta(provider);

            await provider.GetRequiredService<ICentreUpdate>()
                .Update(new DTO.i.DTOs.CentreUpdateParms(
                    id: centre.Data!.Id, codi: "123", nom: "Pepa", esActiu: true));

            var canvi = Assert.Single(canvis);
            Assert.Equal(MenaDeCanvi.Modificacio, canvi.Mena);
            Assert.Equal([new Referencia(Entitat.Centre, centre.Data.Id)], canvi.Afectats);
        }

        [Fact]
        public async Task ActivarIDesactivarTambeEmet()
        {
            using var provider = EntornDeTest.Nou();

            var centre = await provider.GetRequiredService<ICentreCreate>()
                .Create(new DTO.i.DTOs.CentreCreateParms(codi: "123", nom: "Pepe", esActiu: true));

            var canvis = Escolta(provider);

            await provider.GetRequiredService<ICentreActivaDesactiva>().Toggle(centre.Data!.Id);

            var canvi = Assert.Single(canvis);
            Assert.Equal(MenaDeCanvi.Modificacio, canvi.Mena);
            Assert.Equal([new Referencia(Entitat.Centre, centre.Data.Id)], canvi.Afectats);
        }

        [Fact]
        public async Task MoureUnaActuacioDUnAlumneAUnAltreEmetElsDos()
        {
            // El motiu pel qual BLUpdate calcula el DTO previ: amb les referències del DTO
            // nou només, el comptador de l'alumne d'origen es quedaria alt.
            using var provider = EntornDeTest.Nou();
            var (actuacioId, origen, desti, parms) = await UnaActuacioIDosAlumnes(provider);

            var canvis = Escolta(provider);

            await provider.GetRequiredService<IActuacioUpdate>().Update(parms(desti));

            var canvi = Assert.Single(canvis);
            Assert.Contains(new Referencia(Entitat.Alumne, origen), canvi.Afectats);
            Assert.Contains(new Referencia(Entitat.Alumne, desti), canvi.Afectats);
            Assert.Contains(new Referencia(Entitat.Actuacio, actuacioId), canvi.Afectats);
        }

        [Fact]
        public async Task UnaBaixaEmetLesReferenciesDeLActuacioEsborrada()
        {
            using var provider = EntornDeTest.Nou();
            var (actuacioId, origen, _, _) = await UnaActuacioIDosAlumnes(provider);

            var canvis = Escolta(provider);

            await provider.GetRequiredService<IActuacioDelete>()
                .Delete(new DTO.i.DTOs.IdParms(actuacioId));

            var canvi = Assert.Single(canvis);
            Assert.Equal(MenaDeCanvi.Baixa, canvi.Mena);
            Assert.Contains(new Referencia(Entitat.Actuacio, actuacioId), canvi.Afectats);
            Assert.Contains(new Referencia(Entitat.Alumne, origen), canvi.Afectats);
        }

        [Fact]
        public async Task UnaOperacioMassivaDemanaQueEsRefresquiTot()
        {
            using var provider = EntornDeTest.Nou();
            var canvis = Escolta(provider);

            await provider.GetRequiredService<IAlumneSyncActiuByCentre>().Run();

            var canvi = Assert.Single(canvis);
            Assert.True(canvi.AfectaTot);
        }

        // --- Bastida ---------------------------------------------------------------

        /// <summary>
        /// Munta el mínim per poder moure una actuació: dos alumnes i una actuació del
        /// primer. Torna també com es demana el trasllat al segon.
        /// </summary>
        private static async Task<(int Actuacio, int Origen, int Desti,
            Func<int, DTO.i.DTOs.ActuacioUpdateParms> Trasllat)> UnaActuacioIDosAlumnes(
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

            async Task<int> Alumne(string cognoms)
                => (await provider.GetRequiredService<IAlumneCreate>()
                    .Create(new DTO.i.DTOs.AlumneCreateParms(
                        "Nom", cognoms, null, centre.Id, curs.Id, etapa.Id, "1r",
                        null, "", null, "", ""))).Data!.Id;

            var origen = await Alumne("Origen");
            var desti = await Alumne("Desti");

            var actuacio = (await provider.GetRequiredService<IActuacioCreate>()
                .Create(new DTO.i.DTOs.ActuacioCreateParms(
                    origen, tipus.Id, "", DateTime.Today, curs.Id, centre.Id, etapa.Id,
                    "1r", 30, "descripció"))).Data!;

            return (actuacio.Id, origen, desti,
                alumneId => new DTO.i.DTOs.ActuacioUpdateParms(
                    actuacio.Id, alumneId, tipus.Id, "", DateTime.Today, curs.Id, centre.Id,
                    etapa.Id, "1r", 30, "descripció"));
        }

        private static List<CanviDeDomini> Escolta(IServiceProvider provider)
        {
            var canvis = new List<CanviDeDomini>();
            provider.GetRequiredService<INotificadorDeCanvis>().Canvi += canvis.Add;
            return canvis;
        }


        /// <summary>Les operacions concretes del BusinessLayer.</summary>
        private static IEnumerable<Type> Operacions()
            => typeof(CentreSet).Assembly.GetTypes()
                .Where(t => !t.IsAbstract
                         && !t.IsGenericTypeDefinition
                         && typeof(IBLOperation).IsAssignableFrom(t));

        private static bool HeretaDe(Type tipus, Type definicioGenerica)
        {
            for (var t = tipus; t is not null; t = t.BaseType)
                if (t.IsGenericType && t.GetGenericTypeDefinition() == definicioGenerica)
                    return true;

            return false;
        }

        private static MethodInfo MetodeDe(Type classeBase, string nom)
            => classeBase.GetMethod(nom,
                   BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                   | BindingFlags.DeclaredOnly)
               ?? throw new InvalidOperationException($"{classeBase.Name} no té cap {nom}().");

        /// <summary>
        /// Busca al cos del mètode una crida a <see cref="INotificadorDeCanvis.Publica"/>.
        /// Els mètodes són <c>async</c>: el cos real és el <c>MoveNext</c> de la màquina
        /// d'estats que el compilador genera. L'escaneig de l'IL és per força bruta, com
        /// el de <c>ConstructorsDeVistaTest</c>, i els operands que es llegeixin com si
        /// fossin opcodes no resolen a res.
        /// </summary>
        private static bool Publica(MethodInfo metode)
        {
            var maquinaDEstats = metode
                .GetCustomAttribute<AsyncStateMachineAttribute>()?.StateMachineType;

            var cos = maquinaDEstats is null
                ? metode
                : (MethodBase)maquinaDEstats.GetMethod("MoveNext",
                      BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;

            var il = cos.GetMethodBody()?.GetILAsByteArray();
            if (il is null) return false;

            const byte OpCodeCall = 0x28;
            const byte OpCodeCallVirt = 0x6F;
            var modul = cos.Module;
            var objectiu = typeof(INotificadorDeCanvis).GetMethod(nameof(INotificadorDeCanvis.Publica));

            for (var i = 0; i + 4 < il.Length; i++)
            {
                if (il[i] != OpCodeCall && il[i] != OpCodeCallVirt)
                    continue;

                try
                {
                    if (modul.ResolveMethod(BitConverter.ToInt32(il, i + 1)) == objectiu)
                        return true;
                }
                catch (ArgumentException)
                {
                    // No era un token: l'escaneig ha llegit un operand com si fos un opcode.
                }
            }

            return false;
        }
    }
}
