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
using System;

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
            CentreLookupCommand = ReactiveCommand.CreateFromTask( DoCentreLookup );

        }

        //
        public string _Nom = string.Empty;
        public string Nom
        {
            get => _Nom;
            set => this.RaiseAndSetIfChanged(ref _Nom, value);
        }

        //
        public string _Cognoms = string.Empty;
        public string Cognoms
        {
            get => _Cognoms;
            set => this.RaiseAndSetIfChanged(ref _Cognoms, value);
        }

        //
        public DateTime? _DataNaixement;
        public DateTime? DataNaixement
        {
            get => _DataNaixement;
            set => this.RaiseAndSetIfChanged(ref _DataNaixement, value);
        }

        //
        protected virtual int? CentreId { get; set; }
        public string _CentreTxt = string.Empty;
        public string CentreTxt
        {
            get => _CentreTxt;
            set => this.RaiseAndSetIfChanged(ref _CentreTxt, value);
        }

        //
        protected virtual int CursDarreraActualitacioDadesId { get; set; }
        public string _CursDarreraActualitacioDadesTxt = string.Empty;
        public string CursDarreraActualitacioDadesTxt
        {
            get => _CursDarreraActualitacioDadesTxt;
            set => this.RaiseAndSetIfChanged(ref _CursDarreraActualitacioDadesTxt, value);
        }

        //
        protected virtual int? EtapaActualId { get; set; }
        public string _EtapaActualTxt = string.Empty;
        public string EtapaActualTxt
        {
            get => _EtapaActualTxt;
            set => this.RaiseAndSetIfChanged(ref _EtapaActualTxt, value);
        }

        //
        public DateTime? _DataInformeNESENEE;
        public DateTime? DataInformeNESENEE
        {
            get => _DataInformeNESENEE;
            set => this.RaiseAndSetIfChanged(ref _DataInformeNESENEE, value);
        }

        //
        public string _ObservacionsNESENEE = string.Empty;
        public string ObservacionsNESENEE
        {
            get => _ObservacionsNESENEE;
            set => this.RaiseAndSetIfChanged(ref _ObservacionsNESENEE, value);
        }

        //
        public DateTime? _DataInformeNESENoNEE;
        public DateTime? DataInformeNESENoNEE
        {
            get => _DataInformeNESENoNEE;
            set => this.RaiseAndSetIfChanged(ref _DataInformeNESENoNEE, value);
        }

        //
        public string _ObservacionsNESENoNEE = string.Empty;
        public string ObservacionsNESENoNEE
        {
            get => _ObservacionsNESENoNEE;
            set => this.RaiseAndSetIfChanged(ref _ObservacionsNESENoNEE, value);
        }

        //
        public string _Tags = string.Empty;
        public string Tags
        {
            get => _Tags;
            set => this.RaiseAndSetIfChanged(ref _Tags, value);
        }
        //
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
                DataNaixement,
                CentreId,
                CursDarreraActualitacioDadesId,
                EtapaActualId,
                DataInformeNESENEE,
                ObservacionsNESENEE,
                DataInformeNESENoNEE,
                ObservacionsNESENoNEE,
                Tags
            );

            // cridar backend
            using var bl = BLCreate();
            var dto = await bl.Create(parms);
            var data = dto.Data;

            // actualitzar dades amb el resultat
            DTO2ModelView(data);
            BrokenRules.AddRange(dto.BrokenRules.Select(x => x.Message));

            return data;
        }

        public ObservableCollectionExtended<string> BrokenRules { get; } = new();

        public ReactiveCommand<Unit, dtoo.Alumne?> SubmitCommand { get; }

        // ----------------------
        public ICommand CentreLookupCommand { get; }
        public Interaction<Unit, IIdEtiquetaDescripcio?> ShowCentreLookup { get; }
        private async Task DoCentreLookup()
        {
            var data = await ShowCentreLookup.Handle(Unit.Default);
            if (data != null)
            {
                CentreTxt = data.Etiqueta;
                CentreId = data.Id;
            }
        }

        // ----


    }
}