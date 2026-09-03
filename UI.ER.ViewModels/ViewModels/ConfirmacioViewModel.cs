using System.Reactive;
using ReactiveUI;

namespace UI.ER.ViewModels.ViewModels
{
    /// <summary>
    /// Diàleg de «segur que…?». No parla amb el BusinessLayer: només porta els textos i
    /// les dues comandes, perquè la finestra sàpiga amb quin resultat s'ha de tancar.
    /// </summary>
    /// <remarks>
    /// Els paràmetres tenen valor per defecte perquè el contenidor el pugui construir
    /// (regla de registre de <c>DI.Injection</c>) i perquè el
    /// <c>&lt;Design.DataContext&gt;</c> de l'AXAML el pugui instanciar.
    /// </remarks>
    public class ConfirmacioViewModel : ViewModelBase
    {
        public ConfirmacioViewModel(
            string titol = "Confirmació",
            string missatge = "N'estàs segur?",
            string textAfirmatiu = "Sí, endavant")
        {
            Titol = titol;
            Missatge = missatge;
            TextAfirmatiu = textAfirmatiu;

            ConfirmaCommand = ReactiveCommand.Create(() => true);
            CancelaCommand = ReactiveCommand.Create(() => false);
        }

        public string Titol { get; }

        public string Missatge { get; }

        /// <summary>
        /// Text del botó que confirma. Ha de dir què passarà — «Sí, esborrar» — i no
        /// un «Sí» genèric: qui l'obre és qui sap de quina acció es tracta.
        /// </summary>
        public string TextAfirmatiu { get; }

        public string TextNegatiu => "Cancel·lar";

        public ReactiveCommand<Unit, bool> ConfirmaCommand { get; }

        public ReactiveCommand<Unit, bool> CancelaCommand { get; }
    }
}
