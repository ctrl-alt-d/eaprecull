using System.Linq;
using BusinessLayer.Abstract.Services;
using ReactiveUI;
using Dtoo = DTO.o.DTOs;
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
            ShowDialog = new Interaction<CursAcademicCreateViewModel, Dtoo.CursAcademic?>();

            Create = ReactiveCommand.CreateFromTask(async () =>
            {
                var update = new CursAcademicCreateViewModel();

                var data = await ShowDialog.Handle(update);

                if (data != null)
                {
                    var item = new CursAcademicRowViewModel(data, MyItems, ModeLookup);
                    MyItems.Insert(0, item);

                    // Si el nou curs és actiu, desactivar tots els altres a la UI
                    if (data.EsActiu)
                    {
                        foreach (var curs in MyItems.Where(x => x.Id != data.Id))
                        {
                            curs.EsActiu = false;
                            curs.Estat = "Desactivat";
                        }
                    }
                }
            });


        }
        public ObservableCollectionExtended<CursAcademicRowViewModel> MyItems { get; } = new();

        public ObservableCollectionExtended<string> BrokenRules { get; } = new();

        protected virtual async void LoadCursAcademics(bool nomesActius)
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
            using var bl = SuperContext.GetBLOperation<ICursAcademicSet>();
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

        // Loading
        private bool _Loading = true;
        public bool Loading
        {
            get => _Loading;
            set => this.RaiseAndSetIfChanged(ref _Loading, value);
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
        public Interaction<CursAcademicCreateViewModel, Dtoo.CursAcademic?> ShowDialog { get; }


    }
}
