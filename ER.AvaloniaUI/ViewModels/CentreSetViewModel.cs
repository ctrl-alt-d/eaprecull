using System.Linq;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using BusinessLayer.Abstract.Services;
using ReactiveUI;
using dtoo = DTO.o.DTOs;

namespace ER.AvaloniaUI.ViewModels
{
    public class CentreSetViewModel : ViewModelBase
    {
        private readonly ICentreGetSet BLCentres;
        private readonly ICentreActivaDesactiva BLActivaDesactiva;
        public CentreSetViewModel(ICentreGetSet blcentres, ICentreActivaDesactiva blActivaDesactiva)
        {
            BLCentres = blcentres;
            BLActivaDesactiva = blActivaDesactiva;
            RxApp.MainThreadScheduler.Schedule(LoadCentres);    
        }
        public ObservableCollection<CentreRowViewModel> MyItems {get;} = new();

        private async void LoadCentres()
        {
            var parms = new DTO.i.DTOs.EsActiuParms(esActiu: null);
            var l =
                await
                BLCentres
                .GetItems(parms)
                ;

            l.Data!
            .Select(x => new CentreRowViewModel(x ))
            .ToList()
            .ForEach(x=>MyItems.Add(x));
        }
    }
}
