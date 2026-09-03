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
    public class CursAcademicRowViewModel : ViewModelBase, IRowViewModel<CursAcademicUpdateViewModel, Dtoo.CursAcademic, Dtoo.CursAcademic>, IEtiquetaDescripcio, IId
    {

        protected Dtoo.CursAcademic Model { get; set; }
        protected ObservableCollectionExtended<CursAcademicRowViewModel> TotsElsCursos { get; }
        private readonly IServiceFactory _serveis;

        public CursAcademicRowViewModel(
            IServiceFactory serveis,
            Dtoo.CursAcademic CursAcademicDto,
            ObservableCollectionExtended<CursAcademicRowViewModel> totsElsCursos,
            bool modeLookup = false)
        {
            _serveis = serveis;

            // Behavior Parm
            ModeLookup = modeLookup;
            TotsElsCursos = totsElsCursos;

            // State
            Id = CursAcademicDto.Id;
            Model = CursAcademicDto;
            Actualitza(CursAcademicDto);

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
            internal set { this.RaiseAndSetIfChanged(ref _Estat, value); }
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
            internal set { this.RaiseAndSetIfChanged(ref _EsActiu, value); }
        }

        private int _NombreActuacions;
        public int NombreActuacions
        {
            get { return _NombreActuacions; }
            protected set
            {
                this.RaiseAndSetIfChanged(ref _NombreActuacions, value);
                this.RaisePropertyChanged(nameof(NumActuacionsTxt));
            }
        }

        public string NumActuacionsTxt => $"{NombreActuacions} actuacions";

        public int Id { get; }

        /// <summary>
        /// Les entitats que la fila pinta. Es recalculen a cada <see cref="Actualitza"/>:
        /// una fila que canvia de centre canvia de referències.
        /// </summary>
        public IReadOnlySet<Referencia> ReferenciesPintades { get; private set; } = new HashSet<Referencia>();

        public void Actualitza(Dtoo.CursAcademic? data)
        {
            if (data == null)
                return;

            Model = data;
            ReferenciesPintades = Referencies.De(data);
            Etiqueta = data.Etiqueta;
            Descripcio = data.Descripcio;
            Estat = data.EsActiu ? "Activat" : "Desactivat";
            EsActiu = data.EsActiu;
            NombreActuacions = data.NombreActuacions;
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
            using var bl = _serveis.GetBLOperation<ICursAcademicActivaDesactiva>();
            var dto = await bl.Toggle(Id);
            Actualitza(dto.Data);
            BrokenRules2ModelView(dto.BrokenRules);

            if (dto.Data == null)
                return;

            TotsElsCursos
                .Where(x => x.Id != dto.Data.Id)
                .ToList()
                .ForEach(x => x.EsActiu = false);
        }

        // --- Obrir Finestra Edició ---
        public ICommand UpdateCommand { get; }
        public Interaction<CursAcademicUpdateViewModel, Dtoo.CursAcademic?> ShowUpdateDialog { get; } = new();
        private async Task ShowUpdateDialogHandle()
        {
            var update = new CursAcademicUpdateViewModel(_serveis, Id);
            var data = await ShowUpdateDialog.Handle(update);
            if (data != null) Actualitza(data);
        }

        // --- Seleccionar si estem en mode lookup ---
        public ReactiveCommand<Unit, Dtoo.CursAcademic> SeleccionarCommand { get; }
        private Dtoo.CursAcademic SelectRow() => Model;

    }
}
