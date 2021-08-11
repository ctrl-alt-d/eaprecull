﻿using System.Linq;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using BusinessLayer.Abstract.Services;
using ReactiveUI;
using dtoo = DTO.o.DTOs;
using UI.ER.AvaloniaUI.Services;
using System.Reactive.Linq;
using System;
using System.Threading.Tasks;

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
                .Subscribe(async nomesActius=>await LoadCentres(nomesActius))
                ;
        }
        public ObservableCollection<CentreRowViewModel> MyItems {get;} = new();

        protected virtual async Task LoadCentres(bool nomesActius)
        {

            // Buidar la llista actual
            MyItems.Clear();
            await OmplirAmbElsNousValors(nomesActius);
        }

        private async Task OmplirAmbElsNousValors(bool nomesActius)
        {
            // Petició al backend
            var esActiu = nomesActius ? true : (bool?)null;
            var parms = new DTO.i.DTOs.EsActiuParms(esActiu: esActiu);
            var bl = BLCentres();
            var l =
                await
                bl
                .FromPredicate(parms)
                ;

            // Omplir la llista amb els nous valors
            l
            .Data! // ToDo: gestionar broken rules            
            .Select(x => new CentreRowViewModel(x))
            .ToList()
            .ForEach(x => MyItems.Add(x));
        }

        private bool _NomesActius = true;
        public bool NomesActius
        {
            get => _NomesActius;
            set => this.RaiseAndSetIfChanged(ref _NomesActius, value);
        }
        
    }
}
