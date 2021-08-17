using System.Reactive;
using ReactiveUI;
using dtoo = DTO.o.DTOs;
using CommonInterfaces;
using System.Threading.Tasks;
using UI.ER.AvaloniaUI.Services;
using BusinessLayer.Abstract.Services;
using System.Reactive.Concurrency;
using dtoi = DTO.i.DTOs;
using UI.ER.ViewModels.Common;
using System.Linq;

namespace UI.ER.ViewModels.ViewModels
{
    public class TipusActuacioUpdateViewModel : ViewModelBase, IId
    {

        public TipusActuacioUpdateViewModel(int id)
        {
            Id = id;
            RxApp.MainThreadScheduler.Schedule(LoadData);

            SubmitCommand = ReactiveCommand.CreateFromTask(UpdateData);

        }
        public int Id { get; }
        public string IdTxt => $"TipusActuacio #{Id}";

        public string _Codi = string.Empty;
        public string Codi
        {
            get => _Codi;
            set
            {
                // if (string.IsNullOrWhiteSpace(value))
                // {
                //     throw new NotifyDataErrorInfo("Aquest camp no pot quedat buit.");
                // }

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
            // Clear brokenRules
            BrokenRules.Clear();

            // Backend request
            using var bl = SuperContext.GetBLOperation<ITipusActuacioSet>();
            var dto = await bl.FromId(Id);

            // Update UI
            BrokenRules.AddRange(dto.BrokenRules.Select(x => x.Message));
            DTO2ModelView(dto.Data);
        }

        private void DTO2ModelView(dtoo.TipusActuacio? data)
        {
            if (data == null) return;

            Codi = data.Codi;
            Nom = data.Nom;
            EsActiu = data.EsActiu;
        }

        public virtual async Task<dtoo.TipusActuacio?> UpdateData()
        {
            // Clear brokenRules
            BrokenRules.Clear();

            // preparar paràmetres
            var parms = new dtoi.TipusActuacioUpdateParms(Id, Codi, Nom, EsActiu);

            // cridar backend
            using var bl = SuperContext.GetBLOperation<ITipusActuacioUpdate>();
            var dto = await bl.Update(parms);
            var data = dto.Data;

            // actualitzar dades amb el resultat
            DTO2ModelView(data);
            BrokenRules.AddRange(dto.BrokenRules.Select(x => x.Message));

            //
            return data;
        }

        public RangeObservableCollection<string> BrokenRules { get; } = new();

        public ReactiveCommand<Unit, dtoo.TipusActuacio?> SubmitCommand { get; }


    }
}