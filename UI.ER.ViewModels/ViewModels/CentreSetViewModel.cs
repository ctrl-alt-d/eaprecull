using System.Linq;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
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
using DynamicData;
using CommonInterfaces;

namespace UI.ER.ViewModels.ViewModels
{

    public class CentreSetViewModel : ViewModelBase
    {
        protected virtual ICentreSet BLCentres() => SuperContext.GetBLOperation<ICentreSet>();
        public bool ModeLookup {get;}
        public CentreSetViewModel(bool modeLookup = false)
        {

            ModeLookup = modeLookup;

            SourceItems
                .ToObservableChangeSet(t=>t.Id)
                .Bind(out _MyItems)
                .Subscribe();

            // Filtre
            this
                .WhenAnyValue(x => x.NomesActius)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(nomesActius => LoadCentres(nomesActius))
                ;

            // Create
            ShowDialog = new Interaction<CentreCreateViewModel, dtoo.Centre?>();

            Create = ReactiveCommand.CreateFromTask(async () =>
            {
                var update = new CentreCreateViewModel();

                var data = await ShowDialog.Handle(update);

                if (data != null) {
                    var item = new CentreRowViewModel(data, ModeLookup);
                    SourceItems.Insert(0, item);
                }
            });


        }
        public ObservableCollectionExtended<CentreRowViewModel> SourceItems { get; } = new();
        private readonly ReadOnlyObservableCollection<CentreRowViewModel> _MyItems;
        public ReadOnlyObservableCollection<CentreRowViewModel> MyItems => _MyItems;

        public RangeObservableCollection<string> BrokenRules { get; } = new();

        protected virtual async void LoadCentres(bool nomesActius)
        {
            // Nota: aquesta tasca triga molt, la UX és pobra 
            // https://stackoverflow.com/questions/68740471/update-ui-inside-a-suscribed-task.
            SourceItems.Clear();
            await OmplirAmbElsNousValors(nomesActius);
        }

        private async Task OmplirAmbElsNousValors(bool nomesActius)
        {
            // Preparar paràmetres al backend
            var esActiu = nomesActius ? true : (bool?)null;            
            var parms = new DTO.i.DTOs.EsActiuParms(esActiu: esActiu);

            // Petició al backend            
            using var bl = BLCentres();
            var dto = await bl.FromPredicate(parms);

            // 
            BrokenRules.Clear();
            BrokenRules.AddRange(dto.BrokenRules.Select(x=>x.Message));
            

            // Ha fallat la petició
            if (dto.Data == null)
                throw new Exception("Error en fer petició al backend"); // ToDo: gestionar broken rules            

            // Tenim els resultats
            var newItems =
                dto
                .Data
                .Select(x => new CentreRowViewModel(x, ModeLookup));

            SourceItems.AddRange(newItems);

            //
            PaginatedMsg = 
                (dto.Total > dto.TakeRequested) ?
                $"Mostrant els {newItems.Count()} primers resultats d'un total de {dto.Total}" :
                string.Empty;
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
        public Interaction<CentreCreateViewModel, dtoo.Centre?> ShowDialog { get; }


    }
}
