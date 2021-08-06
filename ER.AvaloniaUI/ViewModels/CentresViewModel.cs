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
            RxApp.MainThreadScheduler.Schedule(LoadCentres);    
        }
        public ObservableCollection<CentresRowViewMode> MyItems {get;} = new();

        private async void LoadCentres()
        {
            var parms = new DTO.i.DTOs.EsActiuParms(esActiu: null);
            var l =
                await
                BLCentres
                .GetItems(parms)
                ;

            l.Data!
            .Select(x => new CentresRowViewMode(x ))
            .ToList()
            .ForEach(x=>MyItems.Add(x));
        }
    }
}
