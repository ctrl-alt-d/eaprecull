using System.Linq;
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

    public class CursAcademicSetViewModel : ViewModelBase
    {
        public bool ModeLookup { get; }
        public CursAcademicSetViewModel(bool modeLookup = false)
        {

            ModeLookup = modeLookup;

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

                if (data != null)
                {
                    var item = new CursAcademicRowViewModel(data, MyItems, ModeLookup);
                    MyItems.Insert(0, item);
                }
            });


        }
        public ObservableCollectionExtended<CursAcademicRowViewModel> MyItems { get; } = new();

        public ObservableCollectionExtended<string> BrokenRules { get; } = new();

        protected virtual async void LoadCursAcademics(bool nomesActius)
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
            using var bl = SuperContext.GetBLOperation<ICursAcademicSet>();
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
                .Select(x => new CursAcademicRowViewModel(x, MyItems, ModeLookup));

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
