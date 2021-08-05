using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
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
        private readonly ICentreGetSet BLCentres;
        private readonly ICentreActivaDesactiva BLActivaDesactiva;
        private dtoo.Centre? SelectedItem {
            get; 
            set;
        }
        public CentresViewModel(ICentreGetSet blcentres, ICentreActivaDesactiva blActivaDesactiva)
        {
            BLCentres = blcentres;
            BLActivaDesactiva = blActivaDesactiva;
            DoTheThing = ReactiveCommand.Create( RunTheThing ); // <-- HERE!!!
            RxApp.MainThreadScheduler.Schedule(LoadCentres);    
        }
        public ObservableCollection<dtoo.Centre> MyItems {get;} = new();

        public ReactiveCommand<Unit, Unit> DoTheThing { get; }   // <-- HERE!!!

        void RunTheThing()  // <-- HERE!!!
        {
            if (SelectedItem == null) return;

            //
            var model = (BLActivaDesactiva.Toggle(SelectedItem.Id).GetAwaiter().GetResult()).Data!;
            if (model == null) return;

            //
            var item = MyItems.First(x=>x.Id==model.Id);
            var i = MyItems.IndexOf(item);
            MyItems[i] = model;
        }

        private async void LoadCentres()
        {
            var createParms = new DTO.i.DTOs.EsActiuParms(esActiu: true);
            var l =
                await
                BLCentres
                .GetItems(createParms)
                ;

            l.Data!.ForEach(x=>MyItems.Add(x));
        }
    }
}
