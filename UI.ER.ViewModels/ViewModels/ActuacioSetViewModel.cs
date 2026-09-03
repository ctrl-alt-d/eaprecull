using BusinessLayer.Abstract;
using BusinessLayer.Abstract.Services;
using ReactiveUI;
using Dtoo = DTO.o.DTOs;
using Dtoi = DTO.i.DTOs;
using System.Reactive.Linq;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using BusinessLayer.Abstract.Generic;
using UI.ER.ViewModels.ViewModels.Base;

namespace UI.ER.ViewModels.ViewModels
{

    public class ActuacioSetViewModel
        : SetViewModelBase<ActuacioRowViewModel, Dtoo.Actuacio>,
          ISetViewModel<ActuacioCreateViewModel, Dtoo.Actuacio>
    {
        public ActuacioSetViewModel(IServiceFactory serveis, bool modeLookup = false, int? alumneId = null)
            : base(serveis, modeLookup)
        {
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
                .Subscribe(_ => CarregaAra())
                ;

            // Create
            ShowDialog = new Interaction<ActuacioCreateViewModel, Dtoo.Actuacio?>();

            Create = ReactiveCommand.CreateFromTask(async () =>
            {
                var update = new ActuacioCreateViewModel(Serveis, alumneId: AlumneId);
                var data = await ShowDialog.Handle(update);

                if (data == null)
                    return;

                MyItems.Insert(0, CreaFila(data));
            });

        }

        protected override async Task<OperationResults<Dtoo.Actuacio>> Consulta()
        {
            var esActiu =
                NomesAlumnesActius && !AlumneId.HasValue ?  // si tenim id alumne el mostrem sempre
                true :
                (bool?)null;

            // Preparar paràmetres al backend
            var Parms = new Dtoi.ActuacioSearchParms(
                take: 200,
                searchString: SearchString,
                alumneId: AlumneId,
                alumneEsActiu: esActiu
            );

            // Petició al backend
            using var bl = Serveis.GetBLOperation<IActuacioSet>();
            return await bl.FromPredicate(Parms);
        }

        protected override ActuacioRowViewModel CreaFila(Dtoo.Actuacio dto)
            => new(Serveis, dto, ModeLookup);

        // Filtre
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
