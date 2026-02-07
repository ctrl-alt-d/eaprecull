using System.Reactive;
using ReactiveUI;
using Dtoo = DTO.o.DTOs;
using CommonInterfaces;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Reactive.Linq;
using System.Collections.Generic;
using BusinessLayer.Abstract.Exceptions;
using System.Linq;
using DynamicData.Binding;
using UI.ER.AvaloniaUI.Services;
using BusinessLayer.Abstract.Services;

namespace UI.ER.ViewModels.ViewModels
{
    public class ActuacioRowViewModel : ViewModelBase, IEtiquetaDescripcio, IId
    {

        protected Dtoo.Actuacio Model { get; set; }
        public ActuacioRowViewModel(Dtoo.Actuacio data, bool modeLookup = false)
        {

            // Behavior Parm
            ModeLookup = modeLookup;
            Id = data.Id;
            Model = data;

            // State
            DTO2ModelView(data);

            // Behavior
            SeleccionarCommand = ReactiveCommand.Create(SelectRow);
            UpdateCommand = ReactiveCommand.CreateFromTask(ShowUpdateDialogHandle);
            VeureExpedientAlumneCommand = ReactiveCommand.CreateFromTask(ShowExpedientAlumneDialogHandle);
            EditarAlumneCommand = ReactiveCommand.CreateFromTask(ShowEditarAlumneDialogHandle);
        }


        public bool ModeLookup { get; }

        private string _Etiqueta = string.Empty;
        public string Etiqueta
        {
            get { return _Etiqueta; }
            protected set { this.RaiseAndSetIfChanged(ref _Etiqueta, value); }
        }

        private string _Descripcio = string.Empty;
        public string Descripcio
        {
            get { return _Descripcio; }
            protected set { this.RaiseAndSetIfChanged(ref _Descripcio, value); }
        }

        private string _CentreActuacio = string.Empty;
        public string CentreActuacio
        {
            get { return _CentreActuacio; }
            protected set { this.RaiseAndSetIfChanged(ref _CentreActuacio, value); }
        }

        private string _CursActuacio = string.Empty;
        public string CursActuacio
        {
            get { return _CursActuacio; }
            protected set { this.RaiseAndSetIfChanged(ref _CursActuacio, value); }
        }

        private string _DataTxt = string.Empty;
        public string DataTxt
        {
            get { return _DataTxt; }
            protected set { this.RaiseAndSetIfChanged(ref _DataTxt, value); }
        }

        private string _TipusActuacio = string.Empty;
        public string TipusActuacio
        {
            get { return _TipusActuacio; }
            protected set { this.RaiseAndSetIfChanged(ref _TipusActuacio, value); }
        }

        private string _DuradaTxt = string.Empty;
        public string DuradaTxt
        {
            get { return _DuradaTxt; }
            protected set { this.RaiseAndSetIfChanged(ref _DuradaTxt, value); }
        }

        private string _DescripcioActuacio = string.Empty;
        public string DescripcioActuacio
        {
            get { return _DescripcioActuacio; }
            protected set { this.RaiseAndSetIfChanged(ref _DescripcioActuacio, value); }
        }

        private string _AlumneNom = string.Empty;
        public string AlumneNom
        {
            get { return _AlumneNom; }
            protected set { this.RaiseAndSetIfChanged(ref _AlumneNom, value); }
        }

        public int Id { get; }

        private void DTO2ModelView(Dtoo.Actuacio? ActuacioDto)
        {
            if (ActuacioDto == null)
                return;

            Model = ActuacioDto;
            Etiqueta = ActuacioDto.Etiqueta;
            Descripcio = ActuacioDto.Descripcio;
            CentreActuacio = ActuacioDto.CentreAlMomentDeLactuacio.Etiqueta;
            CursActuacio = ActuacioDto.CursActuacio.Etiqueta;
            DataTxt = ActuacioDto.MomentDeLactuacio.ToString("dd/MM/yyyy");
            TipusActuacio = ActuacioDto.TipusActuacio.Etiqueta;
            DuradaTxt = ActuacioDto.MinutsDuradaActuacio > 0 ? $"{ActuacioDto.MinutsDuradaActuacio} min" : string.Empty;
            DescripcioActuacio = ActuacioDto.DescripcioActuacio ?? string.Empty;
            AlumneNom = ActuacioDto.Alumne.Etiqueta;
        }

        public ObservableCollectionExtended<string> BrokenRules { get; } = new();
        private void BrokenRules2ModelView(List<BrokenRule> brokenRules)
        {
            BrokenRules.Clear();
            BrokenRules.AddRange(brokenRules.Select(x => x.Message));
        }

        // --- Obrir Finestra Edició ---
        public ICommand UpdateCommand { get; }
        public Interaction<ActuacioUpdateViewModel, Dtoo.Actuacio?> ShowUpdateDialog { get; } = new();
        private async Task ShowUpdateDialogHandle()
        {
            var update = new ActuacioUpdateViewModel(Id);
            var data = await ShowUpdateDialog.Handle(update);
            if (data != null) DTO2ModelView(data);
        }

        // --- Seleccionar si estem en mode lookup ---
        public ReactiveCommand<Unit, Dtoo.Actuacio> SeleccionarCommand { get; }
        private Dtoo.Actuacio SelectRow() => Model;

        // --- Veure expedient de l'alumne ---
        public ICommand VeureExpedientAlumneCommand { get; }
        public Interaction<AlumneInformeViewerViewModel, Unit> ShowExpedientAlumneDialog { get; } = new();
        private async Task ShowExpedientAlumneDialogHandle()
        {
            var alumneId = Model.Alumne.Id;
            var vm = new AlumneInformeViewerViewModel(alumneId);
            await ShowExpedientAlumneDialog.Handle(vm);
        }

        // --- Editar dades de l'alumne ---
        public ICommand EditarAlumneCommand { get; }
        public Interaction<AlumneUpdateViewModel, Dtoo.Alumne?> ShowEditarAlumneDialog { get; } = new();
        private async Task ShowEditarAlumneDialogHandle()
        {
            var alumneId = Model.Alumne.Id;
            var vm = new AlumneUpdateViewModel(alumneId);
            await ShowEditarAlumneDialog.Handle(vm);
        }

    }
}
