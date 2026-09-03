using System.Reactive;
using ReactiveUI;
using Dtoo = DTO.o.DTOs;
using System.Threading.Tasks;
using BusinessLayer.Abstract.Services;
using Dtoi = DTO.i.DTOs;
using System.Linq;
using System.Collections.Generic;
using BusinessLayer.Abstract.Exceptions;
using System;
using DynamicData.Binding;
using BusinessLayer.Abstract.Generic;

namespace UI.ER.ViewModels.ViewModels
{
    public class CursAcademicCreateViewModel : ViewModelBase, ISubmitViewModel<Dtoo.CursAcademic>
    {

        private readonly IServiceFactory _serveis;

        public CursAcademicCreateViewModel(IServiceFactory serveis)
        {
            _serveis = serveis;

            SubmitCommand = ReactiveCommand.CreateFromTask(CreateData);
        }

        public double _AnyInici;
        public double AnyInici
        {
            get => _AnyInici;
            set => this.RaiseAndSetIfChanged(ref _AnyInici, value);
        }

        private void DTO2ModelView(Dtoo.CursAcademic? data)
        {
            if (data == null) return;

            AnyInici = data.AnyInici;
        }
        private void BrokenRules2ModelView(List<BrokenRule> brokenRules)
        {
            BrokenRules.Clear();
            BrokenRules.AddRange(brokenRules.Select(x => x.Message));
        }
        public virtual async Task<Dtoo.CursAcademic?> CreateData()
        {
            BrokenRules.Clear();

            // preparar paràmetres
            var Parms = new Dtoi.CursAcademicCreateParms(Convert.ToInt32(AnyInici), true);

            // cridar backend
            using var bl = _serveis.GetBLOperation<ICursAcademicCreate>();
            var dto = await bl.Create(Parms);
            var data = dto.Data;

            // actualitzar dades amb el resultat
            DTO2ModelView(data);
            BrokenRules2ModelView(dto.BrokenRules);

            return data;
        }

        public ObservableCollectionExtended<string> BrokenRules { get; } = new();

        public ReactiveCommand<Unit, Dtoo.CursAcademic?> SubmitCommand { get; }



    }
}