using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Material.Dialog.Interfaces;
using dtoo = DTO.o.DTOs;

namespace ER.AvaloniaUI.ViewsCentre
{
    public class CentreCreate : Window, IDialogWindowResult<dtoo.Centre>
    { 
        public dtoo.Centre Result { get; set; } = default!;

        public CentreCreate()
        {
            this.InitializeComponent(); 
        }
        
        public dtoo.Centre GetResult() => Result;

        public void SetNegativeResult(dtoo.Centre result) => Result = result;

        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}
