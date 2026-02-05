using System.Reactive;
using ReactiveUI;
using Dtoo = DTO.o.DTOs;
using CommonInterfaces;
using System.Threading.Tasks;
using UI.ER.AvaloniaUI.Services;
using BusinessLayer.Abstract.Services;
using System.Reactive.Concurrency;
using Dtoi = DTO.i.DTOs;
using UI.ER.ViewModels.Common;
using System.Linq;
using DynamicData.Binding;

namespace UI.ER.ViewModels.ViewModels
{
    public class EtapaUpdateViewModel : ViewModelBase, IId
    {

        public EtapaUpdateViewModel(int id)
        {
            Id = id;
            RxApp.MainThreadScheduler.Schedule(LoadData);

            SubmitCommand = ReactiveCommand.CreateFromTask(UpdateData);

        }
        public int Id { get; }
        public string IdTxt => $"Etapa #{Id}";

        private string _Codi = string.Empty;
        public string Codi
        {
            get => _Codi;
            set
            {
                this.RaiseAndSetIfChanged(ref _Codi, value);
            }
        }
        private string _Nom = string.Empty;
        public string Nom
        {
            get => _Nom;
            set => this.RaiseAndSetIfChanged(ref _Nom, value);
        }

        public bool _SonEstudisObligatoris;
        public bool SonEstudisObligatoris
        {
            get => _SonEstudisObligatoris;
            set => this.RaiseAndSetIfChanged(ref _SonEstudisObligatoris, value);
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
            using var bl = SuperContext.GetBLOperation<IEtapaSet>();
            var dto = await bl.FromId(Id);

            // Update UI
            BrokenRules.AddRange(dto.BrokenRules.Select(x => x.Message));
            DTO2ModelView(dto.Data);
        }

        private void DTO2ModelView(Dtoo.Etapa? data)
        {
            if (data == null) return;

            Codi = data.Codi;
            Nom = data.Nom;
            SonEstudisObligatoris = data.SonEstudisObligatoris;
            EsActiu = data.EsActiu;
        }

        public virtual async Task<Dtoo.Etapa?> UpdateData()
        {
            // Clear brokenRules
            BrokenRules.Clear();

            // preparar paràmetres
            var Parms = new Dtoi.EtapaUpdateParms(Id, Codi, Nom, SonEstudisObligatoris, EsActiu);

            // cridar backend
            using var bl = SuperContext.GetBLOperation<IEtapaUpdate>();
            var dto = await bl.Update(Parms);
            var data = dto.Data;

            // actualitzar dades amb el resultat
            DTO2ModelView(data);
            BrokenRules.AddRange(dto.BrokenRules.Select(x => x.Message));

            //
            return data;
        }

        public ObservableCollectionExtended<string> BrokenRules { get; } = new();

        public ReactiveCommand<Unit, Dtoo.Etapa?> SubmitCommand { get; }


    }
}