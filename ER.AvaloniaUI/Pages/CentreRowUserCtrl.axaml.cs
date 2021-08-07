using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using BusinessLayer.Abstract;
using ER.AvaloniaUI.ViewModels;
using ReactiveUI;
using dtoo = DTO.o.DTOs;

namespace ER.AvaloniaUI.Pages
{
    public class CentreRowUserCtrl : UserControl {
        public CentreRowUserCtrl( ) {
            InitializeComponent();            
        }
        
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

    }
}