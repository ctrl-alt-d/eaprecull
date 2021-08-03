using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Text;
using BusinessLayer.Abstract.Services;
using DTO.o.DTOs;
using ER.AvaloniaUI.Services;
using ReactiveUI;
using dtoo = DTO.o.DTOs;

namespace ER.AvaloniaUI.ViewModels
{
    public class CentresViewModel : ViewModelBase
    {
        public CentresViewModel()
        {
            RxApp.MainThreadScheduler.Schedule(LoadCentres);    
        }

        public ObservableCollection<dtoo.Centre> MyItems {get;} = new();

        private async void LoadCentres()
        {
            var createParms = new DTO.i.DTOs.EsActiuParms(esActiu: true);
            var l =
                await
                Services.SuperContext
                .GetBLOperation<ICentres>()
                .GetItems(createParms)
                ;

            l.Data!.ForEach(x=>MyItems.Add(x));
        }
    }
}
