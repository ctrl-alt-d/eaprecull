using System.Reactive;
using ReactiveUI;
using dtoo = DTO.o.DTOs;
using CommonInterfaces;
using System.Threading.Tasks;
using ER.AvaloniaUI.Services;
using BusinessLayer.Abstract.Services;

namespace ER.AvaloniaUI.ViewModels
{
    public class CentreCreateViewModel : ViewModelBase
    {
        public string Prova {get; set;} = "";
    }
}