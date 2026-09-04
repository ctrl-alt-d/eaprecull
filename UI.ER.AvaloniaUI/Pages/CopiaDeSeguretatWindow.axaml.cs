using System;
using System.Linq;
using System.Reactive.Linq;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using ReactiveUI;
using ReactiveUI.Avalonia;
using UI.ER.ViewModels.ViewModels;

namespace UI.ER.AvaloniaUI.Pages
{
    /// <summary>
    /// La finestra de còpies de seguretat. L'únic que fa el codi rere la vista és atendre
    /// el selector de carpetes: és d'Avalonia, i el ViewModel no pot conèixer Avalonia.
    /// </summary>
    public partial class CopiaDeSeguretatWindow : ReactiveWindow<CopiaDeSeguretatViewModel>
    {
        public CopiaDeSeguretatWindow()
        {
            InitializeComponent();

            this.WhenActivated(disposables =>
                disposables(
                    this.WhenAnyValue(x => x.ViewModel)
                        .Where(vm => vm is not null)
                        .Subscribe(vm =>
                        {
                            disposables(vm!.ShowTriaCarpetaDialog.RegisterHandler(TriaLaCarpeta));

                            // «Ara no» tanca la finestra. Tancar-la és cosa de la vista,
                            // com a ConfirmacioWindow: el ViewModel només diu que s'ha
                            // premut, no sap que existeix cap Window.
                            disposables(vm.AraNoCommand.Subscribe(_ => Close()));
                        })));
        }

        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

        /// <summary>
        /// El selector de carpetes del sistema. Torna null si l'usuari el tanca sense
        /// triar res, que és el cas que el ViewModel ha de saber ignorar.
        /// </summary>
        private async System.Threading.Tasks.Task TriaLaCarpeta(
            IInteractionContext<System.Reactive.Unit, string?> interaccio)
        {
            var carpetes = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "On vols desar les còpies de seguretat?",
                AllowMultiple = false,
            });

            interaccio.SetOutput(carpetes.FirstOrDefault()?.TryGetLocalPath());
        }
    }
}
