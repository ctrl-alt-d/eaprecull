using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BusinessLayer.Abstract;
using BusinessLayer.Abstract.Generic;
using CommonInterfaces;
using DynamicData.Binding;
using ReactiveUI;
using UI.ER.ViewModels.Services;

namespace UI.ER.ViewModels.ViewModels.Base
{
    /// <summary>
    /// El que les sis llistes tenien copiat: la col·lecció de files, l'estat de càrrega i
    /// el bucle consulta → files. A sobre hi va el que és nou, el refresc silenciós que
    /// dispara el bus de canvis.
    /// </summary>
    /// <remarks>
    /// <c>abstract</c> a posta: és el que la deixa fora de l'escaneig de ViewModels
    /// d'<c>UIConfigureServices</c> i dels tests d'inventari (invariant §9.6 del readme).
    /// </remarks>
    public abstract class SetViewModelBase<TRow, TDto> : ViewModelBase
        where TRow : class, IFilaDeLlista<TDto>
        where TDto : class, IIdEtiquetaDescripcio
    {
        protected SetViewModelBase(IServiceFactory serveis, bool modeLookup)
        {
            Serveis = serveis;
            ModeLookup = modeLookup;

            // Qualsevol escriptura del BusinessLayer que toqui una de les entitats que
            // aquesta llista pinta la fa rellegir-se en silenci. La subscripció viu mentre
            // la finestra està activa: el WhenActivated de la vista activa també
            // l'Activator del ViewModel.
            this.WhenActivated(d =>
                Serveis.Canvis.ComObservable()
                    .Where(EnsAfecta)
                    .Throttle(TimeSpan.FromMilliseconds(300))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .SelectMany(_ => Observable.FromAsync(RefrescaSilenciosament))
                    .Subscribe()
                    .DisposeWith(d));
        }

        protected IServiceFactory Serveis { get; }

        public bool ModeLookup { get; }

        public ObservableCollectionExtended<TRow> MyItems { get; } = new();

        public ObservableCollectionExtended<string> BrokenRules { get; } = new();

        private string _PaginatedMsg = string.Empty;
        public string PaginatedMsg
        {
            get => _PaginatedMsg;
            set => this.RaiseAndSetIfChanged(ref _PaginatedMsg, value);
        }

        private bool _Loading = true;
        public bool Loading
        {
            get => _Loading;
            set => this.RaiseAndSetIfChanged(ref _Loading, value);
        }

        /// <summary>La consulta de la llista, amb els paràmetres que cada filtre hi posa.</summary>
        protected abstract Task<OperationResults<TDto>> Consulta();

        /// <summary>
        /// Construeix una fila. Ho fa la llista i no la classe base perquè els
        /// <c>*RowViewModel</c> tenen signatures diferents.
        /// </summary>
        protected abstract TRow CreaFila(TDto dto);

        /// <summary>
        /// Ganxo per a les dades que la llista necessita abans de construir files i que no
        /// venen de la consulta —el curs actiu, típicament.
        /// </summary>
        protected virtual Task AbansDeCrearFiles() => Task.CompletedTask;

        /// <summary>
        /// Càrrega completa: buida la llista i la torna a omplir. És el camí dels filtres,
        /// i el que els <c>*SetViewModel</c> disparen des del constructor (invariant §9.2).
        /// </summary>
        protected async void CarregaAra() => await Carrega();

        protected async Task Carrega()
        {
            Loading = true;
            MyItems.Clear();

            var resultat = await Consulta();

            BrokenRules.Clear();
            BrokenRules.AddRange(resultat.BrokenRules.Select(x => x.Message));

            // Ha fallat la petició
            if (resultat.Data == null)
                throw new Exception("Error en fer petició al backend"); // ToDo: gestionar broken rules

            await AbansDeCrearFiles();

            MyItems.AddRange(resultat.Data.Select(CreaFila));

            PaginatedMsg = MissatgeDePaginacio(resultat);

            Loading = false;
        }

        /// <summary>
        /// Repeteix la consulta i pedaça les files existents casant per <c>Id</c>: sense
        /// <c>Clear()</c>, sense tocar <see cref="Loading"/> i sense reconstruir la
        /// col·lecció, de manera que no es perd ni la posició de scroll ni la selecció.
        /// </summary>
        /// <remarks>
        /// Les files que ja no surten a la consulta es treuen. Les que hi apareixen de nou
        /// <em>no</em> s'afegeixen: podrien no complir el filtre de qui mira, i fer-les
        /// aparèixer li mouria el que està llegint.
        /// </remarks>
        protected async Task RefrescaSilenciosament()
        {
            var resultat = await Consulta();

            if (resultat.Data == null)
                return;

            var vigents = resultat.Data.ToDictionary(x => x.Id);

            foreach (var fila in MyItems.ToList())
                if (vigents.TryGetValue(fila.Id, out var dto))
                    fila.Actualitza(dto);
                else
                    MyItems.Remove(fila);

            PaginatedMsg = MissatgeDePaginacio(resultat);
        }

        /// <summary>
        /// La regla del bus: una llista es refresca quan les entitats que el canvi ha tocat
        /// coincideixen amb alguna de les que les seves files pinten. Cap mapa de
        /// dependències escrit a mà; les dues bandes apliquen <see cref="Referencies"/>.
        /// </summary>
        protected bool EnsAfecta(CanviDeDomini canvi)
            => canvi.AfectaTot
               || MyItems.Any(fila => canvi.Afectats.Overlaps(fila.ReferenciesPintades));

        private string MissatgeDePaginacio(OperationResults<TDto> resultat)
            => resultat.Total > resultat.TakeRequested
                ? $"Mostrant els {MyItems.Count} primers resultats de {resultat.Total} seleccionats"
                : $"Seleccionats {MyItems.Count} items";
    }
}
