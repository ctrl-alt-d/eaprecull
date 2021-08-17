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
    public class CentreCreateViewModel : ViewModelBase
    {

        public CentreCreateViewModel()
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

        private void DTO2ModelView(dtoo.Centre? data)
        {
            if (data == null) return;

            Codi = data.Codi;
            Nom = data.Nom;
        }
        private void BrokenRules2ModelView(List<BrokenRule> brokenRules)
        {
            BrokenRules.ClearSilently();
            BrokenRules.AddRange(brokenRules.Select(x => x.Message));
        }
        public virtual async Task<dtoo.Centre?> CreateData()
        {
            BrokenRules.Clear();

            // preparar paràmetres
            var parms = new dtoi.CentreCreateParms(Codi, Nom, true);

            // cridar backend
            using var bl = SuperContext.GetBLOperation<ICentreCreate>();
            var dto = await bl.Create(parms);
            var data = dto.Data;

            // actualitzar dades amb el resultat
            DTO2ModelView(data);
            BrokenRules2ModelView(dto.BrokenRules);

            SuccessfullySaved = data != null && !dto.BrokenRules.Any();
            return data;
        }

        public RangeObservableCollection<string> BrokenRules { get; } = new();

        public ReactiveCommand<Unit, dtoo.Centre?> SubmitCommand { get; }

        private bool _Sortir;
        public bool SuccessfullySaved
        {
            get { return _Sortir; }
            protected set { this.RaiseAndSetIfChanged(ref _Sortir, value); }
        }


    }
}