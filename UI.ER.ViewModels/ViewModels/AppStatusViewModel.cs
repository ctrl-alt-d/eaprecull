using ReactiveUI;
using System.Reactive.Linq;
using BusinessLayer.Abstract.Exceptions;
using System.Linq;
using DynamicData.Binding;
using UI.ER.ViewModels.Services;
using BusinessLayer.Abstract.Services;
using System.Reactive.Concurrency;
using System.Reactive;
using System.Windows.Input;
using CommonInterfaces;

namespace UI.ER.ViewModels.ViewModels
{
    public class AppStatusViewModel : ViewModelBase
    {
        private readonly string SPACE = " ";
        private readonly string NA = "N/A";
        public AppStatusViewModel()
        {
            RxApp.MainThreadScheduler.Schedule(LoadData);

            // Una comanda per cada entrada de navegació de la finestra principal: les tres
            // targetes del taulell i les set entrades del menú. La vista només hi enganxa
            // quina finestra atén cada Interaction; el que s'obre i què passa després es
            // decideix aquí.
            ActuacioSetCommand = ComandaDeNavegacio(ShowActuacioSetDialog);
            AlumneSetCommand = ComandaDeNavegacio(ShowAlumneSetDialog);
            CentreSetCommand = ComandaDeNavegacio(ShowCentreSetDialog);
            CursAcademicSetCommand = ComandaDeNavegacio(ShowCursAcademicSetDialog);
            EtapaSetCommand = ComandaDeNavegacio(ShowEtapaSetDialog);
            TipusActuacioSetCommand = ComandaDeNavegacio(ShowTipusActuacioSetDialog);
            UtilitatsCommand = ComandaDeNavegacio(ShowUtilitatsDialog);
        }

        /// <summary>
        /// Comanda que obre un diàleg i, en tancar-lo, refresca les xifres del taulell.
        /// Qualsevol de les set finestres pot haver canviat dades que hi surten.
        /// </summary>
        private ICommand ComandaDeNavegacio<TSortida>(Interaction<Unit, TSortida> dialeg)
            => ReactiveCommand.CreateFromTask(async () =>
            {
                await dialeg.Handle(Unit.Default);
                RxApp.MainThreadScheduler.Schedule(LoadData);
            });

        private async void LoadData()
        {


            BrokenRules.Clear();

            CursActual = NA;
            CursActualWarning = SPACE;
            TotalActuacions = NA;
            TotalActuacionsCursActual = NA;
            TotalALumnes = NA;
            TotalALumnesActualitzats = NA;



            using var blActuacioSet = SuperContext.Resolve<IActuacioSet>();
            using var blAlumneSet = SuperContext.Resolve<IAlumneSet>();
            using var blCursAcademicSet = SuperContext.Resolve<ICursAcademicSet>();

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
