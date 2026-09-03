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

    public class EtapaSetViewModel
        : SetViewModelBase<EtapaRowViewModel, Dtoo.Etapa>,
          ISetViewModel<EtapaCreateViewModel, Dtoo.Etapa>
    {
        public EtapaSetViewModel(IServiceFactory serveis, bool modeLookup = false)
            : base(serveis, modeLookup)
        {
            // Filtre
            this
                .WhenAnyValue(x => x.NomesActius)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => CarregaAra())
                ;

            // Create
            ShowDialog = new Interaction<EtapaCreateViewModel, Dtoo.Etapa?>();

            Create = ReactiveCommand.CreateFromTask(async () =>
            {
                var update = new EtapaCreateViewModel(Serveis);

                var data = await ShowDialog.Handle(update);

                if (data != null)
                    MyItems.Insert(0, CreaFila(data));
            });


        }

        protected override async Task<OperationResults<Dtoo.Etapa>> Consulta()
        {
            // Preparar paràmetres al backend
            var esActiu = NomesActius ? true : (bool?)null;
            var Parms = new Dtoi.EsActiuParms(esActiu: esActiu);

            // Petició al backend
            using var bl = Serveis.GetBLOperation<IEtapaSet>();
            return await bl.FromPredicate(Parms);
        }

        protected override EtapaRowViewModel CreaFila(Dtoo.Etapa dto)
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
        public Interaction<EtapaCreateViewModel, Dtoo.Etapa?> ShowDialog { get; }


    }
}
