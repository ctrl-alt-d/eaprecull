using Avalonia;
using Avalonia.Styling;

namespace UI.ER.AvaloniaUI.Helpers
{
    /// <summary>
    /// Commuta el tema clar/fosc de l'aplicació.
    /// </summary>
    /// <remarks>
    /// Només toca <see cref="Application.RequestedThemeVariant"/>. El
    /// <c>MaterialTheme</c> d'App.axaml està declarat amb <c>BaseTheme="Inherit"</c>,
    /// de manera que segueix aquesta variant tot sol: els dos valors de l'invariant 10
    /// ja no es poden desincronitzar perquè n'hi ha un de sol.
    /// </remarks>
    public static class Tema
    {
        /// <summary>Variant que s'està pintant ara mateix.</summary>
        public static ThemeVariant Actual =>
            Application.Current?.ActualThemeVariant ?? ThemeVariant.Light;

        public static bool EsFosc => Actual == ThemeVariant.Dark;

        /// <summary>Passa de clar a fosc i a l'inrevés.</summary>
        public static void Alterna() => Aplica(EsFosc ? ThemeVariant.Light : ThemeVariant.Dark);

        public static void Aplica(ThemeVariant variant)
        {
            if (Application.Current is { } app)
                app.RequestedThemeVariant = variant;
        }
    }
}
