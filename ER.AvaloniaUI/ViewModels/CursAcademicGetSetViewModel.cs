using System.Collections.ObjectModel;
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

        public ObservableCollection<dtoo.CursAcademic> MyItems {get;} = new();

        private async void LoadCursAcademicGetSet()
        {
            var createParms = new DTO.i.DTOs.EmptyParms();
            var l =
                await
                BLCursAcademicGetSet
                .GetItems(createParms)
                ;
            l.Data!.ForEach(x=>MyItems.Add(x));
        }
    }
}
