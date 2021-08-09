using System.Reactive;
using ReactiveUI;
using dtoo = DTO.o.DTOs;
using CommonInterfaces;
using System.Threading.Tasks;
using ER.AvaloniaUI.Services;
using BusinessLayer.Abstract.Services;
using System.Reactive.Concurrency;
using Avalonia.Data;

namespace ER.AvaloniaUI.ViewModels
{
    public class CentreUpdateViewModel : ViewModelBase, IId
    {

        protected virtual ICentreUpdate BLUpdate() => SuperContext.GetBLOperation<ICentreUpdate>();
        protected virtual ICentreGet BLGet() => SuperContext.GetBLOperation<ICentreGet>();
        public CentreUpdateViewModel(int id)
        {
            Id = id;
            RxApp.MainThreadScheduler.Schedule(LoadData);    
        }
        public int Id {get;}
        public string IdTxt => $"Centre #{Id}";

        public string _Codi = string.Empty;
        public string Codi 
        {
            get => _Codi;
            set
            {
                if (string.IsNullOrWhiteSpace(value) )
                {
                    throw new DataValidationException("Aquest camp no pot quedat buit.");
                }

                this.RaiseAndSetIfChanged(ref _Codi, value);
            }
        }
        public string _Nom = string.Empty;
        public string Nom 
        {
            get => _Nom;
            set => this.RaiseAndSetIfChanged(ref _Nom, value);
        }

        private bool _EsActiu;
        public bool EsActiu
        {
            get { return _EsActiu; }
            protected set { this.RaiseAndSetIfChanged(ref _EsActiu, value); }
        }

        protected virtual async void LoadData()
        {
            using var bl = BLGet();
            var data =
                (await
                bl
                .FromId(Id)
                )
                .Data!;
            Codi = data.Nom;
            Nom = data.Codi;
            EsActiu = data.EsActiu;

                

        }
    }
}