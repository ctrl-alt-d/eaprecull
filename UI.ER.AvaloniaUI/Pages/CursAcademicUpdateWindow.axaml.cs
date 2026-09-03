using Avalonia.Markup.Xaml;
using BusinessLayer.Abstract;
using Dtoo = DTO.o.DTOs;
using UI.ER.AvaloniaUI.Pages.Base;
using UI.ER.ViewModels.ViewModels;

namespace UI.ER.AvaloniaUI.Pages
{
    public partial class CursAcademicUpdateWindow : EntityEditWindow<CursAcademicUpdateViewModel, Dtoo.CursAcademic>
    {
        // ToDo (R5): propietat morta, ningú no la llegeix ni l'escriu.
        public OperationResult<Dtoo.CursAcademic> Result { get; set; } = default!;

        public CursAcademicUpdateWindow() => InitializeComponent();

        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}
