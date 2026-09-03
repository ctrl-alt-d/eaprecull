using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI.Avalonia;
using ReactiveUI;
using UI.ER.ViewModels.ViewModels;
using Dtoo = DTO.o.DTOs;
using System.Reactive.Linq;
using System.Linq;
using UI.ER.AvaloniaUI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace UI.ER.AvaloniaUI.Pages
{
    public partial class CentreSetWindow : ReactiveWindow<CentreSetViewModel>
    {
        private readonly IWindowFactory _windows;

        // Constructor pont: el manté el carregador XAML en temps d'execució i el
        // previsualitzador d'Avalonia, que instancien la vista sense passar pel
        // contenidor. Encadena amb el de DI, així les dues vies deixen _windows a punt.
        public CentreSetWindow() : this(App.Services.GetRequiredService<IWindowFactory>()) { }

        public CentreSetWindow(IWindowFactory windows)
        {
            _windows = windows;

            InitializeComponent();

            this.WhenActivated(disposables =>
            {
                RegisterShowCreateDialog(disposables);
            });
        }

        private void RegisterShowCreateDialog(Action<IDisposable> disposables)
            =>
            disposables(
                this
                .WhenAnyValue(x => x.ViewModel)
                .Subscribe(vm => vm!.ShowDialog.RegisterHandler(async interaction =>
                {
                    var dialog = _windows.GetWith<CentreCreateWindow>(interaction.Input);

                    var result = await dialog.ShowDialog<Dtoo.Centre?>(GetWindow());
                    interaction.SetOutput(result);
                }))
            );

        private void InitializeComponent()
            =>
            AvaloniaXamlLoader.Load(this);

        private Window GetWindow()
            =>
            (Window)this.VisualRoot!;
    }
}