using System.Reactive;
using ReactiveUI;
using Dtoo = DTO.o.DTOs;
using CommonInterfaces;
using System.Threading.Tasks;
using BusinessLayer.Abstract.Services;
using System.Reactive.Concurrency;
using Dtoi = DTO.i.DTOs;
using System.Linq;
using System;
using DynamicData.Binding;
using BusinessLayer.Abstract.Generic;

namespace UI.ER.ViewModels.ViewModels
{
    public class CursAcademicUpdateViewModel : ViewModelBase, ISubmitViewModel<Dtoo.CursAcademic>, IId
    {

        private readonly IServiceFactory _serveis;

        public CursAcademicUpdateViewModel(IServiceFactory serveis, int id)
        {
            _serveis = serveis;
            Id = id;
            RxApp.MainThreadScheduler.Schedule(LoadData);

            SubmitCommand = ReactiveCommand.CreateFromTask(UpdateData);

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
            using var bl = _serveis.GetBLOperation<ICursAcademicSet>();
            var dto = await bl.FromId(Id);

            // Update UI
            BrokenRules.AddRange(dto.BrokenRules.Select(x => x.Message));
            DTO2ModelView(dto.Data);
        }

        private void DTO2ModelView(Dtoo.CursAcademic? data)
        {
            if (data == null) return;

            AnyInici = data.AnyInici;
            EsActiu = data.EsActiu;
        }

        public virtual async Task<Dtoo.CursAcademic?> UpdateData()
        {
            // Clear brokenRules
            BrokenRules.Clear();

            // preparar paràmetres
            var Parms = new Dtoi.CursAcademicUpdateParms(Id, Convert.ToInt32(AnyInici), EsActiu);

            // cridar backend
            using var bl = _serveis.GetBLOperation<ICursAcademicUpdate>();
            var dto = await bl.Update(Parms);
            var data = dto.Data;

            // actualitzar dades amb el resultat
            DTO2ModelView(data);
            BrokenRules.AddRange(dto.BrokenRules.Select(x => x.Message));

            //
            return data;
        }

        public ObservableCollectionExtended<string> BrokenRules { get; } = new();

        public ReactiveCommand<Unit, Dtoo.CursAcademic?> SubmitCommand { get; }


    }
}