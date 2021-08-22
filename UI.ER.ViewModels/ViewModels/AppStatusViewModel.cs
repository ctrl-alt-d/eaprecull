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

        public AppStatusViewModel()
        {
            RxApp.MainThreadScheduler.Schedule(LoadData);
            ActuacioSetCommand = ReactiveCommand.CreateFromTask(ShowActuacioSetDialogHandle);
            AlumneSetCommand = ReactiveCommand.CreateFromTask(ShowAlumneSetDialogHandle);

        }

        private async void LoadData()
        {
            BrokenRules.Clear();

            CursActual = "N/A";
            TotalActuacions = "N/A";
            TotalActuacionsCursActual = "N/A";
            TotalALumnes = "N/A";
            TotalALumnesActualitzats = "N/A";



            using var blActuacioSet = SuperContext.GetBLOperation<IActuacioSet>();
            using var blAlumneSet = SuperContext.GetBLOperation<IAlumneSet>();
            using var blCursAcademicSet = SuperContext.GetBLOperation<ICursAcademicSet>();

            var dtoCursActual = await blCursAcademicSet.FromPredicate(new DTO.i.DTOs.EsActiuParms(true));
            if (dtoCursActual.Data == null || !dtoCursActual.Data.Any())
            {

                CursActual = "No hi ha dades";
                BrokenRules.AddRange(dtoCursActual.BrokenRules);
            };

            var cursActual = dtoCursActual.Data?.FirstOrDefault();

            var nTotalActuacions = await blActuacioSet.CountFromPredicate(new DTO.i.DTOs.ActuacioSearchParms());
            var nTotalActuacionsCursActual = await blActuacioSet.CountFromPredicate(new DTO.i.DTOs.ActuacioSearchParms(cursActuacioId: cursActual?.Id));
            var nAlumnes = await blAlumneSet.CountFromPredicate(new DTO.i.DTOs.AlumneSearchParms());
            var nAlumnesActualitzats = await blAlumneSet.CountFromPredicate(new DTO.i.DTOs.AlumneSearchParms(cursDarreraActualitacioDadesId: cursActual?.Id));

            var nTotalActuacionsTxt = nTotalActuacions.Data?.ToString("N0");
            TotalActuacions = nTotalActuacionsTxt != null ? $"{nTotalActuacionsTxt} Actuacions" : "0 Actuacions=";

            var nTotalActuacionsCursActualTxt = nTotalActuacions.Data?.ToString("N0");
            TotalActuacionsCursActual = 
                nTotalActuacionsCursActual != null && cursActual != null ? 
                $"({nTotalActuacionsCursActualTxt} durant el curs {cursActual?.Nom})" : 
                "";

            var nAlumnesTxt = nAlumnes.Data?.ToString("N0");
            TotalALumnes = nAlumnesTxt != null ? $"{nAlumnesTxt} Alumnes" : "N/A Alumnes :(";

            var nAlumnesActualitzatsTxt = nAlumnesActualitzats.Data?.ToString("N0");
            TotalALumnesActualitzats = nAlumnesActualitzatsTxt != null ? $"({nAlumnesActualitzatsTxt} actualitzats)" : "N/A";

        }

        public ObservableCollectionExtended<BrokenRule> BrokenRules = new();

        public string _CursActual = string.Empty;
        public string CursActual
        {
            get => _CursActual;
            set => this.RaiseAndSetIfChanged(ref _CursActual, value);
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


    }
}
