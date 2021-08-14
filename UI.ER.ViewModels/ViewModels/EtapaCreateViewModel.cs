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
    public class EtapaCreateViewModel : ViewModelBase
    {

        protected virtual IEtapaCreate BLCreate() => SuperContext.GetBLOperation<IEtapaCreate>();
        public EtapaCreateViewModel()
        {
            SubmitCommand = ReactiveCommand.CreateFromTask(() => CreateData());
        }

        public string _Codi = string.Empty;
        public string Codi
        {
            get => _Codi;
            set => this.RaiseAndSetIfChanged(ref _Codi, value);
        }
        public string _Nom = string.Empty;
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

        private void DTO2ModelView(dtoo.Etapa? data)
        {
            if (data==null) return;

            Codi = data.Codi;
            Nom = data.Nom;
        }

        public virtual async Task<dtoo.Etapa?> CreateData()
        {
            BrokenRules.Clear();

            // preparar paràmetres
            var parms = new dtoi.EtapaCreateParms(Codi, Nom, SonEstudisObligatoris, true);

            // cridar backend
            using var bl = BLCreate();
            var dto = await bl.Create(parms);
            var data = dto.Data;

            // actualitzar dades amb el resultat
            DTO2ModelView(data);
            BrokenRules.AddRange(dto.BrokenRules.Select(x=>x.Message));

            SuccessfullySaved = data != null && !dto.BrokenRules.Any();

            return data;
        }

        public RangeObservableCollection<string> BrokenRules { get; } = new();

        public ReactiveCommand<Unit, dtoo.Etapa?> SubmitCommand { get; }

        private bool _Sortir;
        public bool SuccessfullySaved
        {
            get { return _Sortir; }
            protected set { this.RaiseAndSetIfChanged(ref _Sortir, value); }
        }
       

    }
}