using System;
using System.Linq;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using UI.ER.AvaloniaUI.DI;
using UI.ER.AvaloniaUI.Services;
using UI.ER.ViewModels.ViewModels;
using Xunit;

namespace UI.ER.AvaloniaUI.Test
{
    /// <summary>
    /// R0 — la factory tria el ViewModel d'una vista per convenció de noms, o per
    /// <see cref="ViewModelAttribute"/> si n'hi ha. Trencar la convenció fa que
    /// l'aplicació no arrenqui, i aquests tests ho detecten abans.
    /// </summary>
    public class ConvencioVistaViewModelTest
    {
        [Fact]
        public void CadaFinestraResolElSeuViewModel()
        {
            var problemes = Vistes.Finestres
                .Select(vista =>
                {
                    try
                    {
                        WindowFactory.ViewModelTypeFor(vista);
                        return null;
                    }
                    catch (InvalidOperationException ex)
                    {
                        return $"{vista.Name}: {ex.Message}";
                    }
                })
                .OfType<string>()
                .ToList();

            Assert.True(problemes.Count == 0,
                "Vistes sense ViewModel resoluble. Cal renomenar-les segons la convenció "
                + "o afegir-hi [ViewModel(typeof(...))]:" + Environment.NewLine
                + string.Join(Environment.NewLine, problemes));
        }

        [Fact]
        public void HiHaLesFinestresQueEsperem()
        {
            // Si aquest nombre canvia sense voler, vol dir que algú ha afegit o esborrat
            // una finestra: la resta de tests d'aquest fitxer ja l'hauran cobert, però
            // convé que el canvi sigui explícit.
            Assert.Equal(23, Vistes.Finestres.Count);
        }

        [Fact]
        public void ElRegistreDeLaUIValidaLaConvencioAlArrencar()
        {
            // UIConfigureServices fa la mateixa comprovació al composition root.
            // No ha de llançar.
            var excepcio = Record.Exception(() => new ServiceCollection().UIConfigureServices());

            Assert.Null(excepcio);
        }

        [Fact]
        public void LAtributTePrioritatSobreLaConvencio()
        {
            // MainWindow és l'única excepció real: el seu ViewModel no es diu MainViewModel.
            Assert.Equal(
                typeof(AppStatusViewModel),
                WindowFactory.ViewModelTypeFor(typeof(Views.MainWindow)));

            Assert.Equal(
                typeof(UtilitatsViewModel),
                WindowFactory.ViewModelTypeFor(typeof(FinestraAmbAtributWindow)));
        }

        [Fact]
        public void LaConvencioTreuElSufixDeLaVista()
        {
            Assert.Equal(
                typeof(CentreSetViewModel),
                WindowFactory.ViewModelTypeFor(typeof(Pages.CentreSetWindow)));

            Assert.Equal(
                typeof(CentreRowViewModel),
                WindowFactory.ViewModelTypeFor(typeof(Pages.CentreRowUserCtrl)));
        }

        [Fact]
        public void UnaVistaForaDeConvencioDiuQueSEsperavaIComArreglarHo()
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => WindowFactory.ViewModelTypeFor(typeof(FinestraForaDeConvencioWindow)));

            Assert.Contains("FinestraForaDeConvencioWindow", ex.Message);
            Assert.Contains("FinestraForaDeConvencioViewModel", ex.Message);
            Assert.Contains("[ViewModel(typeof(...))]", ex.Message);
        }

        // -- Vistes de mentida, només per als tests. Viuen a l'assembly de tests, així que
        //    no entren a l'escaneig de UIConfigureServices. --

        private class FinestraForaDeConvencioWindow : Window { }

        [ViewModel(typeof(UtilitatsViewModel))]
        private class FinestraAmbAtributWindow : Window { }
    }
}
