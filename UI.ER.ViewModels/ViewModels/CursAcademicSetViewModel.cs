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

    public class CursAcademicSetViewModel : ViewModelBase
    {
        protected virtual ICursAcademicGetSet BLCursAcademics() => SuperContext.GetBLOperation<ICursAcademicGetSet>();
        private IIdEtiquetaDescripcio? _SelectedItem;
        public IIdEtiquetaDescripcio? SelectedItem
        {
            get => _SelectedItem;
            set => this.RaiseAndSetIfChanged(ref _SelectedItem, value);
        }

        public Action<IIdEtiquetaDescripcio>? ModeLookup {get;}
        public CursAcademicSetViewModel(bool? modeLookup = null)
        {

            if (modeLookup ?? false)
                ModeLookup = (i) => this.SelectedItem = i;

            SourceItems
                .ToObservableChangeSet(t=>t.Id)
                // .Filter(x=> !NomesActius || x.EsActiu == NomesActius)
                .Bind(out _MyItems)
                .Subscribe();


            // Filtre
            this
                .WhenAnyValue(x => x.NomesActius)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(nomesActius => LoadCursAcademics(nomesActius))
                ;

            // Create
            ShowDialog = new Interaction<CursAcademicCreateViewModel, dtoo.CursAcademic?>();

            Create = ReactiveCommand.CreateFromTask(async () =>
            {
                var update = new CursAcademicCreateViewModel();

                var data = await ShowDialog.Handle(update);

                if (data != null) {
                    var item = new CursAcademicRowViewModel(data, SourceItems, ModeLookup);
                    SourceItems.Insert(0, item);
                }
            });


        }
        public ObservableCollectionExtended<CursAcademicRowViewModel> SourceItems { get; } = new();
        private readonly ReadOnlyObservableCollection<CursAcademicRowViewModel> _MyItems;
        public ReadOnlyObservableCollection<CursAcademicRowViewModel> MyItems => _MyItems;


        public RangeObservableCollection<string> BrokenRules { get; } = new();

        protected virtual async void LoadCursAcademics(bool nomesActius)
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
            using var bl = BLCursAcademics();
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
                .Select(x => new CursAcademicRowViewModel(x, SourceItems, ModeLookup));
            SourceItems.AddRange(newItems);
        }

        // Filtre
        private bool _NomesActius = false;
        public bool NomesActius
        {
            get => _NomesActius;
            set => this.RaiseAndSetIfChanged(ref _NomesActius, value);
        }

        // Crear item
        public ICommand Create { get; }
        public Interaction<CursAcademicCreateViewModel, dtoo.CursAcademic?> ShowDialog { get; }


    }
}
