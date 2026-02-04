using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI.Avalonia;
using ReactiveUI;
using UI.ER.ViewModels.ViewModels;
using dtoo = DTO.o.DTOs;
using System.Reactive.Linq;
using System.Linq;

namespace UI.ER.AvaloniaUI.Pages
{
    public partial class CursAcademicSetWindow : ReactiveWindow<CursAcademicSetViewModel>
    {
        public CursAcademicSetWindow()
        {
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
                    var dialog = new CursAcademicCreateWindow()
                    {
                        DataContext = interaction.Input
                    };

                    var result = await dialog.ShowDialog<dtoo.CursAcademic?>(GetWindow());
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