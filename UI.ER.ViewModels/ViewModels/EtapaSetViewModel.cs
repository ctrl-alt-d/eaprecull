﻿using System.Linq;
using BusinessLayer.Abstract.Services;
using ReactiveUI;
using dtoo = DTO.o.DTOs;
using UI.ER.AvaloniaUI.Services;
using System.Reactive.Linq;
using System;
using System.Threading.Tasks;
using UI.ER.ViewModels.Common;
using System.Windows.Input;
using DynamicData.Binding;

namespace UI.ER.ViewModels.ViewModels
{

    public class EtapaSetViewModel : ViewModelBase
    {
        public bool ModeLookup { get; }
        public EtapaSetViewModel(bool modeLookup = false)
        {

            ModeLookup = modeLookup;

            // Filtre
            this
                .WhenAnyValue(x => x.NomesActius)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(nomesActius => LoadEtapas(nomesActius))
                ;

            // Create
            ShowDialog = new Interaction<EtapaCreateViewModel, dtoo.Etapa?>();

            Create = ReactiveCommand.CreateFromTask(async () =>
            {
                var update = new EtapaCreateViewModel();

                var data = await ShowDialog.Handle(update);

                if (data != null)
                {
                    var item = new EtapaRowViewModel(data, ModeLookup);
                    MyItems.Insert(0, item);
                }
            });


        }
        public ObservableCollectionExtended<EtapaRowViewModel> MyItems { get; } = new();

        public ObservableCollectionExtended<string> BrokenRules { get; } = new();

        protected virtual async void LoadEtapas(bool nomesActius)
        {
            MyItems.Clear();
            await OmplirAmbElsNousValors(nomesActius);
        }

        private async Task OmplirAmbElsNousValors(bool nomesActius)
        {
            // Preparar paràmetres al backend
            var esActiu = nomesActius ? true : (bool?)null;
            var parms = new DTO.i.DTOs.EsActiuParms(esActiu: esActiu);

            // Petició al backend            
            using var bl = SuperContext.GetBLOperation<IEtapaSet>();
            var dto = await bl.FromPredicate(parms);

            // 
            BrokenRules.Clear();
            BrokenRules.AddRange(dto.BrokenRules.Select(x => x.Message));


            // Ha fallat la petició
            if (dto.Data == null)
                throw new Exception("Error en fer petició al backend"); // ToDo: gestionar broken rules            

            // Tenim els resultats
            var newItems =
                dto
                .Data
                .Select(x => new EtapaRowViewModel(x, ModeLookup));

            MyItems.AddRange(newItems);

            //
            PaginatedMsg =
                (dto.Total > dto.TakeRequested) ?
                $"Mostrant els {newItems.Count()} primers resultats de {dto.Total} seleccionats" :
                $"Seleccionats {newItems.Count()} items";

        }

        // Warning
        private string _PaginatedMsg = string.Empty;
        public string PaginatedMsg
        {
            get => _PaginatedMsg;
            set => this.RaiseAndSetIfChanged(ref _PaginatedMsg, value);
        }

        // Filtre
        private bool _NomesActius = true;
        public bool NomesActius
        {
            get => _NomesActius;
            set => this.RaiseAndSetIfChanged(ref _NomesActius, value);
        }

        // Crear item
        public ICommand Create { get; }
        public Interaction<EtapaCreateViewModel, dtoo.Etapa?> ShowDialog { get; }


    }
}
