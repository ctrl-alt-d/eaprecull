using ReactiveUI;
using System.Reactive.Linq;
using BusinessLayer.Abstract.Exceptions;
using DynamicData.Binding;
using BusinessLayer.Abstract.Generic;
using BusinessLayer.Abstract.Services;
using System.Reactive.Concurrency;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Windows.Input;
using CommonInterfaces;
using System;
using UI.ER.ViewModels.Services;

namespace UI.ER.ViewModels.ViewModels
{
    public class AppStatusViewModel : ViewModelBase
    {
        private readonly string SPACE = " ";
        private readonly string NA = "N/A";
        private readonly IServiceFactory _serveis;

        public AppStatusViewModel(IServiceFactory serveis)
        {
            _serveis = serveis;

            RxApp.MainThreadScheduler.Schedule(LoadData);

            // Les xifres del taulell les fa obsoletes qualsevol escriptura, vingui de la
            // finestra que vingui. En comptes de rellegir-les en tancar cada diàleg,
            // s'escolta el bus de canvis: aquí sí que interessa tot el que passi.
            this.WhenActivated(d =>
                _serveis.Canvis.ComObservable()
                    .Throttle(TimeSpan.FromMilliseconds(300))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => LoadData())
                    .DisposeWith(d));

            // Una comanda per cada entrada de navegació de la finestra principal: les tres
            // targetes del taulell i les set entrades del menú. La vista només hi enganxa
            // quina finestra atén cada Interaction; el que s'obre es decideix aquí.
            ActuacioSetCommand = ReactiveCommand.CreateFromObservable(() => ShowActuacioSetDialog.Handle(Unit.Default));
            AlumneSetCommand = ReactiveCommand.CreateFromObservable(() => ShowAlumneSetDialog.Handle(Unit.Default));
            CentreSetCommand = ReactiveCommand.CreateFromObservable(() => ShowCentreSetDialog.Handle(Unit.Default));
            CursAcademicSetCommand = ReactiveCommand.CreateFromObservable(() => ShowCursAcademicSetDialog.Handle(Unit.Default));
            EtapaSetCommand = ReactiveCommand.CreateFromObservable(() => ShowEtapaSetDialog.Handle(Unit.Default));
            TipusActuacioSetCommand = ReactiveCommand.CreateFromObservable(() => ShowTipusActuacioSetDialog.Handle(Unit.Default));
            UtilitatsCommand = ReactiveCommand.CreateFromObservable(() => ShowUtilitatsDialog.Handle(Unit.Default));
        }

        private async void LoadData()
        {


            BrokenRules.Clear();

            CursActual = NA;
            CursActualWarning = SPACE;
            TotalActuacions = NA;
            TotalActuacionsCursActual = NA;
            TotalALumnes = NA;
            TotalALumnesActualitzats = NA;



            using var blActuacioSet = _serveis.GetBLOperation<IActuacioSet>();
            using var blAlumneSet = _serveis.GetBLOperation<IAlumneSet>();
            using var blCursAcademicSet = _serveis.GetBLOperation<ICursAcademicSet>();

            var dtoCursActual = await blCursAcademicSet.GetCursActiu();

            var dtoCursCorrecte = await blCursAcademicSet.ElCursPerDefecteEsCorresponAmbLaDataActual();
            if (!(dtoCursCorrecte ?? false))
            {
                CursActualWarning = "Revisa el curs Actiu!";
            }

            var cursActual = dtoCursActual.Data;
            CursActual = cursActual?.Nom ?? NA;
            var nTotalActuacions = await blActuacioSet.CountFromPredicate(new DTO.i.DTOs.ActuacioSearchParms());
            var nTotalActuacionsCursActual = await blActuacioSet.CountFromPredicate(new DTO.i.DTOs.ActuacioSearchParms(cursActuacioId: cursActual?.Id));
            var nAlumnes = await blAlumneSet.CountFromPredicate(new DTO.i.DTOs.AlumneSearchParms());
            var nAlumnesActualitzats = await blAlumneSet.CountFromPredicate(new DTO.i.DTOs.AlumneSearchParms(cursDarreraActualitacioDadesId: cursActual?.Id));

            var nTotalActuacionsTxt = nTotalActuacions.Data?.ToString("N0");
            TotalActuacions = nTotalActuacionsTxt != null ? $"{nTotalActuacionsTxt} Actuacions" : NA;

            var nTotalActuacionsCursActualTxt = nTotalActuacionsCursActual.Data?.ToString("N0");
            TotalActuacionsCursActual =
                nTotalActuacionsCursActual != null && cursActual != null ?
                $"Del curs actiu: {nTotalActuacionsCursActualTxt}" :
                SPACE;

            var nAlumnesTxt = nAlumnes.Data?.ToString("N0");
            TotalALumnes = nAlumnesTxt != null ? $"{nAlumnesTxt} Alumnes" : NA;

            var nAlumnesActualitzatsTxt = nAlumnesActualitzats.Data?.ToString("N0");
            TotalALumnesActualitzats = nAlumnesActualitzatsTxt != null ? $"Amb dades actualitzades: {nAlumnesActualitzatsTxt}" : NA;

        }

        public ObservableCollectionExtended<BrokenRule> BrokenRules = new();

        private string _CursActual = string.Empty;
        public string CursActual
        {
            get => _CursActual;
            set => this.RaiseAndSetIfChanged(ref _CursActual, value);
        }

        private string _CursActualWarning = string.Empty;
        public string CursActualWarning
        {
            get => _CursActualWarning;
            set => this.RaiseAndSetIfChanged(ref _CursActualWarning, value);
        }

        private string _TotalActuacions = string.Empty;
        public string TotalActuacions
        {
            get => _TotalActuacions;
            set => this.RaiseAndSetIfChanged(ref _TotalActuacions, value);
        }

        private string _TotalActuacionsCursActual = string.Empty;
        public string TotalActuacionsCursActual
        {
            get => _TotalActuacionsCursActual;
            set => this.RaiseAndSetIfChanged(ref _TotalActuacionsCursActual, value);
        }


        private string _TotalALumnes = string.Empty;
        public string TotalALumnes
        {
            get => _TotalALumnes;
            set => this.RaiseAndSetIfChanged(ref _TotalALumnes, value);
        }

        private string _TotalALumnesActualitzats = string.Empty;
        public string TotalALumnesActualitzats
        {
            get => _TotalALumnesActualitzats;
            set => this.RaiseAndSetIfChanged(ref _TotalALumnesActualitzats, value);
        }

        // --- Navegació -------------------------------------------------------------
        //
        // Cada parella {Entitat}SetCommand / Show{Entitat}SetDialog és un punt d'entrada
        // de la finestra principal. Les llistes retornen IIdEtiquetaDescripcio? perquè les
        // mateixes finestres fan de lookup; obertes des del menú el resultat és null i
        // s'ignora.

        public ICommand ActuacioSetCommand { get; }
        public Interaction<Unit, IIdEtiquetaDescripcio?> ShowActuacioSetDialog { get; } = new();

        public ICommand AlumneSetCommand { get; }
        public Interaction<Unit, IIdEtiquetaDescripcio?> ShowAlumneSetDialog { get; } = new();

        public ICommand CentreSetCommand { get; }
        public Interaction<Unit, IIdEtiquetaDescripcio?> ShowCentreSetDialog { get; } = new();

        public ICommand CursAcademicSetCommand { get; }
        public Interaction<Unit, IIdEtiquetaDescripcio?> ShowCursAcademicSetDialog { get; } = new();

        public ICommand EtapaSetCommand { get; }
        public Interaction<Unit, IIdEtiquetaDescripcio?> ShowEtapaSetDialog { get; } = new();

        public ICommand TipusActuacioSetCommand { get; }
        public Interaction<Unit, IIdEtiquetaDescripcio?> ShowTipusActuacioSetDialog { get; } = new();

        /// <summary>Utilitats no és una llista d'entitats: només s'obre i es tanca.</summary>
        public ICommand UtilitatsCommand { get; }
        public Interaction<Unit, Unit> ShowUtilitatsDialog { get; } = new();
    }
}
