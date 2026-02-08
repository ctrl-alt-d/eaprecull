using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using BusinessLayer.Abstract.Services;
using DynamicData.Binding;
using ReactiveUI;
using UI.ER.ViewModels.Services;
using Dtoo = DTO.o.DTOs;

namespace UI.ER.ViewModels.ViewModels
{
    /// <summary>
    /// ViewModel per mostrar l'informe d'un alumne per pantalla
    /// </summary>
    public class AlumneInformeViewerViewModel : ViewModelBase
    {
        private readonly int _alumneId;

        public AlumneInformeViewerViewModel(int alumneId)
        {
            _alumneId = alumneId;
            CloseCommand = ReactiveCommand.Create(() => { });
            LoadDataCommand = ReactiveCommand.CreateFromTask(LoadData);
            ExportarWordCommand = ReactiveCommand.CreateFromTask(DoExportarWord);
        }

        // --- Propietats de l'Alumne ---

        private string _nomComplet = string.Empty;
        public string NomComplet
        {
            get => _nomComplet;
            private set => this.RaiseAndSetIfChanged(ref _nomComplet, value);
        }

        private string _dataNaixement = string.Empty;
        public string DataNaixement
        {
            get => _dataNaixement;
            private set => this.RaiseAndSetIfChanged(ref _dataNaixement, value);
        }

        private string _centreActual = string.Empty;
        public string CentreActual
        {
            get => _centreActual;
            private set => this.RaiseAndSetIfChanged(ref _centreActual, value);
        }

        private string _etapaActual = string.Empty;
        public string EtapaActual
        {
            get => _etapaActual;
            private set => this.RaiseAndSetIfChanged(ref _etapaActual, value);
        }

        private string _nivellActual = string.Empty;
        public string NivellActual
        {
            get => _nivellActual;
            private set => this.RaiseAndSetIfChanged(ref _nivellActual, value);
        }

        private string _tags = string.Empty;
        public string Tags
        {
            get => _tags;
            private set => this.RaiseAndSetIfChanged(ref _tags, value);
        }

        // NESE NEE
        private bool _teNESENEE;
        public bool TeNESENEE
        {
            get => _teNESENEE;
            private set => this.RaiseAndSetIfChanged(ref _teNESENEE, value);
        }

        private string _dataNESENEE = string.Empty;
        public string DataNESENEE
        {
            get => _dataNESENEE;
            private set => this.RaiseAndSetIfChanged(ref _dataNESENEE, value);
        }

        private string _observacionsNESENEE = string.Empty;
        public string ObservacionsNESENEE
        {
            get => _observacionsNESENEE;
            private set => this.RaiseAndSetIfChanged(ref _observacionsNESENEE, value);
        }

        // NESE No NEE
        private bool _teNESENoNEE;
        public bool TeNESENoNEE
        {
            get => _teNESENoNEE;
            private set => this.RaiseAndSetIfChanged(ref _teNESENoNEE, value);
        }

        private string _dataNESENoNEE = string.Empty;
        public string DataNESENoNEE
        {
            get => _dataNESENoNEE;
            private set => this.RaiseAndSetIfChanged(ref _dataNESENoNEE, value);
        }

        private string _observacionsNESENoNEE = string.Empty;
        public string ObservacionsNESENoNEE
        {
            get => _observacionsNESENoNEE;
            private set => this.RaiseAndSetIfChanged(ref _observacionsNESENoNEE, value);
        }

        // --- Actuacions ---

        private int _totalActuacions;
        public int TotalActuacions
        {
            get => _totalActuacions;
            private set => this.RaiseAndSetIfChanged(ref _totalActuacions, value);
        }

        private string _tempsTotalTxt = string.Empty;
        public string TempsTotalTxt
        {
            get => _tempsTotalTxt;
            private set => this.RaiseAndSetIfChanged(ref _tempsTotalTxt, value);
        }

        public ObservableCollectionExtended<Dtoo.ActuacioInformeItem> Actuacions { get; } = new();

        // --- Estat ---

        private bool _isLoading = true;
        public bool IsLoading
        {
            get => _isLoading;
            private set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        private bool _hasError;
        public bool HasError
        {
            get => _hasError;
            private set => this.RaiseAndSetIfChanged(ref _hasError, value);
        }

        public ObservableCollectionExtended<string> BrokenRules { get; } = new();

        // --- Commands ---

        public ReactiveCommand<Unit, Unit> CloseCommand { get; }
        public ReactiveCommand<Unit, Unit> LoadDataCommand { get; }
        public ReactiveCommand<Unit, Dtoo.SaveResult?> ExportarWordCommand { get; }

        // --- Resultat exportació ---

        private string _resultatExportacio = string.Empty;
        public string ResultatExportacio
        {
            get => _resultatExportacio;
            private set => this.RaiseAndSetIfChanged(ref _resultatExportacio, value);
        }

        // --- Càrrega de dades ---

        private async Task LoadData()
        {
            IsLoading = true;
            HasError = false;
            BrokenRules.Clear();

            using var bl = SuperContext.Resolve<IAlumneInformeViewer>();
            var result = await bl.Run(_alumneId);

            if (result.BrokenRules.Any())
            {
                HasError = true;
                BrokenRules.AddRange(result.BrokenRules.Select(r => r.Message));
            }
            else if (result.Data != null)
            {
                MapData(result.Data);
            }

            IsLoading = false;
        }

        private void MapData(Dtoo.AlumneInformeViewerData data)
        {
            NomComplet = data.NomComplet;
            DataNaixement = data.DataNaixementTxt;
            CentreActual = data.CentreActual;
            EtapaActual = data.EtapaActual;
            NivellActual = data.NivellActual;
            Tags = data.Tags;

            TeNESENEE = data.TeNESENEE;
            DataNESENEE = data.DataNESENEETxt;
            ObservacionsNESENEE = data.ObservacionsNESENEE;

            TeNESENoNEE = data.TeNESENoNEE;
            DataNESENoNEE = data.DataNESENoNEETxt;
            ObservacionsNESENoNEE = data.ObservacionsNESENoNEE;

            TotalActuacions = data.TotalActuacions;
            TempsTotalTxt = data.TempsTotalTxt;
            Actuacions.Clear();
            Actuacions.AddRange(data.Actuacions);
        }

        // --- Exportar a Word ---

        private async Task<Dtoo.SaveResult?> DoExportarWord()
        {
            ResultatExportacio = "";
            using var bl = SuperContext.Resolve<IAlumneInforme>();
            var resultat = await bl.Run(_alumneId);
            ResultatExportacio =
                resultat.Data != null ?
                $"Fitxer desat a: {resultat.Data.FullPath}" :
                "Error generant fitxer: " + string.Join(" * ", resultat.BrokenRules.Select(x => x.Message));

            return resultat.Data;
        }
    }
}
