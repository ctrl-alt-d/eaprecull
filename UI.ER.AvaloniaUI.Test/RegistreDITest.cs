using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using BusinessLayer.Abstract.Generic;
using BusinessLayer.DI;
using UI.ER.AvaloniaUI.DI;
using UI.ER.AvaloniaUI.Services;
using UI.ER.ViewModels.ViewModels;
using Xunit;

namespace UI.ER.AvaloniaUI.Test
{
    /// <summary>
    /// R0 — el registre de la capa de presentació es fa per comprensió. Aquests tests
    /// fixen les propietats de què depèn el funcionament en runtime: què hi ha registrat
    /// i amb quin cicle de vida.
    /// </summary>
    public class RegistreDITest
    {
        // Cap dels *ConfigureServices toca la base de dades ni el sistema de fitxers:
        // les migracions són a IServiceProvider.MigraBaseDeDades(), fora del registre.
        private static readonly IServiceCollection Serveis =
            new ServiceCollection().UIConfigureServices();

        // La composition root registra primer el BusinessLayer i després la UI. Cal el
        // muntatge sencer per veure el bus, que el registra el BusinessLayer.
        private static readonly IServiceCollection ServeisComplets =
            new ServiceCollection().BusinessLayerConfigureServices().UIConfigureServices();

        [Fact]
        public void TotesLesFinestresEstanRegistrades()
        {
            var sense = Vistes.Finestres
                .Where(f => Serveis.All(d => d.ServiceType != f))
                .Select(f => f.Name)
                .ToList();

            Assert.True(sense.Count == 0,
                "Finestres que l'escaneig no ha registrat: " + string.Join(", ", sense));
        }

        [Fact]
        public void TotesLesFinestresSonTransient()
        {
            // Invariant 1: un Window d'Avalonia tancat no es pot reobrir. Amb Singleton,
            // el segon ShowDialog llança InvalidOperationException.
            var dolentes = Serveis
                .Where(d => Vistes.Finestres.Contains(d.ServiceType))
                .Where(d => d.Lifetime != ServiceLifetime.Transient)
                .Select(d => $"{d.ServiceType.Name} ({d.Lifetime})")
                .ToList();

            Assert.True(dolentes.Count == 0,
                "Finestres que no són Transient: " + string.Join(", ", dolentes));
        }

        [Fact]
        public void EsRegistrenExactamentElsViewModelsConstruiblesSenseArguments()
        {
            var esperats = Vistes.ViewModels
                .Where(vm => Vistes.EsConstruiblePelContenidor(vm, Serveis))
                .OrderBy(t => t.Name)
                .ToList();

            var registrats = Serveis
                .Select(d => d.ServiceType)
                .Where(t => typeof(ViewModelBase).IsAssignableFrom(t))
                .OrderBy(t => t.Name)
                .ToList();

            Assert.Equal(esperats, registrats);
        }

        [Fact]
        public void ElsViewModelsAmbArgumentsDeRuntimeNoEsRegistren()
        {
            // Aquests sempre passen per GetWith: el contenidor no els pot construir.
            Type[] fora =
            [
                typeof(ActuacioUpdateViewModel), typeof(AlumneUpdateViewModel),
                typeof(CentreUpdateViewModel), typeof(CursAcademicUpdateViewModel),
                typeof(EtapaUpdateViewModel), typeof(TipusActuacioUpdateViewModel),
                typeof(AlumneInformeViewerViewModel),
                typeof(ActuacioRowViewModel), typeof(AlumneRowViewModel),
                typeof(CentreRowViewModel), typeof(CursAcademicRowViewModel),
                typeof(EtapaRowViewModel), typeof(TipusActuacioRowViewModel),
            ];

            var registrats = fora
                .Where(t => Serveis.Any(d => d.ServiceType == t))
                .Select(t => t.Name)
                .ToList();

            Assert.True(registrats.Count == 0,
                "ViewModels amb arguments de runtime que s'han registrat sense voler: "
                + string.Join(", ", registrats));
        }

        [Fact]
        public void ElsViewModelsQueLaFactoryHaDeResoldreHiSon()
        {
            // Get<TWindow>() només funciona si el ViewModel de la finestra està registrat.
            Type[] necessaris =
            [
                typeof(AppStatusViewModel), typeof(UtilitatsViewModel),
                typeof(ActuacioSetViewModel), typeof(AlumneSetViewModel),
                typeof(CentreSetViewModel), typeof(CursAcademicSetViewModel),
                typeof(EtapaSetViewModel), typeof(TipusActuacioSetViewModel),
                typeof(ActuacioCreateViewModel), typeof(AlumneCreateViewModel),
                typeof(CentreCreateViewModel), typeof(CursAcademicCreateViewModel),
                typeof(EtapaCreateViewModel), typeof(TipusActuacioCreateViewModel),
            ];

            var falten = necessaris
                .Where(t => Serveis.All(d => d.ServiceType != t))
                .Select(t => t.Name)
                .ToList();

            Assert.True(falten.Count == 0, "ViewModels sense registrar: " + string.Join(", ", falten));
        }

        [Fact]
        public void ElsViewModelsRegistratsSonTransient()
        {
            // Un ViewModel compartit entre diàlegs arrossegaria estat i subscripcions.
            var dolents = Serveis
                .Where(d => typeof(ViewModelBase).IsAssignableFrom(d.ServiceType))
                .Where(d => d.Lifetime != ServiceLifetime.Transient)
                .Select(d => $"{d.ServiceType.Name} ({d.Lifetime})")
                .ToList();

            Assert.True(dolents.Count == 0,
                "ViewModels que no són Transient: " + string.Join(", ", dolents));
        }

        [Fact]
        public void LesDependenciesDeCadaFinestraEstanRegistrades()
        {
            var problemes =
                (from finestra in Vistes.Finestres
                 from ctor in finestra.GetConstructors()
                 from parametre in ctor.GetParameters()
                 where Serveis.All(d => d.ServiceType != parametre.ParameterType)
                 select $"{finestra.Name}({parametre.ParameterType.Name} {parametre.Name})")
                .ToList();

            Assert.True(problemes.Count == 0,
                "Constructors de finestra amb dependències sense registrar: "
                + string.Join(", ", problemes));
        }

        [Fact]
        public void ElBusDeCanvisEsSingletonIEsRegistraAbansDeLEscaneig()
        {
            // Singleton perquè n'hi ha d'haver un de sol, i abans de l'escaneig de
            // ViewModels pel mateix motiu que l'IServiceFactory: és una dependència que
            // el filtre de registre ha de poder veure.
            var descriptor = Assert.Single(ServeisComplets,
                d => d.ServiceType == typeof(INotificadorDeCanvis));

            Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);

            var posicioBus = ServeisComplets
                .Select((d, i) => (d, i))
                .First(x => x.d.ServiceType == typeof(INotificadorDeCanvis)).i;

            var primerViewModel = ServeisComplets
                .Select((d, i) => (d, i))
                .First(x => typeof(ViewModelBase).IsAssignableFrom(x.d.ServiceType)).i;

            Assert.True(posicioBus < primerViewModel,
                "El bus de canvis s'ha de registrar abans de l'escaneig dels ViewModels.");
        }

        [Fact]
        public void LaClasseBaseDeLesLlistesNoEntraAlRegistre()
        {
            // Invariant §9.6: els escanejos filtren IsAbstract. SetViewModelBase és
            // abstracta a posta, i el compte de ViewModels registrats no ha de canviar
            // perquè les sis llistes ara en derivin.
            var registrats = ServeisComplets
                .Select(d => d.ServiceType)
                .Where(t => typeof(ViewModelBase).IsAssignableFrom(t))
                .ToList();

            Assert.DoesNotContain(registrats, t => t.IsAbstract || t.IsGenericTypeDefinition);
            Assert.Equal(6, registrats.Count(t => t.Name.EndsWith("SetViewModel", StringComparison.Ordinal)));
        }

        [Fact]
        public void LaFabricaDeServeisArribaAlBusQueHaRegistratElBusinessLayer()
        {
            // Les dues meitats les registren dos mètodes diferents i cap test de cadascuna
            // per separat ho veuria: la fàbrica és de la UI, el bus és del BusinessLayer.
            using var provider = ServeisComplets.BuildServiceProvider(validateScopes: true);
            using var scope = provider.CreateScope();

            var fabrica = scope.ServiceProvider.GetRequiredService<IServiceFactory>();

            Assert.Same(provider.GetRequiredService<INotificadorDeCanvis>(), fabrica.Canvis);
        }

        [Fact]
        public void LaFactoryEsSingletonIEsPotResoldre()
        {
            var descriptor = Assert.Single(Serveis, d => d.ServiceType == typeof(IWindowFactory));
            Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);

            using var provider = Serveis.BuildServiceProvider(validateScopes: true);
            Assert.IsType<WindowFactory>(provider.GetRequiredService<IWindowFactory>());
        }
    }
}
