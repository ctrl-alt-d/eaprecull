using System.Reactive;
using ReactiveUI;
using dtoo = DTO.o.DTOs;
using CommonInterfaces;
using System.Threading.Tasks;
using UI.ER.AvaloniaUI.Services;
using BusinessLayer.Abstract.Services;
using System.Reactive.Concurrency;
using dtoi = DTO.i.DTOs;
using UI.ER.ViewModels.Common;
using System.Linq;
using DynamicData.Binding;
using System;
using ReactiveUI.Validation.Extensions;
using System.Windows.Input;
using System.Reactive.Linq;

namespace UI.ER.ViewModels.ViewModels
{
    public class AlumneUpdateViewModel : ViewModelBase, IId
    {

        public AlumneUpdateViewModel(int id)
        {
            Id = id;
            RxApp.MainThreadScheduler.Schedule(LoadData);

            SubmitCommand = ReactiveCommand.CreateFromTask(UpdateData, this.IsValid() );

            // --- configura lookup Centre ---
            ShowCentreLookup = new Interaction<Unit, IIdEtiquetaDescripcio?>();
            CentreLookupCommand = ReactiveCommand.CreateFromTask(DoCentreLookup);

            // --- configura lookup CursDarreraActualitacioDades ---
            ShowCursDarreraActualitacioDadesLookup = new Interaction<Unit, IIdEtiquetaDescripcio?>();
            CursDarreraActualitacioDadesLookupCommand = ReactiveCommand.CreateFromTask(DoCursDarreraActualitacioDadesLookup);
            CursDarreraActualitacioDadesClearCommand = ReactiveCommand.CreateFromTask(DoCursDarreraActualitacioDadesClear);

            // --- configura lookup EtapaActual ---
            ShowEtapaActualLookup = new Interaction<Unit, IIdEtiquetaDescripcio?>();
            EtapaActualLookupCommand = ReactiveCommand.CreateFromTask(DoEtapaActualLookup);

            SetValidations();
            DealWithDates();
        }

        protected virtual async void LoadDadesInicials()
        {
            using var bl = SuperContext.GetBLOperation<ICursAcademicSet>();
            var dto = await bl.FromPredicate(new dtoi.EsActiuParms(true));
            var cursActual = dto.Data?.FirstOrDefault();
            CursDarreraActualitacioDadesId = cursActual?.Id;
            CursDarreraActualitacioDadesTxt = cursActual?.Etiqueta ?? string.Empty;
        }

        private void DealWithDates()
        {
            this
                .WhenAnyValue(x => x.DataNaixementTxt)
                .Subscribe(x => this.DataNaixement = StringDateConverter.ConvertBack(x));

            this
                .WhenAnyValue(x => x.DataInformeNESENEETxt)
                .Subscribe(x => this.DataInformeNESENEE = StringDateConverter.ConvertBack(x));

            this
                .WhenAnyValue(x => x.DataInformeNESENoNEETxt)
                .Subscribe(x => this.DataInformeNESENoNEE = StringDateConverter.ConvertBack(x));
        }

        private void SetValidations()
        {
            this.ValidationRule(
                x => x.DataNaixementTxt,
                value => StringDateConverter.NullableDataCorrecte(value),
                "Comprova el format de la data: dd.mm.aaaa");

            this.ValidationRule(
                x => x.DataInformeNESENEETxt,
                value => StringDateConverter.NullableDataCorrecte(value),
                "Comprova el format de la data: dd.mm.aaaa");

            this.ValidationRule(
                x => x.DataInformeNESENoNEETxt,
                value => StringDateConverter.NullableDataCorrecte(value),
                "Comprova el format de la data: dd.mm.aaaa");

            this.ValidationRule(
                x => x.CursDarreraActualitacioDadesTxt,
                value => !string.IsNullOrEmpty( value ),
                "Cal informar el curs de la darrera actualització de dades (Cal que hi hagi un curs acadèmic activat)");

            this.ValidationRule(
                x => x.CentreTxt,
                value => !string.IsNullOrEmpty( value ),
                "Cal informar el Centre on cursa estudis");

            this.ValidationRule(
                x => x.Nom,
                value => !string.IsNullOrEmpty( value ),
                "Cal informar el nom de l'alumne");

            this.ValidationRule(
                x => x.Cognoms,
                value => !string.IsNullOrEmpty( value ),
                "Cal informar els Cognoms de l'alumne");

            this.ValidationRule(
                x => x.EtapaActualTxt,
                value => !string.IsNullOrEmpty( value ),
                "Cal informar l'etapa d'estudis actual");
        }

        public int Id { get; }
        public string IdTxt => $"Alumne #{Id}";



        private bool _EsActiu;
        public bool EsActiu
        {
            get { return _EsActiu; }
            protected set { this.RaiseAndSetIfChanged(ref _EsActiu, value); }
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

        public string _DataNaixementTxt = string.Empty;
        public string DataNaixementTxt
        {
            get => _DataNaixementTxt;
            set => this.RaiseAndSetIfChanged(ref _DataNaixementTxt, value);
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
        protected virtual int? CursDarreraActualitacioDadesId { get; set; }
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

        public string _DataInformeNESENEETxt = string.Empty;
        public string DataInformeNESENEETxt
        {
            get => _DataInformeNESENEETxt;
            set => this.RaiseAndSetIfChanged(ref _DataInformeNESENEETxt, value);
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

        public string _DataInformeNESENoNEETxt = string.Empty;
        public string DataInformeNESENoNEETxt
        {
            get => _DataInformeNESENoNEETxt;
            set => this.RaiseAndSetIfChanged(ref _DataInformeNESENoNEETxt, value);
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

        protected virtual async void LoadData()
        {
            // Clear brokenRules
            BrokenRules.Clear();

            // Backend request
            using var bl = SuperContext.GetBLOperation<IAlumneSet>();
            var dto = await bl.FromId(Id);

            // Update UI
            BrokenRules.AddRange(dto.BrokenRules.Select(x => x.Message));
            DTO2ModelView(dto.Data);
        }

        private void DTO2ModelView(dtoo.Alumne? data)
        {
            if (data == null) return;

            Nom = data.Nom;
            Cognoms = data.Cognoms;
            CentreTxt = data.CentreActual?.Etiqueta ?? string.Empty;

            DataNaixement = data.DataNaixement;
            CentreId = data.CentreActual?.Id;
            CursDarreraActualitacioDadesId = data.CursDarreraActualitacioDades?.Id;
            EtapaActualId = data.EtapaActual?.Id;
            DataInformeNESENEE =  data.DataInformeNESENEE;
            ObservacionsNESENEE = data.ObservacionsNESENEE;
            DataInformeNESENoNEE = data.DataInformeNESENoNEE;
            ObservacionsNESENoNEE= data.ObservacionsNESENoNEE;
            Tags= data.Tags;
            EsActiu = data.EsActiu;

        }

        public virtual async Task<dtoo.Alumne?> UpdateData()
        {
            // Clear brokenRules
            BrokenRules.Clear();

            // preparar paràmetres
            var parms = new dtoi.AlumneUpdateParms(
                Id,
                Nom,
                Cognoms, null, null, 
                1,
                null,
                null,
                "",
                null,
                "",
                "",
                EsActiu);

            // cridar backend
            using var bl = SuperContext.GetBLOperation<IAlumneUpdate>();
            var dto = await bl.Update(parms);
            var data = dto.Data;

            // actualitzar dades amb el resultat
            DTO2ModelView(data);
            BrokenRules.AddRange(dto.BrokenRules.Select(x => x.Message));

            //
            return data;
        }

        public ObservableCollectionExtended<string> BrokenRules { get; } = new();

        public ReactiveCommand<Unit, dtoo.Alumne?> SubmitCommand { get; }

        // --- Centre ---
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

        // --- CursDarreraActualitacioDades ---
        public ICommand CursDarreraActualitacioDadesLookupCommand { get; }
        public Interaction<Unit, IIdEtiquetaDescripcio?> ShowCursDarreraActualitacioDadesLookup { get; }
        private async Task DoCursDarreraActualitacioDadesLookup()
        {
            var data = await ShowCursDarreraActualitacioDadesLookup.Handle(Unit.Default);
            if (data != null)
            {
                CursDarreraActualitacioDadesTxt = data.Etiqueta;
                CursDarreraActualitacioDadesId = data.Id;
            }
        }

        // --- Darrera actualització dades
        public ICommand CursDarreraActualitacioDadesClearCommand {get; }
        private async Task DoCursDarreraActualitacioDadesClear()
        {
            CursDarreraActualitacioDadesTxt = "";
            CursDarreraActualitacioDadesId = null;
            await Task.CompletedTask;
        }

        // --- EtapaActual ---
        public ICommand EtapaActualLookupCommand { get; }
        public Interaction<Unit, IIdEtiquetaDescripcio?> ShowEtapaActualLookup { get; }
        private async Task DoEtapaActualLookup()
        {
            var data = await ShowEtapaActualLookup.Handle(Unit.Default);
            if (data != null)
            {
                EtapaActualTxt = data.Etiqueta;
                EtapaActualId = data.Id;
            }
        }


    }
}