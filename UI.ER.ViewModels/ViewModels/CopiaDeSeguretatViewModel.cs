using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusinessLayer.Abstract.Exceptions;
using BusinessLayer.Abstract.Generic;
using BusinessLayer.Abstract.Services;
using DynamicData.Binding;
using ReactiveUI;

namespace UI.ER.ViewModels.ViewModels
{
    /// <summary>
    /// La finestra «Còpia de seguretat»: on van les còpies, la contrasenya que les xifra i
    /// les que ja hi ha al destí.
    /// </summary>
    /// <remarks>
    /// El ViewModel no coneix cap <see cref="IMagatzemDeCopies"/>: tot passa per
    /// <see cref="ICopiaDeSeguretat"/>, i el destí és infraestructura que la UI no ha de
    /// veure. Triar la carpeta sí que és cosa de la vista —el selector és d'Avalonia—, i
    /// per això va per <see cref="ShowTriaCarpetaDialog"/>, com la navegació.
    /// <para>
    /// La contrasenya <strong>no es desa enlloc</strong> i es buida en acabar, tant si la
    /// còpia ha anat bé com si no.
    /// </para>
    /// </remarks>
    public class CopiaDeSeguretatViewModel : ViewModelBase
    {
        /// <summary>Prou per no ser trivial, i no tant com per acabar en un post-it.</summary>
        public const int MinimDeCaracters = 8;

        private readonly IServiceFactory _serveis;

        private CancellationTokenSource? _cancelacio;

        public CopiaDeSeguretatViewModel(IServiceFactory serveis)
        {
            _serveis = serveis;

            // Com a la resta de finestres: llegir el disc no es fa al constructor, que el
            // contenidor ha de poder construir aquest ViewModel sense efectes secundaris.
            RxApp.MainThreadScheduler.Schedule(LoadData);

            var potCopiar = this
                .WhenAnyValue(x => x.Password, x => x.Confirmacio, x => x.DestiConfigurat, x => x.EnMarxa,
                    (password, confirmacio, configurat, enMarxa) =>
                        configurat && !enMarxa && Valida(password, confirmacio).Count == 0);

            // El mateix criteri que habilita el botó alimenta el text que diu què falta:
            // un botó apagat sense explicació és un carreró sense sortida per a l'usuari.
            this.WhenAnyValue(x => x.Password, x => x.Confirmacio, x => x.DestiConfigurat, x => x.EnMarxa,
                    (password, confirmacio, configurat, enMarxa) =>
                        enMarxa ? string.Empty : PrimerRequisit(password, confirmacio, configurat))
                .Subscribe(motiu => QueFalta = motiu);

            var enMarxa = this.WhenAnyValue(x => x.EnMarxa);
            var noEnMarxa = enMarxa.Select(x => !x);

            FesLaCopiaCommand = ReactiveCommand.CreateFromTask(DoCopia, potCopiar);
            AraNoCommand = ReactiveCommand.Create(() => Unit.Default, noEnMarxa);
            TriaCarpetaCommand = ReactiveCommand.CreateFromTask(DoTriaCarpeta, noEnMarxa);
            CancelaCommand = ReactiveCommand.Create(DoCancela, enMarxa);
        }

        // -- El destí -----------------------------------------------------------------

        /// <summary>Els destins d'aquesta compilació. A la fase 1 només n'hi ha un.</summary>
        public ObservableCollectionExtended<DestiDisponible> Destins { get; } = new();

        /// <summary>
        /// Amb un sol destí ni cal ensenyar el selector: la finestra només ha de deixar
        /// triar la carpeta.
        /// </summary>
        public bool HiHaMesDunDesti => Destins.Count > 1;

        private DestiDisponible? _DestiTriat;
        public DestiDisponible? DestiTriat
        {
            get => _DestiTriat;
            set => this.RaiseAndSetIfChanged(ref _DestiTriat, value);
        }

        private bool _DestiConfigurat;
        public bool DestiConfigurat
        {
            get => _DestiConfigurat;
            set => this.RaiseAndSetIfChanged(ref _DestiConfigurat, value);
        }

        /// <summary>On van les còpies ara mateix, tal com s'ha d'ensenyar.</summary>
        private string _Desti = string.Empty;
        public string Desti
        {
            get => _Desti;
            set => this.RaiseAndSetIfChanged(ref _Desti, value);
        }

        // -- La proposta d'arrencada --------------------------------------------------

        /// <summary>
        /// Fa més de dues setmanes de l'última còpia i hi ha actuacions noves. Quan és
        /// cert, la finestra s'obre sola en arrencar i ensenya la pancarta i el botó
        /// «Ara no»; oberta des del menú, tots dos són invisibles.
        /// </summary>
        private bool _CalFerCopia;
        public bool CalFerCopia
        {
            get => _CalFerCopia;
            set => this.RaiseAndSetIfChanged(ref _CalFerCopia, value);
        }

        /// <summary>El text de la proposta: quants dies fa i quantes actuacions noves hi ha.</summary>
        private string _Proposta = string.Empty;
        public string Proposta
        {
            get => _Proposta;
            set => this.RaiseAndSetIfChanged(ref _Proposta, value);
        }

        // -- La còpia -----------------------------------------------------------------

        private string _Password = string.Empty;
        public string Password
        {
            get => _Password;
            set => this.RaiseAndSetIfChanged(ref _Password, value);
        }

        private string _Confirmacio = string.Empty;
        public string Confirmacio
        {
            get => _Confirmacio;
            set => this.RaiseAndSetIfChanged(ref _Confirmacio, value);
        }

        /// <summary>
        /// Què falta perquè «Fes la còpia ara» es pugui clicar, o cadena buida si no falta
        /// res. Es calcula amb el mateix <see cref="PrimerRequisit(string, string, bool)"/> que
        /// governa el <c>canExecute</c> de la comanda: si divergissin, el botó estaria
        /// apagat dient que tot és correcte.
        /// </summary>
        private string _QueFalta = string.Empty;
        public string QueFalta
        {
            get => _QueFalta;
            set => this.RaiseAndSetIfChanged(ref _QueFalta, value);
        }

        private bool _EnMarxa;
        public bool EnMarxa
        {
            get => _EnMarxa;
            set => this.RaiseAndSetIfChanged(ref _EnMarxa, value);
        }

        /// <summary>El que alimenta l'<c>IProgress&lt;string&gt;</c> de l'operació.</summary>
        private string _Estat = string.Empty;
        public string Estat
        {
            get => _Estat;
            set => this.RaiseAndSetIfChanged(ref _Estat, value);
        }

        /// <summary>El resum de l'última còpia feta en aquesta sessió.</summary>
        private string _Resultat = string.Empty;
        public string Resultat
        {
            get => _Resultat;
            set => this.RaiseAndSetIfChanged(ref _Resultat, value);
        }

        private string _Avis = string.Empty;
        public string Avis
        {
            get => _Avis;
            set => this.RaiseAndSetIfChanged(ref _Avis, value);
        }

        // -- Les còpies que hi ha -----------------------------------------------------

        public ObservableCollectionExtended<CopiaRemota> Copies { get; } = new();

        private string _DarreraCopia = string.Empty;
        public string DarreraCopia
        {
            get => _DarreraCopia;
            set => this.RaiseAndSetIfChanged(ref _DarreraCopia, value);
        }

        private string _QuantesEsGuarden = string.Empty;
        public string QuantesEsGuarden
        {
            get => _QuantesEsGuarden;
            set => this.RaiseAndSetIfChanged(ref _QuantesEsGuarden, value);
        }

        public ObservableCollectionExtended<string> BrokenRules { get; } = new();

        // -- Comandes -----------------------------------------------------------------

        public ReactiveCommand<Unit, Unit> FesLaCopiaCommand { get; }

        public ReactiveCommand<Unit, Unit> TriaCarpetaCommand { get; }

        public ReactiveCommand<Unit, Unit> CancelaCommand { get; }

        /// <summary>
        /// Tanca la finestra sense fer la còpia. La proposta és una proposta: qui aquell
        /// dia no té el llapis a sobre ha de poder continuar treballant, i tornarà a sortir
        /// la propera arrencada.
        /// </summary>
        public ReactiveCommand<Unit, Unit> AraNoCommand { get; }

        /// <summary>
        /// El selector de carpetes és d'Avalonia i el ViewModel no la pot conèixer. Mateix
        /// mecanisme que la navegació: el codi rere la vista el resol amb
        /// <c>StorageProvider.OpenFolderPickerAsync</c> i en torna el camí, o null si
        /// l'usuari ha tancat el diàleg.
        /// </summary>
        public Interaction<Unit, string?> ShowTriaCarpetaDialog { get; } = new();

        // -- El que fan ---------------------------------------------------------------

        private async void LoadData()
        {
            BrokenRules.Clear();

            using var bl = _serveis.GetBLOperation<ICopiaDeSeguretat>();

            Destins.Clear();
            Destins.AddRange(bl.Destins);
            this.RaisePropertyChanged(nameof(HiHaMesDunDesti));

            DestiTriat = Destins.FirstOrDefault(d => d.Clau == bl.ClauDelDestiActual)
                         ?? Destins.FirstOrDefault();

            QuantesEsGuarden = $"Se'n guarden {bl.CopiesQueEsGuarden}; en fer-ne una de nova, "
                             + "la més antiga s'esborra del destí.";

            await MostraLaProposta(bl);
            await MostraElDesti(bl);
        }

        private async Task DoTriaCarpeta()
        {
            BrokenRules.Clear();

            var carpeta = await ShowTriaCarpetaDialog.Handle(Unit.Default);

            if (string.IsNullOrWhiteSpace(carpeta))
                return;

            using var bl = _serveis.GetBLOperation<ICopiaDeSeguretat>();

            var clau = DestiTriat?.Clau ?? bl.Destins.FirstOrDefault()?.Clau;
            if (clau is null)
                return;

            var resultat = await bl.ConfiguraDesti(clau, carpeta);

            Afegeix(resultat.BrokenRules);

            await MostraElDesti(bl);
        }

        private async Task DoCopia()
        {
            BrokenRules.Clear();
            Resultat = string.Empty;
            Avis = string.Empty;

            var trencades = Valida(Password, Confirmacio);
            if (trencades.Count > 0)
            {
                Afegeix(trencades);
                return;
            }

            using var cancelacio = new CancellationTokenSource();
            _cancelacio = cancelacio;

            EnMarxa = true;
            Estat = "Preparant la còpia…";

            try
            {
                using var bl = _serveis.GetBLOperation<ICopiaDeSeguretat>();

                var progres = new Progress<string>(text => Estat = text);
                var resultat = await bl.Run(Password, progres, cancelacio.Token);

                Afegeix(resultat.BrokenRules);

                if (resultat.Data is { } copia)
                {
                    Resultat = $"{copia.Etiqueta}. {copia.Descripcio}";
                    Avis = copia.Avis ?? string.Empty;
                }

                Estat = string.Empty;

                // Si la còpia ha anat bé, la proposta ja no toca: la pancarta i el botó
                // «Ara no» han de desaparèixer sense haver de tancar i tornar a obrir.
                await MostraLaProposta(bl);
                await MostraElDesti(bl);
            }
            finally
            {
                // Passi el que passi: la contrasenya no s'ha de quedar al ViewModel.
                Password = Confirmacio = string.Empty;

                EnMarxa = false;
                _cancelacio = null;
            }
        }

        private void DoCancela()
        {
            Estat = "Cancel·lant…";
            _cancelacio?.Cancel();
        }

        /// <summary>
        /// Pregunta al negoci si toca proposar una còpia. La regla —dues setmanes i
        /// actuacions noves— viu allà i no aquí: la comparteixen aquesta finestra i el
        /// taulell, que és qui decideix obrir-la en arrencar.
        /// </summary>
        private async Task MostraLaProposta(ICopiaDeSeguretat bl)
        {
            var proposta = (await bl.CalFerCopia()).Data ?? PropostaDeCopia.No;

            CalFerCopia = proposta.Cal;
            Proposta = proposta.Descripcio;
        }

        /// <summary>Rellegeix el destí i les còpies que hi ha, i actualitza els avisos.</summary>
        private async Task MostraElDesti(ICopiaDeSeguretat bl)
        {
            var desti = bl.DestiActual;

            DestiConfigurat = desti.Configurat;
            Desti = desti.Configurat ? desti.Detall : "Encara no has triat on desar les còpies.";

            DarreraCopia = bl.DarreraCopia is { } quan
                ? $"Darrera còpia: {quan:dd/MM/yyyy} a les {quan:HH:mm}"
                : "Encara no s'ha fet cap còpia.";

            Copies.Clear();

            if (!desti.Configurat)
                return;

            var llistat = await bl.Llista();

            Afegeix(llistat.BrokenRules);

            if (llistat.Data is { } copies)
                Copies.AddRange(copies.Items);
        }

        /// <summary>
        /// El primer requisit que no es compleix, en l'ordre en què l'usuari els troba:
        /// primer el destí, després la contrasenya. En diu un de sol perquè un botó apagat
        /// només necessita respondre una pregunta: «i ara què em falta?».
        /// </summary>
        private static string PrimerRequisit(string password, string confirmacio, bool destiConfigurat)
        {
            if (!destiConfigurat)
                return "Tria primer on vols desar les còpies.";

            if (Valida(password, confirmacio).FirstOrDefault() is { } trencada)
                return trencada.Message;

            return string.Empty;
        }

        /// <summary>
        /// Les validacions del formulari. Les de negoci —que la carpeta existeixi, que s'hi
        /// pugui escriure— són a l'operació i a l'adaptador, que és on valen per a
        /// qualsevol consumidor.
        /// </summary>
        private static List<BrokenRule> Valida(string password, string confirmacio)
        {
            var trencades = new List<BrokenRule>();

            if (string.IsNullOrEmpty(password))
                trencades.Add(new(nameof(Password), "Cal una contrasenya per xifrar la còpia."));
            else if (password.Length < MinimDeCaracters)
                trencades.Add(new(nameof(Password),
                    $"La contrasenya ha de tenir com a mínim {MinimDeCaracters} caràcters."));

            if (password != confirmacio)
                trencades.Add(new(nameof(Confirmacio), "Les dues contrasenyes no coincideixen."));

            return trencades;
        }

        /// <summary>
        /// <strong>Acumula</strong>, no substitueix: qui comença una acció buida la llista
        /// primer. Si esborrés, el refresc del destí que ve tot seguit s'emportaria l'error
        /// que l'usuari acaba de provocar i no en quedaria rastre a la pantalla.
        /// </summary>
        private void Afegeix(List<BrokenRule> trencades)
            => BrokenRules.AddRange(trencades.Select(x => x.Message));
    }
}
