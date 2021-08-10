using System.Linq;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using BusinessLayer.Abstract.Services;
using ReactiveUI;
using dtoo = DTO.o.DTOs;
using UI.ER.AvaloniaUI.Services;
using System.Reactive.Linq;
using System;

namespace UI.ER.ViewModels.ViewModels
{
    public class CentreSetViewModel : ViewModelBase
    {
        protected virtual ICentreGetSet BLCentres() => SuperContext.GetBLOperation<ICentreGetSet>();
        public CentreSetViewModel()
        {
            //RxApp.MainThreadScheduler
            //    .Schedule(LoadCentresNoParm);    
            this
                .WhenAnyValue(x => x.NomesActius)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(LoadCentres);
        }
        public ObservableCollection<CentreRowViewModel> MyItems {get;} = new();

        protected virtual void LoadCentresNoParm()
        {
            LoadCentres(true);
        }
        protected virtual async void LoadCentres(bool nomesActius)
        {

            MyItems.Clear();
            var esActiu = nomesActius ? true : (bool?) null;
            var parms = new DTO.i.DTOs.EsActiuParms(esActiu: esActiu);

            using var bl = BLCentres();
            var l =
                await
                bl
                .FromPredicate(parms)
                ;

            l
            .Data! // ToDo: gestionar broken rules
            .Select(x => new CentreRowViewModel(x ))
            .ToList()
            .ForEach(x=>MyItems.Add(x));
        }

        private bool _NomesActius = true;
        public bool NomesActius
        {
            get => _NomesActius;
            set => this.RaiseAndSetIfChanged(ref _NomesActius, value);
        }
        
    }
}
