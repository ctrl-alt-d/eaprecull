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
        protected virtual ICentreGetSet BLCentres() => SuperContext.GetBLOperation<ICentreGetSet>();
        private IIdEtiquetaDescripcio? _SelectedItem;
        public IIdEtiquetaDescripcio? SelectedItem
        {
            get => _SelectedItem;
            set => this.RaiseAndSetIfChanged(ref _SelectedItem, value);
        }

        public Action<IIdEtiquetaDescripcio>? ModeLookup {get;}
        public CentreSetViewModel(bool? modeLookup = null)
        {

            if (modeLookup ?? false)
                ModeLookup = (i) => this.SelectedItem = i;

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
                    MyItems.Insert(0, item);
                }
            });

        }
        public RangeObservableCollection<CentreRowViewModel> MyItems { get; } = new();
        public RangeObservableCollection<string> BrokenRules { get; } = new();

        protected virtual async void LoadCentres(bool nomesActius)
        {
            // Nota: aquesta tasca triga molt, la UX és pobra 
            // https://stackoverflow.com/questions/68740471/update-ui-inside-a-suscribed-task.
            MyItems.ClearSilently();
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
            MyItems.AddRange(newItems);
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
