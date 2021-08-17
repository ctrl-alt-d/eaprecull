using System.Reactive;
using ReactiveUI;
using dtoo = DTO.o.DTOs;
using CommonInterfaces;
using System.Threading.Tasks;
using UI.ER.AvaloniaUI.Services;
using BusinessLayer.Abstract.Services;
using System.Reactive.Concurrency;
using dtoi = DTO.i.DTOs;
using System.ComponentModel;
using UI.ER.ViewModels.Common;
using System.Linq;
using System.Windows.Input;
using System.Reactive.Linq;
using DynamicData.Binding;

namespace UI.ER.ViewModels.ViewModels
{
    public class AlumneCreateViewModel : ViewModelBase
    {

        protected virtual IAlumneCreate BLCreate() => SuperContext.GetBLOperation<IAlumneCreate>();
        public AlumneCreateViewModel()
        {
            SubmitCommand = ReactiveCommand.CreateFromTask(CreateData);

            // --- configura lookup --
            ShowCentreLookup = new Interaction<Unit, IIdEtiquetaDescripcio?>();
            CentreLookup = ReactiveCommand.CreateFromTask(async () =>
            {
                var data = await ShowCentreLookup.Handle(Unit.Default);
                if (data != null)
                {
                    CentreTxt = data.Etiqueta;
                    CentreId = data.Id;
                }
            });

        }

        /*
                public string Nom { get; set; } = string.Empty;
                public string Cognoms { get; set; } = string.Empty;
                public DateTime? DataNaixement { get; set; }
                public Centre? CentreActual { get; set; }
                public CursAcademic CursDarreraActualitacioDades { get; set; } = default!;
                public Etapa? EtapaActual { get; set; }
                public DateTime? DataInformeNESENEE { get; set; }
                public string ObservacionsNESENEE { get; set; } = string.Empty;
                public DateTime? DataInformeNESENoNEE { get; set; }
                public string ObservacionsNESENoNEE { get; set; } = string.Empty;
        */
        public string _Nom = string.Empty;
        public string Nom
        {
            get => _Nom;
            set => this.RaiseAndSetIfChanged(ref _Nom, value);
        }

        public string _Cognoms = string.Empty;
        public string Cognoms
        {
            get => _Cognoms;
            set => this.RaiseAndSetIfChanged(ref _Cognoms, value);
        }

        protected virtual int? CentreId { get; set; }
        public string _CentreTxt = string.Empty;
        public string CentreTxt
        {
            get => _CentreTxt;
            set => this.RaiseAndSetIfChanged(ref _CentreTxt, value);
        }

        private void DTO2ModelView(dtoo.Alumne? data)
        {
            if (data == null) return;

            Nom = data.Nom;
            Cognoms = data.Cognoms;
            CentreTxt = data.CentreActual?.Etiqueta ?? string.Empty;

        }

        public virtual async Task<dtoo.Alumne?> CreateData()
        {
            BrokenRules.Clear();

            // preparar paràmetres
            var parms = new dtoi.AlumneCreateParms(
                Nom,
                Cognoms,
                null,
                CentreId,
                1,
                1,
                null,
                "",
                null,
                ""
            );

            // cridar backend
            using var bl = BLCreate();
            var dto = await bl.Create(parms);
            var data = dto.Data;

            // actualitzar dades amb el resultat
            DTO2ModelView(data);
            BrokenRules.AddRange(dto.BrokenRules.Select(x => x.Message));

            SuccessfullySaved = data != null && !dto.BrokenRules.Any();

            return data;
        }

        public ObservableCollectionExtended<string> BrokenRules { get; } = new();

        public ReactiveCommand<Unit, dtoo.Alumne?> SubmitCommand { get; }

        private bool _Sortir;
        public bool SuccessfullySaved
        {
            get { return _Sortir; }
            protected set { this.RaiseAndSetIfChanged(ref _Sortir, value); }
        }

        // ----------------------
        public ICommand CentreLookup { get; }
        public Interaction<Unit, IIdEtiquetaDescripcio?> ShowCentreLookup { get; }



    }
}