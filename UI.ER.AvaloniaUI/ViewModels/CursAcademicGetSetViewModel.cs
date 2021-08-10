using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using BusinessLayer.Abstract.Services;
using UI.ER.AvaloniaUI.Services;
using ReactiveUI;
using dtoo = DTO.o.DTOs;

namespace UI.ER.AvaloniaUI.ViewModels
{
    public class CursAcademicGetSetViewModel : ViewModelBase
    {
        protected virtual ICursAcademicGetSet BLCursAcademicGetSet() => SuperContext.GetBLOperation<ICursAcademicGetSet>();
        public CursAcademicGetSetViewModel()
        {
            RxApp.MainThreadScheduler.Schedule(LoadCursAcademicGetSet);    
        }

        public ObservableCollection<CursAcademicRowViewModel> MyItems {get;} = new();

        private async void LoadCursAcademicGetSet()
        {
            var parms = new DTO.i.DTOs.EmptyParms();

            using var bl = BLCursAcademicGetSet();
            var l =
                await
                bl
                .FromPredicate(parms)
                ;

            l
            .Data! // ToDo: gestionar broken rules
            .Select(x=>new CursAcademicRowViewModel(x, MyItems))
            .ToList()
            .ForEach(x=>MyItems.Add(x));
        }
    }
}
