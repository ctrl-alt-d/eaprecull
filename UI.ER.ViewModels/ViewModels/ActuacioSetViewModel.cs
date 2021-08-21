using System.Linq;
using BusinessLayer.Abstract.Services;
using ReactiveUI;
using dtoo = DTO.o.DTOs;
using dtoi = DTO.i.DTOs;
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
        public ActuacioSetViewModel(bool modeLookup = false)
        {

            ModeLookup = modeLookup;

            // Filtre
            var SearchStringObserver =
                this
                .WhenAnyValue(x=>x.SearchString)
                .Throttle(TimeSpan.FromMilliseconds(400));

            this
                .WhenAnyValue(x=>x.AlumneId)
                .CombineLatest(
                        SearchStringObserver,
                        (alumneId, searchString) => 
                        (alumneId, searchString)
                )
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(t => LoadActuacioSet(t.alumneId, t.searchString))
                ;

            // Create
            ShowDialog = new Interaction<ActuacioCreateViewModel, dtoo.Actuacio?>();

            Create = ReactiveCommand.CreateFromTask(async () =>
            {
                var update = new ActuacioCreateViewModel();
                var data = await ShowDialog.Handle(update);
                var cursActual_dto = await SuperContext.GetBLOperation<ICursAcademicSet>().FromPredicate(new dtoi.EsActiuParms(true));
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

        protected virtual async void LoadActuacioSet(int? alumneId, string searchString)
        {
            Loading = true;
            MyItems.Clear();

            // Preparar paràmetres al backend
            var parms = new DTO.i.DTOs.ActuacioSearchParms(
                searchString: searchString,
                alumneId: alumneId
            );

            // Petició al backend            
            using var bl = SuperContext.GetBLOperation<IActuacioSet>();
            var dto = await bl.FromPredicate(parms);

            // 
            BrokenRules.Clear();
            BrokenRules.AddRange(dto.BrokenRules.Select(x => x.Message));


            // Ha fallat la petició
            if (dto.Data == null)
                throw new Exception("Error en fer petició al backend"); // ToDo: gestionar broken rules            

            //
            var cursActual_dto = await SuperContext.GetBLOperation<ICursAcademicSet>().FromPredicate(new dtoi.EsActiuParms(true));
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
                $"Mostrant els {newItems.Count()} primers resultats d'un total de {dto.Total}" :
                string.Empty;

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

        // Crear item
        public ICommand Create { get; }
        public Interaction<ActuacioCreateViewModel, dtoo.Actuacio?> ShowDialog { get; }


    }
}
