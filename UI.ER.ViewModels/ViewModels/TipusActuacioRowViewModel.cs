using System.Reactive;
using ReactiveUI;
using dtoo = DTO.o.DTOs;
using CommonInterfaces;
using System.Threading.Tasks;
using UI.ER.AvaloniaUI.Services;
using BusinessLayer.Abstract.Services;
using System.Windows.Input;
using System.Reactive.Linq;
using System.Collections.Generic;
using BusinessLayer.Abstract.Exceptions;
using UI.ER.ViewModels.Common;
using System.Linq;

namespace UI.ER.ViewModels.ViewModels
{
    public class TipusActuacioRowViewModel : ViewModelBase, IEtiquetaDescripcio, IId
    {

        protected dtoo.TipusActuacio Model { get; }
        public TipusActuacioRowViewModel(dtoo.TipusActuacio TipusActuacioDto, bool modeLookup = false)
        {

            // Behavior Parm
            ModeLookup = modeLookup;

            // State
            Model = TipusActuacioDto;
            _Etiqueta = TipusActuacioDto.Etiqueta;
            _Descripcio = TipusActuacioDto.Descripcio;
            _Estat = TipusActuacioDto.EsActiu ? "Activat" : "Desactivat";
            _EsActiu = TipusActuacioDto.EsActiu;
            Id = TipusActuacioDto.Id;

            // Behavior
            DoActiuToggleCommand = ReactiveCommand.CreateFromTask(RunActiuToggle);
            SeleccionarCommand = ReactiveCommand.Create(SelectRow);
            UpdateCommand = ReactiveCommand.CreateFromTask(ShowUpdateDialogHandle);
        }


        public bool ModeLookup { get; }

        private string _Etiqueta = string.Empty;
        public string Etiqueta
        {
            get { return _Etiqueta; }
            protected set { this.RaiseAndSetIfChanged(ref _Etiqueta, value); }
        }

        private string _Estat = string.Empty;
        public string Estat
        {
            get { return _Estat; }
            protected set { this.RaiseAndSetIfChanged(ref _Estat, value); }
        }

        private string _Descripcio = string.Empty;
        public string Descripcio
        {
            get { return _Descripcio; }
            protected set { this.RaiseAndSetIfChanged(ref _Descripcio, value); }
        }

        private bool _EsActiu;
        public bool EsActiu
        {
            get { return _EsActiu; }
            protected set { this.RaiseAndSetIfChanged(ref _EsActiu, value); }
        }

        public int Id { get; }

        private void DTO2ModelView(dtoo.TipusActuacio? data)
        {
            if (data == null)
                return;

            Etiqueta = data.Etiqueta;
            Descripcio = data.Descripcio;
            Estat = data.EsActiu ? "Activat" : "Desactivat";
            EsActiu = data.EsActiu;
        }
        public RangeObservableCollection<string> BrokenRules { get; } = new();
        private void BrokenRules2ModelView(List<BrokenRule> brokenRules)
        {
            BrokenRules.ClearSilently();
            BrokenRules.AddRange(brokenRules.Select(x => x.Message));
        }

        // --- Activar / Desactivar ---
        public ReactiveCommand<Unit, Unit> DoActiuToggleCommand { get; }
        protected async Task RunActiuToggle()
        {
            using var bl = SuperContext.GetBLOperation<ITipusActuacioActivaDesactiva>();
            var dto = await bl.Toggle(Id);
            DTO2ModelView(dto.Data);
            BrokenRules2ModelView(dto.BrokenRules);
        }

        // --- Obrir Finestra Edició ---
        public ICommand UpdateCommand { get; }
        public Interaction<TipusActuacioUpdateViewModel, dtoo.TipusActuacio?> ShowUpdateDialog { get; } = new();
        private async Task ShowUpdateDialogHandle()
        {
            var update = new TipusActuacioUpdateViewModel(Id);
            var data = await ShowUpdateDialog.Handle(update);
            if (data != null) DTO2ModelView(data);
        }

        // --- Seleccionar si estem en mode lookup ---
        public ReactiveCommand<Unit, dtoo.TipusActuacio> SeleccionarCommand { get; }
        private dtoo.TipusActuacio SelectRow() => Model;

    }
}
