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
using System;

namespace UI.ER.ViewModels.ViewModels
{
    public class CursAcademicCreateViewModel : ViewModelBase
    {

        protected virtual ICursAcademicCreate BLCreate() => SuperContext.GetBLOperation<ICursAcademicCreate>();
        public CursAcademicCreateViewModel()
        {
            SubmitCommand = ReactiveCommand.CreateFromTask(() => CreateData());
        }

        public double _AnyInici;
        public double AnyInici
        {
            get => _AnyInici;
            set => this.RaiseAndSetIfChanged(ref _AnyInici, value);
        }

        private void DTO2ModelView(dtoo.CursAcademic? data)
        {
            if (data==null) return;

            AnyInici = data.AnyInici;
        }

        public virtual async Task<dtoo.CursAcademic?> CreateData()
        {
            BrokenRules.Clear();

            // preparar paràmetres
            var parms = new dtoi.CursAcademicCreateParms( Convert.ToInt32(AnyInici), true);

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

        public ReactiveCommand<Unit, dtoo.CursAcademic?> SubmitCommand { get; }

        private bool _Sortir;
        public bool SuccessfullySaved
        {
            get { return _Sortir; }
            protected set { this.RaiseAndSetIfChanged(ref _Sortir, value); }
        }
       

    }
}