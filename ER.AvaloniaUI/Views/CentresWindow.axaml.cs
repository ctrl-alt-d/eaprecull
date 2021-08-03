using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using BusinessLayer.Abstract.Services;
using ER.AvaloniaUI.Services;
using ReactiveUI;
using dtoo = DTO.o.DTOs;
namespace ER.AvaloniaUI.Views
{
    public partial class CentresWindow : Window
    {
        public CentresWindow()
        {

            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            
            RxApp.MainThreadScheduler.Schedule(LoadCentres);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public ObservableCollection<dtoo.Centre> MyItems {get;} = new();

        private async void LoadCentres()
        {
            var createParms = new DTO.i.DTOs.EsActiuParms(esActiu: true);
            var l =
                await
                AppContext
                .GetBLOperation<ICentres>()
                .GetItems(createParms)
                ;

            l.Data!.ForEach(x=>MyItems.Add(x));
        }
    }
}