using System.Linq;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using BusinessLayer.Abstract.Services;
using ReactiveUI;
using dtoo = DTO.o.DTOs;
using ER.AvaloniaUI.Services;

namespace ER.AvaloniaUI.ViewModels
{
    public class CentreSetViewModel : ViewModelBase
    {
        protected virtual ICentreGetSet BLCentres() => SuperContext.GetBLOperation<ICentreGetSet>();
        public CentreSetViewModel()
        {
            RxApp.MainThreadScheduler.Schedule(LoadCentres);    
        }
        public ObservableCollection<CentreRowViewModel> MyItems {get;} = new();

        private async void LoadCentres()
        {
            var parms = new DTO.i.DTOs.EsActiuParms(esActiu: null);

            using var bl = BLCentres();
            var l =
                await
                bl
                .GetItems(parms)
                ;

            l
            .Data! // ToDo: gestionar broken rules
            .OrderBy(x=>x.Id) // for issue
            .Select(x => new CentreRowViewModel(x ))
            .ToList()
            .ForEach(x=>MyItems.Add(x));
        }
    }
}
