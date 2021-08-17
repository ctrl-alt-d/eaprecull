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
using System;
using DynamicData.Binding;

namespace UI.ER.ViewModels.ViewModels
{
    public class CursAcademicCreateViewModel : ViewModelBase
    {

        public CursAcademicCreateViewModel()
        {
            SubmitCommand = ReactiveCommand.CreateFromTask(CreateData);
        }

                public double _AnyInici;
        public double AnyInici
        {
            get => _AnyInici;
            set => this.RaiseAndSetIfChanged(ref _AnyInici, value);
        }

        private void DTO2ModelView(dtoo.CursAcademic? data)
        {
            if (data == null) return;

            AnyInici = data.AnyInici;
        }
        private void BrokenRules2ModelView(List<BrokenRule> brokenRules)
        {
            BrokenRules.Clear();
            BrokenRules.AddRange(brokenRules.Select(x => x.Message));
        }
        public virtual async Task<dtoo.CursAcademic?> CreateData()
        {
            BrokenRules.Clear();

            // preparar paràmetres
            var parms = new dtoi.CursAcademicCreateParms( Convert.ToInt32(AnyInici), true);

            // cridar backend
            using var bl = SuperContext.GetBLOperation<ICursAcademicCreate>();
            var dto = await bl.Create(parms);
            var data = dto.Data;

            // actualitzar dades amb el resultat
            DTO2ModelView(data);
            BrokenRules2ModelView(dto.BrokenRules);

            return data;
        }

        public ObservableCollectionExtended<string> BrokenRules { get; } = new();

        public ReactiveCommand<Unit, dtoo.CursAcademic?> SubmitCommand { get; }



    }
}