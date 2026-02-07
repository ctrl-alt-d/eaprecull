using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using UI.ER.ViewModels.ViewModels;

namespace UI.ER.AvaloniaUI.Pages
{
    public partial class AlumneInformeViewerWindow : Window
    {
        public AlumneInformeViewerWindow()
        {
            InitializeComponent();

            // Quan s'obre la finestra, carregar les dades
            this.Opened += async (s, e) =>
            {
                if (DataContext is AlumneInformeViewerViewModel vm)
                {
                    await vm.LoadDataCommand.Execute();
                }
            };

            // Subscripció al CloseCommand
            this.WhenAnyValue(x => x.DataContext)
                .Where(dc => dc != null)
                .Subscribe(dc =>
                {
                    if (dc is AlumneInformeViewerViewModel vm)
                    {
                        vm.CloseCommand.Subscribe(_ => Close());
                    }
                });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
