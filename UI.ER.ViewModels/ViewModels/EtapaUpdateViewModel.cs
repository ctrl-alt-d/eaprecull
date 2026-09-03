using System.Reactive;
using ReactiveUI;
using Dtoo = DTO.o.DTOs;
using CommonInterfaces;
using System.Threading.Tasks;
using BusinessLayer.Abstract.Services;
using System.Reactive.Concurrency;
using Dtoi = DTO.i.DTOs;
using System.Linq;
using DynamicData.Binding;
using BusinessLayer.Abstract.Generic;

namespace UI.ER.ViewModels.ViewModels
{
    public class EtapaUpdateViewModel : ViewModelBase, ISubmitViewModel<Dtoo.Etapa>, IId
    {

        private readonly IServiceFactory _serveis;

        public EtapaUpdateViewModel(IServiceFactory serveis, int id)
        {
            _serveis = serveis;
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
            using var bl = _serveis.GetBLOperation<IEtapaSet>();
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
            using var bl = _serveis.GetBLOperation<IEtapaUpdate>();
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