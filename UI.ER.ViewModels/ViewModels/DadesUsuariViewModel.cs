using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using BusinessLayer.Abstract.Exceptions;
using BusinessLayer.Abstract.Generic;
using DynamicData.Binding;
using ReactiveUI;

namespace UI.ER.ViewModels.ViewModels
{
    /// <summary>
    /// El formulari de «Les meves dades». Només tres camps i un desat: la validació és al
    /// servei, no aquí, perquè és una regla de negoci i val igual per a qualsevol
    /// consumidor.
    /// </summary>
    /// <remarks>
    /// Les dades es carreguen amb el scheduler i no al constructor: llegir el <c>.ini</c>
    /// és tocar el disc, i el contenidor ha de poder construir aquest ViewModel sense
    /// efectes secundaris. És el mateix diferiment que fan les llistes amb la consulta.
    /// </remarks>
    public class DadesUsuariViewModel : ViewModelBase, ISubmitViewModel<DadesUsuari>
    {
        private readonly IServiceFactory _serveis;

        public DadesUsuariViewModel(IServiceFactory serveis)
        {
            _serveis = serveis;

            RxApp.MainThreadScheduler.Schedule(LoadData);

            SubmitCommand = ReactiveCommand.CreateFromTask(SaveData);
        }

        private void LoadData()
        {
            var dades = _serveis.DadesUsuari.Actuals;

            Nom = dades.Nom;
            Cognoms = dades.Cognoms;
            AdrecaXtec = dades.AdrecaXtec;
            Ubicacio = _serveis.DadesUsuari.Ubicacio;
        }

        private string _Nom = string.Empty;
        public string Nom
        {
            get => _Nom;
            set => this.RaiseAndSetIfChanged(ref _Nom, value);
        }

        private string _Cognoms = string.Empty;
        public string Cognoms
        {
            get => _Cognoms;
            set => this.RaiseAndSetIfChanged(ref _Cognoms, value);
        }

        private string _AdrecaXtec = string.Empty;
        public string AdrecaXtec
        {
            get => _AdrecaXtec;
            set => this.RaiseAndSetIfChanged(ref _AdrecaXtec, value);
        }

        /// <summary>On és el fitxer, perquè l'usuari el pugui trobar o copiar.</summary>
        private string _Ubicacio = string.Empty;
        public string Ubicacio
        {
            get => _Ubicacio;
            set => this.RaiseAndSetIfChanged(ref _Ubicacio, value);
        }

        public ObservableCollectionExtended<string> BrokenRules { get; } = new();

        public ReactiveCommand<Unit, DadesUsuari?> SubmitCommand { get; }

        /// <summary>
        /// Retorna les dades desades, o <c>null</c> si el servei les ha rebutjat: és el
        /// que fa que la finestra es tanqui només quan el desat ha anat bé.
        /// </summary>
        private async Task<DadesUsuari?> SaveData()
        {
            BrokenRules.Clear();

            var resultat = await _serveis.DadesUsuari.Desa(new DadesUsuari(Nom, Cognoms, AdrecaXtec));

            BrokenRules2ModelView(resultat.BrokenRules);

            return resultat.Data;
        }

        private void BrokenRules2ModelView(List<BrokenRule> brokenRules)
        {
            BrokenRules.Clear();
            BrokenRules.AddRange(brokenRules.Select(x => x.Message));
        }
    }
}
