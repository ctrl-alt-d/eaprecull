using System.Reactive;
using ReactiveUI;
using dtoo = DTO.o.DTOs;
using CommonInterfaces;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Reactive.Linq;
using System.Collections.Generic;
using BusinessLayer.Abstract.Exceptions;
using System.Linq;
using DynamicData.Binding;

namespace UI.ER.ViewModels.ViewModels
{
    public class ActuacioRowViewModel : ViewModelBase, IEtiquetaDescripcio, IId
    {

        protected dtoo.Actuacio Model { get; set;}
        protected readonly dtoo.CursAcademic? CursActual;
        public ActuacioRowViewModel(dtoo.Actuacio data, dtoo.CursAcademic? cursActual, bool modeLookup = false)
        {

            // Behavior Parm
            ModeLookup = modeLookup;
            Id = data.Id;
            Model = data;
            CursActual = cursActual;

            // State
            DTO2ModelView(data);

            // Behavior
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

        public int Id { get; }

        private void DTO2ModelView(dtoo.Actuacio? ActuacioDto)
        {
            if (ActuacioDto == null)
                return;

            Model = ActuacioDto;
            Etiqueta = ActuacioDto.Etiqueta;
            Descripcio = ActuacioDto.Descripcio;
            CentreActuacio = ActuacioDto.CentreAlMomentDeLactuacio.Etiqueta;
            CursActuacio = ActuacioDto.CursActuacio.Etiqueta;
        }

        public ObservableCollectionExtended<string> BrokenRules { get; } = new();
        private void BrokenRules2ModelView(List<BrokenRule> brokenRules)
        {
            BrokenRules.Clear();
            BrokenRules.AddRange(brokenRules.Select(x => x.Message));
        }

        // --- Obrir Finestra Edició ---
        public ICommand UpdateCommand { get; }
        public Interaction<ActuacioUpdateViewModel, dtoo.Actuacio?> ShowUpdateDialog { get; } = new();
        private async Task ShowUpdateDialogHandle()
        {
            var update = new ActuacioUpdateViewModel(Id);
            var data = await ShowUpdateDialog.Handle(update);
            if (data != null) DTO2ModelView(data);
        }

        // --- Seleccionar si estem en mode lookup ---
        public ReactiveCommand<Unit, dtoo.Actuacio> SeleccionarCommand { get; }
        private dtoo.Actuacio SelectRow() => Model;

    }
}
