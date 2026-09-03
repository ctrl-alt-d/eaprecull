
using System.Reactive;
using ReactiveUI;
using Dtoo = DTO.o.DTOs;
using System.Threading.Tasks;
using BusinessLayer.Abstract.Services;
using Dtoi = DTO.i.DTOs;
using System.Linq;
using System.Collections.Generic;
using BusinessLayer.Abstract.Exceptions;
using DynamicData.Binding;
using BusinessLayer.Abstract.Generic;

namespace UI.ER.ViewModels.ViewModels
{
    public class EtapaCreateViewModel : ViewModelBase, ISubmitViewModel<Dtoo.Etapa>
    {

        private readonly IServiceFactory _serveis;

        public EtapaCreateViewModel(IServiceFactory serveis)
        {
            _serveis = serveis;

            SubmitCommand = ReactiveCommand.CreateFromTask(CreateData);
        }

        private string _Codi = string.Empty;
        public string Codi
        {
            get => _Codi;
            set => this.RaiseAndSetIfChanged(ref _Codi, value);
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

        private void DTO2ModelView(Dtoo.Etapa? data)
        {
            if (data == null) return;

            Codi = data.Codi;
            Nom = data.Nom;
            SonEstudisObligatoris = data.SonEstudisObligatoris;
        }
        private void BrokenRules2ModelView(List<BrokenRule> brokenRules)
        {
            BrokenRules.Clear();
            BrokenRules.AddRange(brokenRules.Select(x => x.Message));
        }
        public virtual async Task<Dtoo.Etapa?> CreateData()
        {
            BrokenRules.Clear();

            // preparar paràmetres
            var Parms = new Dtoi.EtapaCreateParms(Codi, Nom, SonEstudisObligatoris, true);

            // cridar backend
            using var bl = _serveis.GetBLOperation<IEtapaCreate>();
            var dto = await bl.Create(Parms);
            var data = dto.Data;

            // actualitzar dades amb el resultat
            DTO2ModelView(data);
            BrokenRules2ModelView(dto.BrokenRules);

            return data;
        }

        public ObservableCollectionExtended<string> BrokenRules { get; } = new();

        public ReactiveCommand<Unit, Dtoo.Etapa?> SubmitCommand { get; }



    }
}
