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
    public class UtilitatsViewModel : ViewModelBase
    {
        public UtilitatsViewModel()
        {
            var ObservaHiHaActuacions =
                this
                .WhenAnyValue(x => (x.NumTotalActuacions ?? 0 ) > 0 );

            RxApp.MainThreadScheduler.Schedule(LoadData);
            PivotSetCommand = ReactiveCommand.CreateFromTask(ShowPivotSetDialogHandle, ObservaHiHaActuacions);
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

        public int? _NumTotalActuacions;
        public int? NumTotalActuacions
        {
            get => _NumTotalActuacions;
            set => this.RaiseAndSetIfChanged(ref _NumTotalActuacions, value);
        }

        public string _TotalActuacions = string.Empty;
        public string TotalActuacions
        {
            get => _TotalActuacions;
            set => this.RaiseAndSetIfChanged(ref _TotalActuacions, value);
        }

        // ---
        public ICommand PivotSetCommand { get; }
        public Interaction<Unit, IIdEtiquetaDescripcio?> ShowPivotSetDialog { get; } = new();
        private async Task ShowPivotSetDialogHandle()
        {
            var data = await ShowPivotSetDialog.Handle(Unit.Default);
        }

    }
}
