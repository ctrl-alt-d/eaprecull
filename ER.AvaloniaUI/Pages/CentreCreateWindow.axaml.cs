using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using BusinessLayer.Abstract;
using Material.Dialog.Interfaces;
using dtoo = DTO.o.DTOs;

namespace ER.AvaloniaUI.Pages
{
    public class CentreCreateWindow : Window, IDialogWindowResult<OperationResult<dtoo.Centre>>
    { 
        public OperationResult<dtoo.Centre> Result { get; set; } = default!;
        public CentreCreateWindow()
        {
            this.InitializeComponent(); 
        }        
        public OperationResult<dtoo.Centre> GetResult() => Result;
        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}
