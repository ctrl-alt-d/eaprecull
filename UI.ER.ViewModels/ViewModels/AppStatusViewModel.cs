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

namespace UI.ER.ViewModels.ViewModels
{
    public class AppStatusViewModel : ViewModelBase
    {
        private readonly string SPACE = " ";
        private readonly string NA = "N/A";
        public AppStatusViewModel()
        {
            RxApp.MainThreadScheduler.Schedule(LoadData);
            ActuacioSetCommand = ReactiveCommand.CreateFromTask(ShowActuacioSetDialogHandle);
            AlumneSetCommand = ReactiveCommand.CreateFromTask(ShowAlumneSetDialogHandle);
            CursAcademicSetCommand = ReactiveCommand.CreateFromTask(ShowCursAcademicSetDialogHandle);

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



            using var blActuacioSet = SuperContext.GetBLOperation<IActuacioSet>();
            using var blAlumneSet = SuperContext.GetBLOperation<IAlumneSet>();
            using var blCursAcademicSet = SuperContext.GetBLOperation<ICursAcademicSet>();

            var dtoCursActual = await blCursAcademicSet.GetCursActiu();

            var dtoCursCorrecte = await blCursAcademicSet.ElCursPerDefecteEsCorresponAmbLaDataActual();
            if (! (dtoCursCorrecte ?? false))
            {
                CursActualWarning = "Revisa el curs activat!";
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
                $"({nTotalActuacionsCursActualTxt} durant el curs {cursActual?.Nom})" : 
                SPACE;

            var nAlumnesTxt = nAlumnes.Data?.ToString("N0");
            TotalALumnes = nAlumnesTxt != null ? $"{nAlumnesTxt} Alumnes" : NA;

            var nAlumnesActualitzatsTxt = nAlumnesActualitzats.Data?.ToString("N0");
            TotalALumnesActualitzats = nAlumnesActualitzatsTxt != null ? $"({nAlumnesActualitzatsTxt} actualitzats)" : NA;

        }

        public ObservableCollectionExtended<BrokenRule> BrokenRules = new();

        public string _CursActual = string.Empty;
        public string CursActual
        {
            get => _CursActual;
            set => this.RaiseAndSetIfChanged(ref _CursActual, value);
        }

        public string _CursActualWarning = string.Empty;
        public string CursActualWarning
        {
            get => _CursActualWarning;
            set => this.RaiseAndSetIfChanged(ref _CursActualWarning, value);
        }

        public string _TotalActuacions = string.Empty;
        public string TotalActuacions
        {
            get => _TotalActuacions;
            set => this.RaiseAndSetIfChanged(ref _TotalActuacions, value);
        }

        public string _TotalActuacionsCursActual = string.Empty;
        public string TotalActuacionsCursActual
        {
            get => _TotalActuacionsCursActual;
            set => this.RaiseAndSetIfChanged(ref _TotalActuacionsCursActual, value);
        }


        public string _TotalALumnes = string.Empty;
        public string TotalALumnes
        {
            get => _TotalALumnes;
            set => this.RaiseAndSetIfChanged(ref _TotalALumnes, value);
        }

        public string _TotalALumnesActualitzats = string.Empty;
        public string TotalALumnesActualitzats
        {
            get => _TotalALumnesActualitzats;
            set => this.RaiseAndSetIfChanged(ref _TotalALumnesActualitzats, value);
        }

        // ---
        public ICommand ActuacioSetCommand { get; }
        public Interaction<Unit, IIdEtiquetaDescripcio?> ShowActuacioSetDialog { get; } = new();
        private async Task ShowActuacioSetDialogHandle()
        {
            var data = await ShowActuacioSetDialog.Handle(Unit.Default);
            RxApp.MainThreadScheduler.Schedule(LoadData);
        }

        // ---
        public ICommand AlumneSetCommand { get; }
        public Interaction<Unit, IIdEtiquetaDescripcio?> ShowAlumneSetDialog { get; } = new();
        private async Task ShowAlumneSetDialogHandle()
        {
            var data = await ShowAlumneSetDialog.Handle(Unit.Default);
            RxApp.MainThreadScheduler.Schedule(LoadData);
        }


        // ---
        public ICommand CursAcademicSetCommand { get; }
        public Interaction<Unit, IIdEtiquetaDescripcio?> ShowCursAcademicSetDialog { get; } = new();
        private async Task ShowCursAcademicSetDialogHandle()
        {
            var data = await ShowCursAcademicSetDialog.Handle(Unit.Default);
            RxApp.MainThreadScheduler.Schedule(LoadData);
        }


    }
}
