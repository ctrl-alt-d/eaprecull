using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
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
        // UIConfigureServices no toca ni la base de dades ni el sistema de fitxers,
        // a diferència de DataLayerConfigureServices (que executa les migracions).
        private static readonly IServiceCollection Serveis =
            new ServiceCollection().UIConfigureServices();

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
                .Where(Vistes.EsConstruiblePelContenidor)
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
        public void LaFactoryEsSingletonIEsPotResoldre()
        {
            var descriptor = Assert.Single(Serveis, d => d.ServiceType == typeof(IWindowFactory));
            Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);

            using var provider = Serveis.BuildServiceProvider(validateScopes: true);
            Assert.IsType<WindowFactory>(provider.GetRequiredService<IWindowFactory>());
        }
    }
}
