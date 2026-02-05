using System.Linq;
using BusinessLayer.Abstract.Services;
using ReactiveUI;
using Dtoo = DTO.o.DTOs;
using Dtoi = DTO.i.DTOs;
using UI.ER.AvaloniaUI.Services;
using System.Reactive.Linq;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using DynamicData.Binding;

namespace UI.ER.ViewModels.ViewModels
{

    public class ActuacioSetViewModel : ViewModelBase
    {
        public bool ModeLookup { get; }
        public ActuacioSetViewModel(bool modeLookup = false, int? alumneId = null)
        {

            ModeLookup = modeLookup;
            AlumneId = alumneId;

            // Filtre
            var SearchStringObserver =
                this
                .WhenAnyValue(x => x.SearchString)
                .Throttle(TimeSpan.FromMilliseconds(400));

            var NomesAlumnesActiusObserver =
                this
                .WhenAnyValue(x => x.NomesAlumnesActius);

            this
                .WhenAnyValue(x => x.AlumneId)
                .CombineLatest(
                        NomesAlumnesActiusObserver,
                        SearchStringObserver,
                        (alumneId, nomesAlumnesActius, searchString) =>
                        (alumneId, nomesAlumnesActius, searchString)
                )
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(t => LoadActuacioSet(t.nomesAlumnesActius, t.alumneId, t.searchString))
                ;

            // Create
            ShowDialog = new Interaction<ActuacioCreateViewModel, Dtoo.Actuacio?>();

            Create = ReactiveCommand.CreateFromTask(async () =>
            {
                var update = new ActuacioCreateViewModel(alumneId: AlumneId);
                var data = await ShowDialog.Handle(update);
                var cursActual_dto = await SuperContext.GetBLOperation<ICursAcademicSet>().FromPredicate(new Dtoi.EsActiuParms(true));
                var cursActual = cursActual_dto.Data?.FirstOrDefault();

                if (data != null)
                {
                    var item = new ActuacioRowViewModel(data, ModeLookup);
                    MyItems.Insert(0, item);
                }
            });

        }
        public ObservableCollectionExtended<ActuacioRowViewModel> MyItems { get; } = new();

        public ObservableCollectionExtended<string> BrokenRules { get; } = new();

        protected virtual async void LoadActuacioSet(bool nomesAlumnesActius, int? alumneId, string searchString)
        {
            Loading = true;
            MyItems.Clear();

            var esActiu =
                nomesAlumnesActius && !alumneId.HasValue ?  // si tenim id alumne el mostrem sempre
                true :
                (bool?)null;            

            // Preparar paràmetres al backend
            var Parms = new DTO.i.DTOs.ActuacioSearchParms(
                take: 200,
                searchString: searchString,
                alumneId: alumneId,
                alumneEsActiu: esActiu 
            );

            // Petició al backend            
            using var bl = SuperContext.GetBLOperation<IActuacioSet>();
            var dto = await bl.FromPredicate(Parms);

            // 
            BrokenRules.Clear();
            BrokenRules.AddRange(dto.BrokenRules.Select(x => x.Message));


            // Ha fallat la petició
            if (dto.Data == null)
                throw new Exception("Error en fer petició al backend"); // ToDo: gestionar broken rules            

            //
            var cursActual_dto = await SuperContext.GetBLOperation<ICursAcademicSet>().FromPredicate(new Dtoi.EsActiuParms(true));
            var cursActual = cursActual_dto.Data?.FirstOrDefault();

            // Tenim els resultats
            var newItems =
                dto
                .Data
                .Select(x => new ActuacioRowViewModel(x, ModeLookup));

            MyItems.AddRange(newItems);

            //
            PaginatedMsg =
                (dto.Total > dto.TakeRequested) ?
                $"Mostrant els {newItems.Count()} primers resultats de {dto.Total} seleccionats" :
                $"Seleccionats {newItems.Count()} items";

            Loading = false;
        }

        // Warning
        private string _PaginatedMsg = string.Empty;
        public string PaginatedMsg
        {
            get => _PaginatedMsg;
            set => this.RaiseAndSetIfChanged(ref _PaginatedMsg, value);
        }

        // Filtre
        private bool _Loading = true;
        public bool Loading
        {
            get => _Loading;
            set => this.RaiseAndSetIfChanged(ref _Loading, value);
        }

        private int? _AlumneId;
        public int? AlumneId
        {
            get => _AlumneId;
            set => this.RaiseAndSetIfChanged(ref _AlumneId, value);
        }

        private string _SearchString = string.Empty;
        public string SearchString
        {
            get => _SearchString;
            set => this.RaiseAndSetIfChanged(ref _SearchString, value);
        }

        private bool _NomesAlumnesActius = true;
        public bool NomesAlumnesActius
        {
            get => _NomesAlumnesActius;
            set => this.RaiseAndSetIfChanged(ref _NomesAlumnesActius, value);
        }

        // Crear item
        public ICommand Create { get; }
        public Interaction<ActuacioCreateViewModel, Dtoo.Actuacio?> ShowDialog { get; }


    }
}
