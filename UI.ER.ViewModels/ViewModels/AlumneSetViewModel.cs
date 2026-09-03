using System.Linq;
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

    public class AlumneSetViewModel
        : SetViewModelBase<AlumneRowViewModel, Dtoo.Alumne>,
          ISetViewModel<AlumneCreateViewModel, Dtoo.Alumne>
    {
        public AlumneSetViewModel(IServiceFactory serveis, bool modeLookup = false)
            : base(serveis, modeLookup)
        {
            // Filtre
            var NomCognomsCentreObserver =
                this
                .WhenAnyValue(x => x.NomCognomsTagCentre)
                .Throttle(TimeSpan.FromMilliseconds(400));

            var OrdreAlfabeticObserver =
                this
                .WhenAnyValue(x => x.OrdreAlfabetic);

            this
                .WhenAnyValue(x => x.NomesActius)
                .CombineLatest(
                        NomCognomsCentreObserver,
                        OrdreAlfabeticObserver,
                        (nomesActius, NomCognomsTagCentre, ordreAlfabetic) =>
                        (nomesActius, NomCognomsTagCentre, ordreAlfabetic)
                )
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => CarregaAra())
                ;

            // Create
            ShowDialog = new Interaction<AlumneCreateViewModel, Dtoo.Alumne?>();

            Create = ReactiveCommand.CreateFromTask(async () =>
            {
                var update = new AlumneCreateViewModel(Serveis);
                var data = await ShowDialog.Handle(update);

                if (data == null)
                    return;

                await AbansDeCrearFiles();
                MyItems.Insert(0, CreaFila(data));
            });

        }

        protected override async Task<OperationResults<Dtoo.Alumne>> Consulta()
        {
            // Preparar paràmetres al backend
            var esActiu =
                NomesActius ?
                true :
                (bool?)null;

            var ordre =
                OrdreAlfabetic ?
                Dtoi.AlumneSearchParms.OrdreResultatsChoice.CognomsNom :
                Dtoi.AlumneSearchParms.OrdreResultatsChoice.DarreraModificacio;

            var Parms = new Dtoi.AlumneSearchParms(
                esActiu: esActiu,
                nomCognomsTagCentre: NomCognomsTagCentre,
                ordreResultats: ordre
            );

            // Petició al backend
            using var bl = Serveis.GetBLOperation<IAlumneSet>();
            return await bl.FromPredicate(Parms);
        }

        /// <summary>El curs actiu: la fila el necessita per saber si l'alumne està al dia.</summary>
        protected override async Task AbansDeCrearFiles()
        {
            using var blCurs = Serveis.GetBLOperation<ICursAcademicSet>();
            var cursActual_dto = await blCurs.FromPredicate(new Dtoi.EsActiuParms(true));
            CursActual = cursActual_dto.Data?.FirstOrDefault();
        }

        private Dtoo.CursAcademic? CursActual;

        protected override AlumneRowViewModel CreaFila(Dtoo.Alumne dto)
            => new(Serveis, dto, CursActual, ModeLookup);

        // Filtre
        private bool _NomesActius = true;
        public bool NomesActius
        {
            get => _NomesActius;
            set => this.RaiseAndSetIfChanged(ref _NomesActius, value);
        }

        private bool _OrdreAlfabetic = false;
        public bool OrdreAlfabetic
        {
            get => _OrdreAlfabetic;
            set => this.RaiseAndSetIfChanged(ref _OrdreAlfabetic, value);
        }

        private string _NomCognomsCentre = string.Empty;
        public string NomCognomsTagCentre
        {
            get => _NomCognomsCentre;
            set => this.RaiseAndSetIfChanged(ref _NomCognomsCentre, value);
        }

        // Crear item
        public ICommand Create { get; }
        public Interaction<AlumneCreateViewModel, Dtoo.Alumne?> ShowDialog { get; }


    }
}
