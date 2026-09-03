using System.Reactive;
using ReactiveUI;
using Dtoo = DTO.o.DTOs;
using CommonInterfaces;
using System.Threading.Tasks;
using BusinessLayer.Abstract.Services;
using System.Windows.Input;
using System.Reactive.Linq;
using System.Collections.Generic;
using BusinessLayer.Abstract.Exceptions;
using System.Linq;
using DynamicData.Binding;
using BusinessLayer.Abstract.Generic;

namespace UI.ER.ViewModels.ViewModels
{
    public class CentreRowViewModel : ViewModelBase, IRowViewModel<CentreUpdateViewModel, Dtoo.Centre, Dtoo.Centre>, IEtiquetaDescripcio, IId
    {

        protected Dtoo.Centre Model { get; }
        private readonly IServiceFactory _serveis;

        public CentreRowViewModel(IServiceFactory serveis, Dtoo.Centre centreDto, bool modeLookup = false)
            : this(serveis, centreDto, modeLookup, 0, 0)
        {
        }

        public CentreRowViewModel(IServiceFactory serveis, Dtoo.CentreAmbActuacions centreDto, bool modeLookup = false)
            : this(serveis, centreDto, modeLookup, centreDto.TotalActuacions, centreDto.ActuacionsCursActiu)
        {
        }

        private CentreRowViewModel(IServiceFactory serveis, Dtoo.Centre centreDto, bool modeLookup, int totalActuacions, int actuacionsCursActiu)
        {

            _serveis = serveis;

            // Behavior Parm
            ModeLookup = modeLookup;

            // State
            Model = centreDto;
            _Etiqueta = centreDto.Etiqueta;
            _Descripcio = centreDto.Descripcio;
            _Estat = centreDto.EsActiu ? "Activat" : "Desactivat";
            _EsActiu = centreDto.EsActiu;
            _TotalActuacions = totalActuacions;
            _ActuacionsCursActiu = actuacionsCursActiu;
            Id = centreDto.Id;

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

        private int _TotalActuacions;
        public int TotalActuacions
        {
            get { return _TotalActuacions; }
            protected set { this.RaiseAndSetIfChanged(ref _TotalActuacions, value); }
        }

        private int _ActuacionsCursActiu;
        public int ActuacionsCursActiu
        {
            get { return _ActuacionsCursActiu; }
            protected set { this.RaiseAndSetIfChanged(ref _ActuacionsCursActiu, value); }
        }

        public string ActuacionsTxt => $"{ActuacionsCursActiu} actuacions (curs actiu) / {TotalActuacions} total";

        public int Id { get; }

        private void DTO2ModelView(Dtoo.Centre? data)
        {
            if (data == null)
                return;

            Etiqueta = data.Etiqueta;
            Descripcio = data.Descripcio;
            Estat = data.EsActiu ? "Activat" : "Desactivat";
            EsActiu = data.EsActiu;
        }
        public ObservableCollectionExtended<string> BrokenRules { get; } = new();
        private void BrokenRules2ModelView(List<BrokenRule> brokenRules)
        {
            BrokenRules.Clear();
            BrokenRules.AddRange(brokenRules.Select(x => x.Message));
        }

        // --- Activar / Desactivar ---
        public ReactiveCommand<Unit, Unit> DoActiuToggleCommand { get; }
        protected async Task RunActiuToggle()
        {
            using var bl = _serveis.GetBLOperation<ICentreActivaDesactiva>();
            var dto = await bl.Toggle(Id);
            DTO2ModelView(dto.Data);
            BrokenRules2ModelView(dto.BrokenRules);
        }

        // --- Obrir Finestra Edició ---
        public ICommand UpdateCommand { get; }
        public Interaction<CentreUpdateViewModel, Dtoo.Centre?> ShowUpdateDialog { get; } = new();
        private async Task ShowUpdateDialogHandle()
        {
            var update = new CentreUpdateViewModel(_serveis, Id);
            var data = await ShowUpdateDialog.Handle(update);
            if (data != null) DTO2ModelView(data);
        }

        // --- Seleccionar si estem en mode lookup ---
        public ReactiveCommand<Unit, Dtoo.Centre> SeleccionarCommand { get; }
        private Dtoo.Centre SelectRow() => Model;

    }
}
