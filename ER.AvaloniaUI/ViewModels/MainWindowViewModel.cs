using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Input;
using ER.AvaloniaUI.Services;
using ReactiveUI;

namespace ER.AvaloniaUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly CentresViewModel CentresViewModel;
        public MainWindowViewModel(CentresViewModel centresViewModel)
        {
            CentresViewModel = centresViewModel;
            ShowDialog = new Interaction<CentresViewModel, bool>();

            GestionaCentresCommand = ReactiveCommand.CreateFromTask(async () => 
            {
                await ShowDialog.Handle(CentresViewModel);
            });
        }

        public ICommand GestionaCentresCommand { get; }

        public Interaction<CentresViewModel, bool> ShowDialog { get; }

    }
}
