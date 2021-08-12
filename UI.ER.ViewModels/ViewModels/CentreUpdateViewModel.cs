using System.Reactive;
using ReactiveUI;
using dtoo = DTO.o.DTOs;
using CommonInterfaces;
using System.Threading.Tasks;
using UI.ER.AvaloniaUI.Services;
using BusinessLayer.Abstract.Services;
using System.Reactive.Concurrency;
using dtoi = DTO.i.DTOs;
using System.ComponentModel;
using UI.ER.ViewModels.Common;
using System.Linq;

namespace UI.ER.ViewModels.ViewModels
{
    public class CentreUpdateViewModel : ViewModelBase, IId
    {

        protected virtual ICentreUpdate BLUpdate() => SuperContext.GetBLOperation<ICentreUpdate>();
        protected virtual ICentreGet BLGet() => SuperContext.GetBLOperation<ICentreGet>();
        public CentreUpdateViewModel(int id)
        {
            Id = id;
            RxApp.MainThreadScheduler.Schedule(LoadData);

            SubmitCommand = ReactiveCommand.CreateFromTask(() => UpdateData());

        }
        public int Id { get; }
        public string IdTxt => $"Centre #{Id}";

        public string _Codi = string.Empty;
        public string Codi
        {
            get => _Codi;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new NotifyDataErrorInfo("Aquest camp no pot quedat buit.");
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
                .Data!; // ToDo: deal with br
            DTO2ModelView(data);
        }

        private void DTO2ModelView(dtoo.Centre? data)
        {
            if (data==null) return;

            Codi = data.Codi;
            Nom = data.Nom;
            EsActiu = data.EsActiu;
        }

        public virtual async Task<dtoo.Centre?> UpdateData()
        {
            BrokenRules.Clear();

            // preparar paràmetres
            var parms = new dtoi.CentreUpdateParms(Id, Codi, Nom, EsActiu);

            // cridar backend
            using var bl = BLUpdate();
            var dto = await bl.Update(parms);
            var data = dto.Data;

            // actualitzar dades amb el resultat
            DTO2ModelView(data);
            BrokenRules.AddRange(dto.BrokenRules.Select(x=>x.Message));

            Sortir = data != null && !dto.BrokenRules.Any();

            return data;
        }

        public RangeObservableCollection<string> BrokenRules { get; } = new();

        public ReactiveCommand<Unit, dtoo.Centre?> SubmitCommand { get; }

        private bool _Sortir;
        public bool Sortir
        {
            get { return _Sortir; }
            protected set { this.RaiseAndSetIfChanged(ref _Sortir, value); }
        }
       

    }
}