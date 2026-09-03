using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace UI.ER.AvaloniaUI.Helpers
{
    public static class VisualRootExtensions
    {
        /// <summary>
        /// Finestra que conté aquest element. Substitueix les 10 còpies de
        /// <c>private Window GetWindow() => (Window)this.VisualRoot!;</c> escampades pel
        /// codi rere les vistes.
        /// </summary>
        /// <remarks>
        /// Només és vàlida quan l'element ja penja de l'arbre visual: cridar-la des d'un
        /// constructor peta. Els usos legítims són dins de <c>WhenActivated</c> o d'un
        /// handler d'<c>Interaction</c>.
        /// </remarks>
        public static Window GetOwnerWindow(this Visual visual)
            => (Window)visual.GetVisualRoot()!;
    }
}
