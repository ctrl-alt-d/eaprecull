﻿using ReactiveUI;
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
using dtoo = DTO.o.DTOs;

namespace UI.ER.ViewModels.ViewModels
{
    public class UtilitatsViewModel : ViewModelBase
    {
        public UtilitatsViewModel()
        {
            var ObservaHiHaActuacions =
                this
                .WhenAnyValue(x => x.BotoPivotActivat );

            RxApp.MainThreadScheduler.Schedule(LoadData);
            GeneraPivotCommand = ReactiveCommand.CreateFromTask(DoGeneraPivot);
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

        public bool _BotoPivotActivat;

        public bool BotoPivotActivat
        {
            get => _BotoPivotActivat;
            set => this.RaiseAndSetIfChanged(ref _BotoPivotActivat, value);
        }

        public int? _NumTotalActuacions;
        public int? NumTotalActuacions
        {
            get => _NumTotalActuacions;
            set 
            {
                this.RaiseAndSetIfChanged(ref _NumTotalActuacions, value);
                BotoPivotActivat = (_NumTotalActuacions ?? 0) > 0;
            }
        }

        public string _TotalActuacions = string.Empty;
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

        public ReactiveCommand<Unit, dtoo.SaveResult?> GeneraPivotCommand { get; }
        private async Task<dtoo.SaveResult?> DoGeneraPivot()
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


    }
}
