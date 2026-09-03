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
    public class AlumneRowViewModel : ViewModelBase, IRowViewModel<AlumneUpdateViewModel, Dtoo.Alumne, Dtoo.Alumne>, IEtiquetaDescripcio, IId
    {

        protected Dtoo.Alumne Model { get; set; }
        protected readonly Dtoo.CursAcademic? CursActual;
        private readonly IServiceFactory _serveis;

        public AlumneRowViewModel(IServiceFactory serveis, Dtoo.Alumne data, Dtoo.CursAcademic? cursActual, bool modeLookup = false)
        {

            _serveis = serveis;

            // Behavior Parm
            ModeLookup = modeLookup;
            Id = data.Id;
            Model = data;
            CursActual = cursActual;

            // State
            Actualitza(data);

            // Behavior
            DoActiuToggleCommand = ReactiveCommand.CreateFromTask(RunActiuToggle);
            SeleccionarCommand = ReactiveCommand.Create(SelectRow);
            UpdateCommand = ReactiveCommand.CreateFromTask(ShowUpdateDialogHandle);
            ActuacioSetCommand = ReactiveCommand.CreateFromTask(ShowActuacioSetDialogHandle);
            GeneraInformeCommand = ReactiveCommand.CreateFromTask(DoGeneraInforme);
            VeureInformeCommand = ReactiveCommand.CreateFromTask(ShowInformeViewerDialogHandle);

        }


        public bool ModeLookup { get; }

        private string _Etiqueta = string.Empty;
        public string Etiqueta
        {
            get { return _Etiqueta; }
            protected set { this.RaiseAndSetIfChanged(ref _Etiqueta, value); }
        }

        private bool _Desactualitzat;
        public bool Desactualitzat
        {
            get { return _Desactualitzat; }
            protected set { this.RaiseAndSetIfChanged(ref _Desactualitzat, value); }
        }

        private string _Descripcio = string.Empty;
        public string Descripcio
        {
            get { return _Descripcio; }
            protected set { this.RaiseAndSetIfChanged(ref _Descripcio, value); }
        }

        private string _CentreActual = string.Empty;
        public string CentreActual
        {
            get { return _CentreActual; }
            protected set { this.RaiseAndSetIfChanged(ref _CentreActual, value); }
        }

        private string _CursDarreraActualitzacio = string.Empty;
        public string CursDarreraActualitzacio
        {
            get { return _CursDarreraActualitzacio; }
            protected set { this.RaiseAndSetIfChanged(ref _CursDarreraActualitzacio, value); }
        }

        private string _ResultatInformeAlumne = string.Empty;
        public string ResultatInformeAlumne
        {
            get { return _ResultatInformeAlumne; }
            protected set { this.RaiseAndSetIfChanged(ref _ResultatInformeAlumne, value); }
        }

        private string _NumActuacionsTxt = string.Empty;
        public string NumActuacionsTxt
        {
            get { return _NumActuacionsTxt; }
            protected set { this.RaiseAndSetIfChanged(ref _NumActuacionsTxt, value); }
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

        public void Actualitza(Dtoo.Alumne? AlumneDto)
        {
            if (AlumneDto == null)
                return;

            Model = AlumneDto;
            ReferenciesPintades = Referencies.De(AlumneDto);
            Etiqueta = AlumneDto.Etiqueta;
            Descripcio = AlumneDto.Descripcio;
            CentreActual = AlumneDto.CentreActual?.Etiqueta ?? "** Sense centre assignat **";
            Desactualitzat = CursActual != null && AlumneDto.CursDarreraActualitacioDades.Id != CursActual.Id;
            EsActiu = AlumneDto.EsActiu;
            CursDarreraActualitzacio = $"Curs darrera actualització de dades: {AlumneDto.CursDarreraActualitacioDades.Descripcio}";
            NumActuacionsTxt = $"{AlumneDto.NombreActuacions} x ";
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
            using var bl = _serveis.GetBLOperation<IAlumneActivaDesactiva>();
            var dto = await bl.Toggle(Id);
            Actualitza(dto.Data);
            BrokenRules2ModelView(dto.BrokenRules);
        }

        // --- Obrir Finestra Edició ---
        public ICommand UpdateCommand { get; }
        public Interaction<AlumneUpdateViewModel, Dtoo.Alumne?> ShowUpdateDialog { get; } = new();
        private async Task ShowUpdateDialogHandle()
        {
            var update = new AlumneUpdateViewModel(_serveis, Id);
            var data = await ShowUpdateDialog.Handle(update);
            if (data != null) Actualitza(data);
        }

        // --- Obrir Finestra Actuacions ---
        public ICommand ActuacioSetCommand { get; }
        public Interaction<ActuacioSetViewModel, IIdEtiquetaDescripcio?> ShowActuacioSetDialog { get; } = new();
        private async Task ShowActuacioSetDialogHandle()
        {
            // Cap rellegida a mà en tancar: el que s'hagi fet a la finestra d'actuacions
            // arriba pel bus de canvis i la llista d'alumnes ja es refresca sola.
            var vm = new ActuacioSetViewModel(_serveis, alumneId: Id);
            await ShowActuacioSetDialog.Handle(vm);
        }

        // --- Seleccionar si estem en mode lookup ---
        public ReactiveCommand<Unit, Dtoo.Alumne> SeleccionarCommand { get; }
        private Dtoo.Alumne SelectRow() => Model;

        // --- Generar informe ---
        public ReactiveCommand<Unit, Dtoo.SaveResult?> GeneraInformeCommand { get; }
        private async Task<Dtoo.SaveResult?> DoGeneraInforme()
        {
            ResultatInformeAlumne = "";
            using var bl = _serveis.GetBLOperation<IAlumneInforme>();
            var resultat = await bl.Run(Id);
            ResultatInformeAlumne =
                resultat.Data != null ?
                $"Fitxer desat a: {resultat.Data.FullPath}" :
                "Error generant fitxer: " + string.Join(" * ", resultat.BrokenRules.Select(x => x.Message));

            return resultat.Data;
        }

        // --- Veure informe per pantalla ---
        public ICommand VeureInformeCommand { get; }
        public Interaction<AlumneInformeViewerViewModel, Unit> ShowInformeViewerDialog { get; } = new();
        private async Task ShowInformeViewerDialogHandle()
        {
            var vm = new AlumneInformeViewerViewModel(_serveis, Id);
            await ShowInformeViewerDialog.Handle(vm);
        }

    }
}
