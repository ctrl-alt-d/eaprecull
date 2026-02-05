using ReactiveUI;
using System.Reactive.Linq;
using BusinessLayer.Abstract.Exceptions;
using System.Linq;
using DynamicData.Binding;
using UI.ER.AvaloniaUI.Services;
using BusinessLayer.Abstract.Services;
using System.Reactive.Concurrency;
using System.Reactive;
using System.Windows.Input;
using System.Threading.Tasks;
using CommonInterfaces;
using Dtoo = DTO.o.DTOs;
using System;

namespace UI.ER.ViewModels.ViewModels
{
    public class UtilitatsViewModel : ViewModelBase
    {
        public UtilitatsViewModel()
        {
            this
                .WhenAnyValue(x => x.NumTotalActuacions)
                .Subscribe(x => this.BotoPivotActivat = (x ?? 0) > 0);

            this
                .WhenAnyValue(x => x.OperacionsDelicadesActivat)
                .Subscribe(x => this.BotoSyncActivat = x);

            var ObservaBotoPivotActivat =
                this
                .WhenAnyValue(x => x.BotoPivotActivat );

            var ObservaBotoSyncActivat =
                this
                .WhenAnyValue(x => x.BotoSyncActivat );

            RxApp
                .MainThreadScheduler
                .Schedule(LoadData);

            GeneraPivotCommand = ReactiveCommand.CreateFromTask(DoGeneraPivot, ObservaBotoPivotActivat);
            GeneraSyncCommand = ReactiveCommand.CreateFromTask(DoGeneraSync, ObservaBotoSyncActivat);
        }

        private async void LoadData()
        {
            BrokenRules.Clear();

            using var blActuacioSet = SuperContext.GetBLOperation<IActuacioSet>();
            using var blCursAcademicSet = SuperContext.GetBLOperation<ICursAcademicSet>();
            var dtoCursActual = await blCursAcademicSet.GetCursActiu();

            var nTotalActuacions = await blActuacioSet.CountFromPredicate(new DTO.i.DTOs.ActuacioSearchParms());

            var nTotalActuacionsTxt = nTotalActuacions.Data?.ToString("N0");
            TotalActuacions = nTotalActuacionsTxt != null ? $"{nTotalActuacionsTxt} Actuacions" : "No hi ha actuacions";
            NumTotalActuacions = nTotalActuacions.Data;
        }

        public ObservableCollectionExtended<BrokenRule> BrokenRules = new();

        public bool _OperacionsDelicadesActivat;
        public bool OperacionsDelicadesActivat
        {
            get => _OperacionsDelicadesActivat;
            set => this.RaiseAndSetIfChanged(ref _OperacionsDelicadesActivat, value);                
        }

        #region Pivot
        // Botó pivot
        public bool _BotoPivotActivat;

        public bool BotoPivotActivat
        {
            get => _BotoPivotActivat;
            set => this.RaiseAndSetIfChanged(ref _BotoPivotActivat, value);
        }

        // Vars pivot
        private int? _NumTotalActuacions;
        public int? NumTotalActuacions
        {
            get => _NumTotalActuacions;
            set => this.RaiseAndSetIfChanged(ref _NumTotalActuacions, value);                
        }

        private string _TotalActuacions = string.Empty;
        public string TotalActuacions
        {
            get => _TotalActuacions;
            set => this.RaiseAndSetIfChanged(ref _TotalActuacions, value);
        }

        private string _ResultatPivotAlumne = string.Empty;
        public string ResultatPivotAlumne
        {
            get { return _ResultatPivotAlumne; }
            protected set { this.RaiseAndSetIfChanged(ref _ResultatPivotAlumne, value); }
        }

        // Generar Pivot

        public ReactiveCommand<Unit, Dtoo.SaveResult?> GeneraPivotCommand { get; }
        private async Task<Dtoo.SaveResult?> DoGeneraPivot()
        {
            ResultatPivotAlumne = "";
            using var bl = SuperContext.GetBLOperation<IPivotActuacions>();
            var resultat = await bl.Run();
            ResultatPivotAlumne =
                resultat.Data != null ?
                $"Fitxer desat a: {resultat.Data.FullPath}" :
                "Error generant fitxer: " + string.Join(" * ", resultat.BrokenRules.Select(x => x.Message));

            return resultat.Data;
            
        }

        #endregion

        #region Sync Actiu
        // Botó Sync
        public bool _BotoSyncActivat;

        public bool BotoSyncActivat
        {
            get => _BotoSyncActivat;
            set => this.RaiseAndSetIfChanged(ref _BotoSyncActivat, value);
        }

        // Vars Sync

        private string _ResultatSyncAlumne = string.Empty;
        public string ResultatSyncAlumne
        {
            get { return _ResultatSyncAlumne; }
            protected set { this.RaiseAndSetIfChanged(ref _ResultatSyncAlumne, value); }
        }

        // Generar Sync

        public ReactiveCommand<Unit, Dtoo.EtiquetaDescripcio?> GeneraSyncCommand { get; }
        private async Task<Dtoo.EtiquetaDescripcio?> DoGeneraSync()
        {
            ResultatSyncAlumne = "";
            using var bl = SuperContext.GetBLOperation<IAlumneSyncActiuByCentre>();
            var resultat = await bl.Run();
            ResultatSyncAlumne =
                resultat.Data != null ?
                resultat.Data.Etiqueta :
                "Error sincronitzant: " + string.Join(" * ", resultat.BrokenRules.Select(x => x.Message));

            return resultat.Data;            
        }

        #endregion



    }
}
