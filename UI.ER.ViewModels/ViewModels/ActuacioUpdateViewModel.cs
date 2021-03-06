using System.Reactive;
using ReactiveUI;
using dtoo = DTO.o.DTOs;
using CommonInterfaces;
using System.Threading.Tasks;
using UI.ER.AvaloniaUI.Services;
using BusinessLayer.Abstract.Services;
using System.Reactive.Concurrency;
using dtoi = DTO.i.DTOs;
using System.Linq;
using System.Windows.Input;
using System.Reactive.Linq;
using DynamicData.Binding;
using System;
using ReactiveUI.Validation.Extensions;

namespace UI.ER.ViewModels.ViewModels
{
    public class ActuacioUpdateViewModel : ViewModelBase
    {

        protected virtual IActuacioUpdate BLUpdate() => SuperContext.GetBLOperation<IActuacioUpdate>();
        public ActuacioUpdateViewModel(int id)
        {
            Id = id;
            RxApp.MainThreadScheduler.Schedule(LoadDadesInicials);

            SubmitCommand = ReactiveCommand.CreateFromTask(UpdateData, this.IsValid());

            // --- configura lookup Alumne ---
            ShowAlumneLookup = new Interaction<Unit, IIdEtiquetaDescripcio?>();
            AlumneLookupCommand = ReactiveCommand.CreateFromTask(DoAlumneLookup);
            AlumneClearCommand = ReactiveCommand.CreateFromTask(DoAlumneClear);

            // --- configura lookup TipusActuacio ---
            ShowTipusActuacioLookup = new Interaction<Unit, IIdEtiquetaDescripcio?>();
            TipusActuacioLookupCommand = ReactiveCommand.CreateFromTask(DoTipusActuacioLookup);
            TipusActuacioClearCommand = ReactiveCommand.CreateFromTask(DoTipusActuacioClear);

            // --- configura lookup CursActuacio ---
            ShowCursActuacioLookup = new Interaction<Unit, IIdEtiquetaDescripcio?>();
            CursActuacioLookupCommand = ReactiveCommand.CreateFromTask(DoCursActuacioLookup);
            CursActuacioClearCommand = ReactiveCommand.CreateFromTask(DoCursActuacioClear);

            // --- configura lookup Centre ---
            ShowCentreLookup = new Interaction<Unit, IIdEtiquetaDescripcio?>();
            CentreLookupCommand = ReactiveCommand.CreateFromTask(DoCentreLookup);
            CentreClearCommand = ReactiveCommand.CreateFromTask(DoCentreClear);

            // --- configura lookup EtapaAlMomentDeLactuacio ---
            ShowEtapaAlMomentDeLactuacioLookup = new Interaction<Unit, IIdEtiquetaDescripcio?>();
            EtapaAlMomentDeLactuacioLookupCommand = ReactiveCommand.CreateFromTask(DoEtapaAlMomentDeLactuacioLookup);
            EtapaAlMomentDeLactuacioClearCommand = ReactiveCommand.CreateFromTask(DoEtapaAlMomentDeLactuacioClear);

            SetValidations();
            DealWithDates();
        }

        protected virtual async void LoadDadesInicials()
        {
            // Clear brokenRules
            BrokenRules.Clear();

            // Backend request
            using var bl = SuperContext.GetBLOperation<IActuacioSet>();
            var dto = await bl.FromId(Id);

            // Update UI
            BrokenRules.AddRange(dto.BrokenRules.Select(x => x.Message));
            DTO2ModelView(dto.Data);
        }

        private async Task OnChangeAlumne(int? alumneId)
        {
            AlumneId = null;
            AlumneTxt = string.Empty;

            // Centre
            CentreId = null;
            CentreTxt = string.Empty;

            // Etapa
            EtapaAlMomentDeLactuacioId = null;
            EtapaAlMomentDeLactuacioTxt = string.Empty;

            // Nivell
            NivellAlMomentDeLactuacio = string.Empty;

            // Sense alumne? marxem
            if (alumneId == null) return;

            // Amb alumne? Portem les dades de l'alumne cap aqu??
            using (var bl = SuperContext.GetBLOperation<IAlumneSet>())
            {
                var dto = await bl.FromId(alumneId!.Value);
                var data = dto.Data;

                // Alumne
                AlumneId = data?.Id;
                AlumneTxt = data?.Etiqueta ?? string.Empty;

                // Centre
                CentreId = data?.CentreActual?.Id;
                CentreTxt = data?.CentreActual?.Etiqueta ?? string.Empty;

                // Etapa
                EtapaAlMomentDeLactuacioId = data?.EtapaActual?.Id;
                EtapaAlMomentDeLactuacioTxt = data?.EtapaActual?.Etiqueta ?? string.Empty;

                // Nivell
                NivellAlMomentDeLactuacio = data?.NivellActual ?? string.Empty;
            }
        }

        private void DealWithDates()
        {
            this
                .WhenAnyValue(x => x.MomentDeLactuacioTxt)
                .Subscribe(x => this.MomentDeLactuacio = StringDateConverter.ConvertBack(x));

            this
                .WhenAnyValue(x => x.MinutsDuradaActuacioTxt)
                .Subscribe(x => this.MinutsDuradaActuacio = StringIntConverter.ConvertBack(x));
        }

        private void SetValidations()
        {
            this.ValidationRule(
                x => x.AlumneTxt,
                value => !string.IsNullOrEmpty(value),
                "Cal informar l'alumne sobre el que es fa l'actuaci??");

            this.ValidationRule(
                x => x.TipusActuacioTxt,
                value => !string.IsNullOrEmpty(value),
                "Cal informar el tipus d'actuaci?? realitzat");

            this.ValidationRule(
                x => x.MomentDeLactuacioTxt,
                value => StringDateConverter.NullableDataCorrecte(value),
                "Comprova el format de la data: dd.mm.aaaa");

            this.ValidationRule(
                x => x.CursActuacioTxt,
                value => !string.IsNullOrEmpty(value),
                "Cal informar el curs de l'actuaci??");

            this.ValidationRule(
                x => x.CentreTxt,
                value => !string.IsNullOrEmpty(value),
                "Cal informar el centre de l'alumne al moment de l'actuaci??");

            this.ValidationRule(
                x => x.EtapaAlMomentDeLactuacioTxt,
                value => !string.IsNullOrEmpty(value),
                "Cal informar l'etapa de l'alumne al moment de l'actuaci??");

            this.ValidationRule(
                x => x.NivellAlMomentDeLactuacio,
                value => !string.IsNullOrEmpty(value),
                "Cal informar el nivell de l'alumne al moment de l'actuaci??");

            this.ValidationRule(
                x => x.MinutsDuradaActuacioTxt,
                value => StringIntConverter.IntCorrecte(value),
                "Cal posar un n??mero (ex: 120)");

        }

        public int Id { get; }
        public string IdTxt => $"Alumne #{Id}";


        //
        protected virtual int? AlumneId { get; set; }
        public string _AlumneTxt = string.Empty;
        public string AlumneTxt
        {
            get => _AlumneTxt;
            set => this.RaiseAndSetIfChanged(ref _AlumneTxt, value);
        }

        //
        protected virtual int? TipusActuacioId { get; set; }
        public string _TipusActuacioTxt = string.Empty;
        public string TipusActuacioTxt
        {
            get => _TipusActuacioTxt;
            set => this.RaiseAndSetIfChanged(ref _TipusActuacioTxt, value);
        }

        //
        public string _ObservacionsTipusActuacio = string.Empty;
        public string ObservacionsTipusActuacio
        {
            get => _ObservacionsTipusActuacio;
            set => this.RaiseAndSetIfChanged(ref _ObservacionsTipusActuacio, value);
        }

        //
        public DateTime? _MomentDeLactuacio;
        public DateTime? MomentDeLactuacio
        {
            get => _MomentDeLactuacio;
            set => this.RaiseAndSetIfChanged(ref _MomentDeLactuacio, value);
        }

        public string _MomentDeLactuacioTxt = string.Empty;
        public string MomentDeLactuacioTxt
        {
            get => _MomentDeLactuacioTxt;
            set => this.RaiseAndSetIfChanged(ref _MomentDeLactuacioTxt, value);
        }

        //
        protected virtual int? CursActuacioId { get; set; }
        public string _CursActuacioTxt = string.Empty;
        public string CursActuacioTxt
        {
            get => _CursActuacioTxt;
            set => this.RaiseAndSetIfChanged(ref _CursActuacioTxt, value);
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
        protected virtual int? EtapaAlMomentDeLactuacioId { get; set; }
        public string _EtapaAlMomentDeLactuacioTxt = string.Empty;
        public string EtapaAlMomentDeLactuacioTxt
        {
            get => _EtapaAlMomentDeLactuacioTxt;
            set => this.RaiseAndSetIfChanged(ref _EtapaAlMomentDeLactuacioTxt, value);
        }


        //
        public string _NivellAlMomentDeLactuacio = string.Empty;
        public string NivellAlMomentDeLactuacio
        {
            get => _NivellAlMomentDeLactuacio;
            set => this.RaiseAndSetIfChanged(ref _NivellAlMomentDeLactuacio, value);
        }

        //
        public string _MinutsDuradaActuacioTxt = "0";
        public string MinutsDuradaActuacioTxt
        {
            get => _MinutsDuradaActuacioTxt;
            set => this.RaiseAndSetIfChanged(ref _MinutsDuradaActuacioTxt, value);
        }

        public int _MinutsDuradaActuacio;
        public int MinutsDuradaActuacio
        {
            get => _MinutsDuradaActuacio;
            set => this.RaiseAndSetIfChanged(ref _MinutsDuradaActuacio, value);
        }

        //
        public string _DescripcioActuacio = string.Empty;
        public string DescripcioActuacio
        {
            get => _DescripcioActuacio;
            set => this.RaiseAndSetIfChanged(ref _DescripcioActuacio, value);
        }
        //
        private void DTO2ModelView(dtoo.Actuacio? data)
        {
            if (data == null) return;

            AlumneTxt = data.Alumne.Etiqueta;
            AlumneId = data.Alumne.Id;

            TipusActuacioTxt = data.TipusActuacio.Etiqueta;
            TipusActuacioId = data.TipusActuacio.Id;

            ObservacionsTipusActuacio = data.ObservacionsTipusActuacio;

            MomentDeLactuacio = data.MomentDeLactuacio;
            MomentDeLactuacioTxt = StringDateConverter.Convert(data.MomentDeLactuacio); // Limitacions avalonia

            CursActuacioId = data.CursActuacio.Id;
            CursActuacioTxt = data.CursActuacio.Etiqueta;

            CentreId = data.CentreAlMomentDeLactuacio.Id;
            CentreTxt = data.CentreAlMomentDeLactuacio.Etiqueta;

            EtapaAlMomentDeLactuacioId = data.EtapaAlMomentDeLactuacio.Id;
            EtapaAlMomentDeLactuacioTxt = data.EtapaAlMomentDeLactuacio.Etiqueta;

            NivellAlMomentDeLactuacio = data.NivellAlMomentDeLactuacio;

            MinutsDuradaActuacio = data.MinutsDuradaActuacio;
            MinutsDuradaActuacioTxt = StringIntConverter.Convert(data.MinutsDuradaActuacio);  // Limitacions avalonia

            DescripcioActuacio = data.DescripcioActuacio;
        }

        //


        public virtual async Task<dtoo.Actuacio?> UpdateData()
        {
            BrokenRules.Clear();

            // preparar par??metres
            var parms = new dtoi.ActuacioUpdateParms(
                Id,
                AlumneId!.Value,
                TipusActuacioId!.Value,
                ObservacionsTipusActuacio,
                MomentDeLactuacio!.Value,
                CursActuacioId!.Value,
                CentreId!.Value,
                EtapaAlMomentDeLactuacioId!.Value,
                NivellAlMomentDeLactuacio,
                MinutsDuradaActuacio,
                DescripcioActuacio
            );

            // cridar backend
            using var bl = BLUpdate();
            var dto = await bl.Update(parms);
            var data = dto.Data;

            // actualitzar dades amb el resultat
            DTO2ModelView(data);
            BrokenRules.AddRange(dto.BrokenRules.Select(x => x.Message));

            return data;
        }

        public ObservableCollectionExtended<string> BrokenRules { get; } = new();

        public ReactiveCommand<Unit, dtoo.Actuacio?> SubmitCommand { get; }


        // --- Alumne ---
        public ICommand AlumneLookupCommand { get; }
        public Interaction<Unit, IIdEtiquetaDescripcio?> ShowAlumneLookup { get; }
        private async Task DoAlumneLookup()
        {
            var data = await ShowAlumneLookup.Handle(Unit.Default);
            await OnChangeAlumne(data.Id);
        }
        public ICommand AlumneClearCommand { get; }
        private async Task DoAlumneClear()
        {
            AlumneTxt = "";
            AlumneId = null;
            await Task.CompletedTask;
        }

        // --- TipusActuacio ---
        public ICommand TipusActuacioLookupCommand { get; }
        public Interaction<Unit, IIdEtiquetaDescripcio?> ShowTipusActuacioLookup { get; }
        private async Task DoTipusActuacioLookup()
        {
            var data = await ShowTipusActuacioLookup.Handle(Unit.Default);
            if (data != null)
            {
                TipusActuacioTxt = data.Etiqueta;
                TipusActuacioId = data.Id;
            }
        }
        public ICommand TipusActuacioClearCommand { get; }
        private async Task DoTipusActuacioClear()
        {
            TipusActuacioTxt = "";
            TipusActuacioId = null;
            await Task.CompletedTask;
        }

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
        public ICommand CentreClearCommand { get; }
        private async Task DoCentreClear()
        {
            CentreTxt = "";
            CentreId = null;
            await Task.CompletedTask;
        }

        // --- CursActuacio ---
        public ICommand CursActuacioLookupCommand { get; }
        public Interaction<Unit, IIdEtiquetaDescripcio?> ShowCursActuacioLookup { get; }
        private async Task DoCursActuacioLookup()
        {
            var data = await ShowCursActuacioLookup.Handle(Unit.Default);
            if (data != null)
            {
                CursActuacioTxt = data.Etiqueta;
                CursActuacioId = data.Id;
            }
        }

        public ICommand CursActuacioClearCommand { get; }
        private async Task DoCursActuacioClear()
        {
            CursActuacioTxt = "";
            CursActuacioId = null;
            await Task.CompletedTask;
        }

        // --- EtapaAlMomentDeLactuacio ---
        public ICommand EtapaAlMomentDeLactuacioLookupCommand { get; }
        public Interaction<Unit, IIdEtiquetaDescripcio?> ShowEtapaAlMomentDeLactuacioLookup { get; }
        private async Task DoEtapaAlMomentDeLactuacioLookup()
        {
            var data = await ShowEtapaAlMomentDeLactuacioLookup.Handle(Unit.Default);
            if (data != null)
            {
                EtapaAlMomentDeLactuacioTxt = data.Etiqueta;
                EtapaAlMomentDeLactuacioId = data.Id;
            }
        }

        public ICommand EtapaAlMomentDeLactuacioClearCommand { get; }
        private async Task DoEtapaAlMomentDeLactuacioClear()
        {
            EtapaAlMomentDeLactuacioTxt = "";
            EtapaAlMomentDeLactuacioId = null;
            await Task.CompletedTask;
        }

        // ----


    }
}