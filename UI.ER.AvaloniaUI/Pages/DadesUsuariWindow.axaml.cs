using Avalonia.Markup.Xaml;
using BusinessLayer.Abstract.Generic;
using UI.ER.AvaloniaUI.Pages.Base;
using UI.ER.ViewModels.ViewModels;

namespace UI.ER.AvaloniaUI.Pages
{
    /// <summary>
    /// El formulari de «Les meves dades». No edita cap entitat de la base de dades, però
    /// el patró és el mateix que el dels diàlegs d'edició —desar i tancar-se només si ha
    /// anat bé—, i per això reaprofita <see cref="EntityEditWindow{TVm,TDto}"/> en comptes
    /// de tornar a escriure la subscripció al <c>SubmitCommand</c>.
    /// </summary>
    public partial class DadesUsuariWindow : EntityEditWindow<DadesUsuariViewModel, DadesUsuari>
    {
        public DadesUsuariWindow() => InitializeComponent();

        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}
