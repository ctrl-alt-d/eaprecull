using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Input;
using ReactiveUI;

namespace ER.AvaloniaUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            ShowDialog = new Interaction<CentresViewModel, CentreViewModel?>();

            BuyMusicCommand = ReactiveCommand.CreateFromTask(async () => 
            {
                var centres = new CentresViewModel();

                var result = await ShowDialog.Handle(centres);
            });
        }

        public ICommand BuyMusicCommand { get; }

        public Interaction<CentresViewModel, CentreViewModel?> ShowDialog { get; }

    }
}
