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
    public class CursAcademicUpdateViewModel : ViewModelBase, IId
    {

        protected virtual ICursAcademicUpdate BLUpdate() => SuperContext.GetBLOperation<ICursAcademicUpdate>();
        protected virtual ICursAcademicGetSet BLGet() => SuperContext.GetBLOperation<ICursAcademicGetSet>();
        public CursAcademicUpdateViewModel(int id)
        {
            Id = id;
            RxApp.MainThreadScheduler.Schedule(LoadData);

            SubmitCommand = ReactiveCommand.CreateFromTask(() => UpdateData());

        }
        public int Id { get; }
        public string IdTxt => $"CursAcademic #{Id}";

        public double _AnyInici;
        public double AnyInici
        {
            get => _AnyInici;
            set => this.RaiseAndSetIfChanged(ref _AnyInici, value);
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
            using var bl = BLGet();
            var dto = await bl.FromId(Id); 

            // Update UI
            BrokenRules.AddRange(dto.BrokenRules.Select(x=>x.Message));
            DTO2ModelView(dto.Data);
        }

        private void DTO2ModelView(dtoo.CursAcademic? data)
        {
            if (data==null) return;

            AnyInici = data.AnyInici;
            EsActiu = data.EsActiu;
        }

        public virtual async Task<dtoo.CursAcademic?> UpdateData()
        {
            // Clear brokenRules
            BrokenRules.Clear();

            // preparar paràmetres
            var parms = new dtoi.CursAcademicUpdateParms(Id, Convert.ToInt32(AnyInici), EsActiu);

            // cridar backend
            using var bl = BLUpdate();
            var dto = await bl.Update(parms);
            var data = dto.Data;

            // actualitzar dades amb el resultat
            DTO2ModelView(data);
            BrokenRules.AddRange(dto.BrokenRules.Select(x=>x.Message));

            // Close window?
            SuccessfullySaved = data != null && !dto.BrokenRules.Any();

            //
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