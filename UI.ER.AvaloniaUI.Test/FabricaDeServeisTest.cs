using System;
using System.Linq;
using System.Reflection;
using BusinessLayer.Abstract.Generic;
using Microsoft.Extensions.DependencyInjection;
using UI.ER.AvaloniaUI.DI;
using UI.ER.ViewModels.Services;
using Xunit;

namespace UI.ER.AvaloniaUI.Test
{
    /// <summary>
    /// R1 — els ViewModels arriben al BusinessLayer per una <see cref="IServiceFactory"/>
    /// injectada, no per un Service Locator estàtic. Aquests tests fixen les tres
    /// propietats de què depèn que això funcioni: el cicle de vida de la fàbrica, que no
    /// es pugui resoldre fora d'un scope, i que no hagi tornat cap variant estàtica.
    /// </summary>
    public class FabricaDeServeisTest
    {
        private static readonly IServiceCollection Serveis =
            new ServiceCollection().UIConfigureServices();

        [Fact]
        public void LaFabricaDeServeisEsScoped()
        {
            // Scoped, no Singleton: és el que fa que les operacions transitòries
            // IDisposable del BusinessLayer quedin apuntades a l'scope del diàleg i
            // s'alliberin en tancar-lo. Amb Singleton tornaríem al provider arrel, que
            // és el que feia SuperContext i el que mantenia les 51 operacions vives
            // fins a tancar l'aplicació.
            var descriptor = Assert.Single(Serveis, d => d.ServiceType == typeof(IServiceFactory));

            Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
            Assert.Equal(typeof(ServiceFactory), descriptor.ImplementationType);
        }

        [Fact]
        public void LaFabricaDeServeisNomesSurtDUnScope()
        {
            using var provider = Serveis.BuildServiceProvider(validateScopes: true);

            Assert.Throws<InvalidOperationException>(
                () => provider.GetRequiredService<IServiceFactory>());

            using var scope = provider.CreateScope();
            Assert.IsType<ServiceFactory>(scope.ServiceProvider.GetRequiredService<IServiceFactory>());
        }

        [Fact]
        public void LaFabricaDeServeisEsRegistraAbansDelsViewModels()
        {
            // El filtre de registre dels ViewModels pregunta a la col·lecció si cada
            // paràmetre del constructor és resoluble. Si la fàbrica es registrés després,
            // el filtre no en sabria res i no es registraria cap ViewModel.
            var posicioFabrica = Serveis
                .Select((d, i) => (d, i))
                .First(x => x.d.ServiceType == typeof(IServiceFactory)).i;

            var primerViewModel = Serveis
                .Select((d, i) => (d, i))
                .First(x => typeof(UI.ER.ViewModels.ViewModels.ViewModelBase)
                            .IsAssignableFrom(x.d.ServiceType)).i;

            Assert.True(posicioFabrica < primerViewModel,
                "L'IServiceFactory s'ha de registrar abans de l'escaneig dels ViewModels.");
        }

        [Fact]
        public void CapViewModelGuardaElsServeisEnUnCampEstatic()
        {
            // Guarda contra el retorn de SuperContext sota qualsevol altre nom: un
            // proveïdor o una fàbrica en un camp estàtic torna a ser un Service Locator,
            // i torna a fer els ViewModels no construïbles ni testejables.
            var locators =
                (from tipus in Vistes.AssemblyViewModels.GetTypes()
                 from camp in tipus.GetFields(BindingFlags.Static | BindingFlags.Public
                                              | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                 where camp.FieldType == typeof(IServiceProvider)
                    || camp.FieldType == typeof(IServiceFactory)
                 select $"{tipus.Name}.{camp.Name}")
                .ToList();

            Assert.True(locators.Count == 0,
                "Camps estàtics que tornen a fer de Service Locator: "
                + string.Join(", ", locators));
        }

        [Fact]
        public void ElsViewModelsAmbArgumentsDeRuntimeReepLaFabricaPelConstructor()
        {
            // Aquests el contenidor no els pot construir (porten un id, un DTO…): els
            // construeix el ViewModel pare, que els ha de poder passar la seva fàbrica.
            // Sense el paràmetre, l'única manera que tindrien d'arribar al backend
            // tornaria a ser estàtica.
            var candidats = Vistes.ViewModels
                .Where(vm => !Vistes.EsConstruiblePelContenidor(vm, Serveis))
                .ToList();

            Assert.NotEmpty(candidats);

            var sense = candidats
                .Where(vm => vm.GetConstructors()
                    .All(c => c.GetParameters() is not [{ ParameterType: var t }, ..]
                              || t != typeof(IServiceFactory)))
                .Select(vm => vm.Name)
                .ToList();

            Assert.True(sense.Count == 0,
                "ViewModels amb arguments de runtime que no reben la IServiceFactory com a "
                + "primer paràmetre del constructor: " + string.Join(", ", sense));
        }
    }
}
