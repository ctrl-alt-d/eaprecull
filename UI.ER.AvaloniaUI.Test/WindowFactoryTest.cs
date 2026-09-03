using System;
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

        private sealed class ViewModelQualsevol : ViewModelBase;
    }
}
