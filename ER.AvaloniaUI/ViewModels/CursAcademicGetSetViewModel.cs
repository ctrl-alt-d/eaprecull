using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using BusinessLayer.Abstract.Services;
using ReactiveUI;
using dtoo = DTO.o.DTOs;

namespace ER.AvaloniaUI.ViewModels
{
    public class CursAcademicGetSetViewModel : ViewModelBase
    {
        private readonly ICursAcademicGetSet BLCursAcademicGetSet;
        public CursAcademicGetSetViewModel(ICursAcademicGetSet blCursAcademicGetSet)
        {
            BLCursAcademicGetSet = blCursAcademicGetSet;
            RxApp.MainThreadScheduler.Schedule(LoadCursAcademicGetSet);    
        }

        public ObservableCollection<CursAcademicRowViewModel> MyItems {get;} = new();

        private async void LoadCursAcademicGetSet()
        {
            var parms = new DTO.i.DTOs.EmptyParms();
            var l =
                await
                BLCursAcademicGetSet
                .GetItems(parms)
                ;
            l.Data!
            .Select(x=>new CursAcademicRowViewModel(x, MyItems))
            .ToList()
            .ForEach(x=>MyItems.Add(x));
        }
    }
}
