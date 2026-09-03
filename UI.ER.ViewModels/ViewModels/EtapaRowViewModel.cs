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
    public class EtapaRowViewModel : ViewModelBase, IRowViewModel<EtapaUpdateViewModel, Dtoo.Etapa, Dtoo.Etapa>, IEtiquetaDescripcio, IId
    {

        protected Dtoo.Etapa Model { get; set; }
        private readonly IServiceFactory _serveis;

        public EtapaRowViewModel(IServiceFactory serveis, Dtoo.Etapa EtapaDto, bool modeLookup = false)
        {

            _serveis = serveis;

            // Behavior Parm
            ModeLookup = modeLookup;

            // State
            Id = EtapaDto.Id;
            Model = EtapaDto;
            Actualitza(EtapaDto);

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

        /// <summary>
        /// Les entitats que la fila pinta. Es recalculen a cada <see cref="Actualitza"/>:
        /// una fila que canvia de centre canvia de referències.
        /// </summary>
        public IReadOnlySet<Referencia> ReferenciesPintades { get; private set; } = new HashSet<Referencia>();

        public void Actualitza(Dtoo.Etapa? data)
        {
            if (data == null)
                return;

            Model = data;
            ReferenciesPintades = Referencies.De(data);
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
            using var bl = _serveis.GetBLOperation<IEtapaActivaDesactiva>();
            var dto = await bl.Toggle(Id);
            Actualitza(dto.Data);
            BrokenRules2ModelView(dto.BrokenRules);
        }

        // --- Obrir Finestra Edició ---
        public ICommand UpdateCommand { get; }
        public Interaction<EtapaUpdateViewModel, Dtoo.Etapa?> ShowUpdateDialog { get; } = new();
        private async Task ShowUpdateDialogHandle()
        {
            var update = new EtapaUpdateViewModel(_serveis, Id);
            var data = await ShowUpdateDialog.Handle(update);
            if (data != null) Actualitza(data);
        }

        // --- Seleccionar si estem en mode lookup ---
        public ReactiveCommand<Unit, Dtoo.Etapa> SeleccionarCommand { get; }
        private Dtoo.Etapa SelectRow() => Model;

    }
}
