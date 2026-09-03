using System;
using BusinessLayer.Abstract.Generic;
using Microsoft.Extensions.DependencyInjection;
using UI.ER.AvaloniaUI.DI;
using UI.ER.AvaloniaUI.Pages;
using UI.ER.AvaloniaUI.Services;
using UI.ER.ViewModels.ViewModels;
using Xunit;

namespace UI.ER.AvaloniaUI.Test
{
    /// <summary>
    /// R0 — comportament de la factory que no depèn d'Avalonia.
    /// </summary>
    /// <remarks>
    /// El camí feliç (<c>Get</c>/<c>GetWith</c> retornant una finestra) no es pot cobrir
    /// aquí: construir un <c>Window</c> demana una plataforma d'Avalonia inicialitzada, i
    /// construir qualsevol ViewModel real dispara la càrrega de dades contra la base de
    /// dades des del constructor (invariant 3). Es valida amb la prova manual.
    /// </remarks>
    public class WindowFactoryTest
    {
        private static IWindowFactory Factory()
            => new ServiceCollection()
                .UIConfigureServices()
                .BuildServiceProvider()
                .GetRequiredService<IWindowFactory>();

        [Fact]
        public void GetWithRebutjaUnViewModelQueNoEsElDeLaFinestra()
        {
            var ex = Assert.Throws<ArgumentException>(
                () => Factory().GetWith<CentreSetWindow>(new ViewModelQualsevol()));

            Assert.Contains("CentreSetWindow", ex.Message);
            Assert.Contains("CentreSetViewModel", ex.Message);
            Assert.Contains(nameof(ViewModelQualsevol), ex.Message);
        }

        [Fact]
        public void GetWithValidaAbansDeConstruirRes()
        {
            // La comprovació ha de passar abans de resoldre la finestra: si es fes després,
            // el missatge d'error quedaria amagat rere una excepció d'Avalonia en un entorn
            // sense plataforma gràfica, i s'hauria obert un scope per no res.
            Assert.Throws<ArgumentException>(
                () => Factory().GetWith<AlumneInformeViewerWindow>(new ViewModelQualsevol()));
        }

        // -- CreaViewModel: l'única part de la factory que no demana una plataforma
        //    d'Avalonia, i la que R1 hi ha afegit. --

        [Fact]
        public void CreaViewModelSenseArgumentsElTreuDelContenidor()
        {
            var provider = new ServiceCollection()
                .AddScoped<IServiceFactory, FabricaDeMentida>()
                .AddTransient<ViewModelAmbArgumentsDeRuntime>()
                .BuildServiceProvider();

            using var scope = provider.CreateScope();

            var vm = Assert.IsType<ViewModelAmbArgumentsDeRuntime>(
                WindowFactory.CreaViewModel(
                    scope.ServiceProvider, typeof(ViewModelAmbArgumentsDeRuntime)));

            Assert.False(vm.ModeLookup);
            Assert.Null(vm.AlumneId);
        }

        [Fact]
        public void CreaViewModelBarrejaElsArgumentsDeRuntimeAmbLesDependencies()
        {
            // El cas dels 16 lookups: la vista només sap dir «modeLookup», i la fàbrica
            // de serveis —que no és cosa de la vista— l'ha de posar l'scope. La resta de
            // paràmetres es queden al valor per defecte.
            var provider = new ServiceCollection()
                .AddScoped<IServiceFactory, FabricaDeMentida>()
                .BuildServiceProvider();

            using var scope = provider.CreateScope();

            var vm = Assert.IsType<ViewModelAmbArgumentsDeRuntime>(
                WindowFactory.CreaViewModel(
                    scope.ServiceProvider, typeof(ViewModelAmbArgumentsDeRuntime), true));

            Assert.True(vm.ModeLookup);
            Assert.Null(vm.AlumneId);
            Assert.NotNull(vm.Serveis);
        }

        [Fact]
        public void CreaViewModelOmpleElsParametresNullables()
        {
            // El pla de R1 avisava que un int podia no arribar mai a un paràmetre int? —
            // IsAssignableFrom diu que no— i que caldria treure els nullables dels
            // constructors. Amb l'ActivatorUtilities d'avui no cal: aparella el nullable
            // tant si l'argument ve tipat com si ve pelat. El test hi és per saber-ho si
            // algun dia deixa de ser cert.
            var provider = new ServiceCollection()
                .AddScoped<IServiceFactory, FabricaDeMentida>()
                .BuildServiceProvider();

            using var scope = provider.CreateScope();

            var vm = Assert.IsType<ViewModelAmbArgumentsDeRuntime>(
                WindowFactory.CreaViewModel(
                    scope.ServiceProvider, typeof(ViewModelAmbArgumentsDeRuntime), true, (int?)7));

            Assert.Equal(7, vm.AlumneId);

            var ambIntPelat = Assert.IsType<ViewModelAmbArgumentsDeRuntime>(
                WindowFactory.CreaViewModel(
                    scope.ServiceProvider, typeof(ViewModelAmbArgumentsDeRuntime), true, 7));

            Assert.Equal(7, ambIntPelat.AlumneId);
        }

        private sealed class ViewModelQualsevol : ViewModelBase;

        /// <summary>
        /// Mateixa forma de constructor que <c>ActuacioSetViewModel</c>: la dependència
        /// que resol el contenidor, un argument de runtime i un nullable amb valor per
        /// defecte. És el cas que exercita els 16 lookups.
        /// </summary>
        private sealed class ViewModelAmbArgumentsDeRuntime(
            IServiceFactory serveis, bool modeLookup = false, int? alumneId = null) : ViewModelBase
        {
            public IServiceFactory Serveis { get; } = serveis;

            public bool ModeLookup { get; } = modeLookup;

            public int? AlumneId { get; } = alumneId;
        }

        private sealed class FabricaDeMentida : IServiceFactory
        {
            public T GetBLOperation<T>() where T : IBLOperation
                => throw new NotSupportedException("Cap test d'aquest fitxer toca el backend.");

            public INotificadorDeCanvis Canvis
                => throw new NotSupportedException("Cap test d'aquest fitxer escolta el bus.");
        }
    }
}
