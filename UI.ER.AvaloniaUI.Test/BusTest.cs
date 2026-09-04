using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLayer.Abstract;
using BusinessLayer.Abstract.Generic;
using ReactiveUI;
using UI.ER.ViewModels.ViewModels;
using UI.ER.ViewModels.ViewModels.Base;
using Xunit;
using Dtoo = DTO.o.DTOs;

namespace UI.ER.AvaloniaUI.Test
{
    /// <summary>
    /// La subscripció d'una llista al bus de canvis viu i mor amb l'activació del seu
    /// ViewModel. Sense això, obrir i tancar la mateixa finestra aniria acumulant
    /// subscriptors al notificador —que és Singleton— i cada canvi dispararia N consultes.
    /// </summary>
    public class BusTest
    {
        [Fact]
        public void LaSubscripcioDUnaLlistaEsDonaDeBaixaEnDesactivarSe()
        {
            var notificador = new NotificadorEspia();
            var llista = new LlistaDeProva(new FabricaAmbBus(notificador));

            Assert.Equal(0, notificador.Subscriptors);

            for (var i = 0; i < 3; i++)
            {
                llista.Activator.Activate();
                Assert.Equal(1, notificador.Subscriptors);

                llista.Activator.Deactivate();
                Assert.Equal(0, notificador.Subscriptors);
            }
        }

        [Fact]
        public void NomesEsRefrescaLaLlistaQueTeLesEntitatsDelCanviPintades()
        {
            // La regla del bus. Sense el filtre, cada escriptura de l'aplicació faria
            // consultar totes les llistes obertes; sense el cas «massiu», una operació
            // que no diu què ha tocat no en refrescaria cap.
            var llista = new LlistaDeProva(new FabricaAmbBus(new NotificadorEspia()));
            llista.MyItems.Add(new FilaDeProva(CentreDeProva(total: 0, cursActiu: 0)));

            Assert.False(llista.Afecta(Canvi(new Referencia(Entitat.Alumne, 1))));
            Assert.True(llista.Afecta(Canvi(new Referencia(Entitat.Centre, 3))));
            Assert.True(llista.Afecta(CanviDeDomini.Tot));
        }

        private static CanviDeDomini Canvi(params Referencia[] afectats)
            => new(MenaDeCanvi.Alta, new HashSet<Referencia>(afectats));

        [Fact]
        public void UnaFilaPintaLesReferenciesDelSeuDtoIElsRecalculaEnActualitzarSe()
        {
            // El costat receptor de la convenció: una fila d'alumne no només es refresca
            // quan canvia l'alumne, sinó també quan canvia el centre o el curs que en
            // mostra. I si l'alumne canvia de centre, canvia de referències.
            var fabrica = new FabricaAmbBus(new NotificadorEspia());
            var fila = new AlumneRowViewModel(fabrica, AlumneDeProva(centreId: 3), null);

            Assert.Contains(new Referencia(Entitat.Alumne, 7), fila.ReferenciesPintades);
            Assert.Contains(new Referencia(Entitat.Centre, 3), fila.ReferenciesPintades);

            fila.Actualitza(AlumneDeProva(centreId: 9));

            Assert.Contains(new Referencia(Entitat.Centre, 9), fila.ReferenciesPintades);
            Assert.DoesNotContain(new Referencia(Entitat.Centre, 3), fila.ReferenciesPintades);
        }

        [Fact]
        public void UnaFilaDeCentreRefrescaElsComptadorsIElTextQueEnPenja()
        {
            // ActuacionsTxt és una propietat calculada: sense avisar-ne el canvi, el
            // comptador es refrescaria al ViewModel i no a la pantalla.
            var fabrica = new FabricaAmbBus(new NotificadorEspia());
            var fila = new CentreRowViewModel(fabrica, CentreDeProva(total: 10, cursActiu: 4));

            var avisos = 0;
            fila.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(fila.ActuacionsTxt)) avisos++; };

            fila.Actualitza(CentreDeProva(total: 11, cursActiu: 5));

            Assert.Equal(11, fila.TotalActuacions);
            Assert.Equal(5, fila.ActuacionsCursActiu);
            Assert.True(avisos > 0, "ActuacionsTxt no ha avisat que havia canviat.");
        }

        private static Dtoo.Alumne AlumneDeProva(int centreId)
            => new(7, "Nom", "Cognoms", null,
                new Dtoo.Centre(centreId, "C", "Centre", true, "Centre", ""),
                new Dtoo.CursAcademic(2, 2024, "2024-25", true, "2024-25", "", 0),
                null, "", null, "", null, "", true, "Cognoms, Nom", "", "", 0);

        private static Dtoo.CentreAmbActuacions CentreDeProva(int total, int cursActiu)
            => new(3, "C", "Centre", true, "Centre", "", total, cursActiu);

        // --- Bastida ---------------------------------------------------------------

        /// <summary>
        /// Una llista mínima. No fa cap càrrega al constructor —això ho fa cada
        /// <c>*SetViewModel</c> real amb els seus filtres— de manera que el test no
        /// necessita ni backend ni fil d'UI.
        /// </summary>
        private sealed class LlistaDeProva(IServiceFactory serveis)
            : SetViewModelBase<FilaDeProva, Dtoo.Centre>(serveis, modeLookup: false)
        {
            public int Consultes { get; private set; }

            /// <summary>La regla del bus, exposada perquè el test la miri sense el throttle.</summary>
            public bool Afecta(CanviDeDomini canvi) => EnsAfecta(canvi);

            protected override Task<OperationResults<Dtoo.Centre>> Consulta()
            {
                Consultes++;
                return Task.FromResult(new OperationResults<Dtoo.Centre>([], 0, 0));
            }

            protected override FilaDeProva CreaFila(Dtoo.Centre dto) => new(dto);
        }

        private sealed class FilaDeProva(Dtoo.Centre dto) : IFilaDeLlista<Dtoo.Centre>
        {
            public int Id { get; } = dto.Id;

            public IReadOnlySet<Referencia> ReferenciesPintades { get; private set; }
                = Referencies.De(dto);

            public void Actualitza(Dtoo.Centre nou) => ReferenciesPintades = Referencies.De(nou);
        }

        private sealed class FabricaAmbBus(INotificadorDeCanvis canvis) : IServiceFactory
        {
            public T GetBLOperation<T>() where T : IBLOperation
                => throw new NotSupportedException("Aquest test no toca el backend.");

            public INotificadorDeCanvis Canvis { get; } = canvis;

            public IDadesDeLusuari DadesUsuari
                => throw new NotSupportedException("Aquest test no llegeix les dades de l'usuari.");
        }

        /// <summary>Un notificador que sap quanta gent l'escolta.</summary>
        private sealed class NotificadorEspia : INotificadorDeCanvis
        {
            private Action<CanviDeDomini>? _canvi;

            public int Subscriptors { get; private set; }

            public event Action<CanviDeDomini>? Canvi
            {
                add { _canvi += value; Subscriptors++; }
                remove { _canvi -= value; Subscriptors--; }
            }

            public void Publica(CanviDeDomini canvi) => _canvi?.Invoke(canvi);
        }
    }
}
