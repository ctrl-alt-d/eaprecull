using BusinessLayer.Abstract;
using BusinessLayer.Abstract.Services;
using ReactiveUI;
using Dtoo = DTO.o.DTOs;
using Dtoi = DTO.i.DTOs;
using System.Reactive.Linq;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using BusinessLayer.Abstract.Generic;
using UI.ER.ViewModels.ViewModels.Base;

namespace UI.ER.ViewModels.ViewModels
{

    public class TipusActuacioSetViewModel
        : SetViewModelBase<TipusActuacioRowViewModel, Dtoo.TipusActuacio>,
          ISetViewModel<TipusActuacioCreateViewModel, Dtoo.TipusActuacio>
    {
        public TipusActuacioSetViewModel(IServiceFactory serveis, bool modeLookup = false)
            : base(serveis, modeLookup)
        {
            // Filtre
            this
                .WhenAnyValue(x => x.NomesActius)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => CarregaAra())
                ;

            // Create
            ShowDialog = new Interaction<TipusActuacioCreateViewModel, Dtoo.TipusActuacio?>();

            Create = ReactiveCommand.CreateFromTask(async () =>
            {
                var update = new TipusActuacioCreateViewModel(Serveis);

                var data = await ShowDialog.Handle(update);

                if (data != null)
                    MyItems.Insert(0, CreaFila(data));
            });


        }

        protected override async Task<OperationResults<Dtoo.TipusActuacio>> Consulta()
        {
            // Preparar paràmetres al backend
            var esActiu = NomesActius ? true : (bool?)null;
            var Parms = new Dtoi.EsActiuParms(esActiu: esActiu);

            // Petició al backend
            using var bl = Serveis.GetBLOperation<ITipusActuacioSet>();
            return await bl.FromPredicate(Parms);
        }

        protected override TipusActuacioRowViewModel CreaFila(Dtoo.TipusActuacio dto)
            => new(Serveis, dto, ModeLookup);

        // Filtre
        private bool _NomesActius = true;
        public bool NomesActius
        {
            get => _NomesActius;
            set => this.RaiseAndSetIfChanged(ref _NomesActius, value);
        }

        // Crear item
        public ICommand Create { get; }
        public Interaction<TipusActuacioCreateViewModel, Dtoo.TipusActuacio?> ShowDialog { get; }


    }
}
