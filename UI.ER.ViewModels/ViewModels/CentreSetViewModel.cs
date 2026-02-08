using System.Linq;
using BusinessLayer.Abstract.Services;
using ReactiveUI;
using Dtoo = DTO.o.DTOs;
using UI.ER.ViewModels.Services;
using System.Reactive.Linq;
using System;
using System.Threading.Tasks;
using UI.ER.ViewModels.Common;
using System.Windows.Input;
using DynamicData.Binding;

namespace UI.ER.ViewModels.ViewModels
{

    public class CentreSetViewModel : ViewModelBase
    {
        public bool ModeLookup { get; }
        public CentreSetViewModel(bool modeLookup = false)
        {

            ModeLookup = modeLookup;

            // Filtre
            this
                .WhenAnyValue(x => x.NomesActius)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(nomesActius => LoadCentres(nomesActius))
                ;

            // Create
            ShowDialog = new Interaction<CentreCreateViewModel, Dtoo.Centre?>();

            Create = ReactiveCommand.CreateFromTask(async () =>
            {
                var update = new CentreCreateViewModel();

                var data = await ShowDialog.Handle(update);

                if (data != null)
                {
                    var item = new CentreRowViewModel(data, ModeLookup);
                    MyItems.Insert(0, item);
                }
            });


        }
        public ObservableCollectionExtended<CentreRowViewModel> MyItems { get; } = new();

        public ObservableCollectionExtended<string> BrokenRules { get; } = new();

        protected virtual async void LoadCentres(bool nomesActius)
        {
            Loading = true;
            MyItems.Clear();
            await OmplirAmbElsNousValors(nomesActius);
            Loading = false;
        }

        private async Task OmplirAmbElsNousValors(bool nomesActius)
        {
            // Preparar paràmetres al backend
            var esActiu = nomesActius ? true : (bool?)null;
            var Parms = new DTO.i.DTOs.EsActiuParms(esActiu: esActiu);

            // Petició al backend            
            using var bl = SuperContext.Resolve<ICentreSetAmbActuacions>();
            var dto = await bl.FromPredicate(Parms);

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
                .Cast<Dtoo.CentreAmbActuacions>()
                .Select(x => new CentreRowViewModel(x, ModeLookup));

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

        // Loading
        private bool _Loading = true;
        public bool Loading
        {
            get => _Loading;
            set => this.RaiseAndSetIfChanged(ref _Loading, value);
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
        public Interaction<CentreCreateViewModel, Dtoo.Centre?> ShowDialog { get; }


    }
}
