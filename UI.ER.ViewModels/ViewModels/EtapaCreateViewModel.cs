
using System.Reactive;
using ReactiveUI;
using dtoo = DTO.o.DTOs;
using System.Threading.Tasks;
using UI.ER.AvaloniaUI.Services;
using BusinessLayer.Abstract.Services;
using dtoi = DTO.i.DTOs;
using UI.ER.ViewModels.Common;
using System.Linq;
using System.Collections.Generic;
using BusinessLayer.Abstract.Exceptions;

namespace UI.ER.ViewModels.ViewModels
{
    public class EtapaCreateViewModel : ViewModelBase
    {

        public EtapaCreateViewModel()
        {
            SubmitCommand = ReactiveCommand.CreateFromTask(CreateData);
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
            if (data == null) return;

            Codi = data.Codi;
            Nom = data.Nom;
            SonEstudisObligatoris = data.SonEstudisObligatoris;
        }
        private void BrokenRules2ModelView(List<BrokenRule> brokenRules)
        {
            BrokenRules.ClearSilently();
            BrokenRules.AddRange(brokenRules.Select(x => x.Message));
        }
        public virtual async Task<dtoo.Etapa?> CreateData()
        {
            BrokenRules.Clear();

            // preparar paràmetres
            var parms = new dtoi.EtapaCreateParms(Codi, Nom, SonEstudisObligatoris, true);

            // cridar backend
            using var bl = SuperContext.GetBLOperation<IEtapaCreate>();
            var dto = await bl.Create(parms);
            var data = dto.Data;

            // actualitzar dades amb el resultat
            DTO2ModelView(data);
            BrokenRules2ModelView(dto.BrokenRules);

            return data;
        }

        public RangeObservableCollection<string> BrokenRules { get; } = new();

        public ReactiveCommand<Unit, dtoo.Etapa?> SubmitCommand { get; }



    }
}
