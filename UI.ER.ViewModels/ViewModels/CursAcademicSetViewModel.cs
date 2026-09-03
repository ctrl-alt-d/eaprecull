using System.Linq;
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

    public class CursAcademicSetViewModel
        : SetViewModelBase<CursAcademicRowViewModel, Dtoo.CursAcademic>,
          ISetViewModel<CursAcademicCreateViewModel, Dtoo.CursAcademic>
    {
        public CursAcademicSetViewModel(IServiceFactory serveis, bool modeLookup = false)
            : base(serveis, modeLookup)
        {
            // Filtre
            this
                .WhenAnyValue(x => x.NomesActius)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => CarregaAra())
                ;

            // Create
            ShowDialog = new Interaction<CursAcademicCreateViewModel, Dtoo.CursAcademic?>();

            Create = ReactiveCommand.CreateFromTask(async () =>
            {
                var update = new CursAcademicCreateViewModel(Serveis);

                var data = await ShowDialog.Handle(update);

                if (data != null)
                {
                    MyItems.Insert(0, CreaFila(data));

                    // Si el nou curs és actiu, desactivar tots els altres a la UI
                    if (data.EsActiu)
                    {
                        foreach (var curs in MyItems.Where(x => x.Id != data.Id))
                        {
                            curs.EsActiu = false;
                            curs.Estat = "Desactivat";
                        }
                    }
                }
            });


        }

        protected override async Task<OperationResults<Dtoo.CursAcademic>> Consulta()
        {
            // Preparar paràmetres al backend
            var esActiu = NomesActius ? true : (bool?)null;
            var Parms = new Dtoi.EsActiuParms(esActiu: esActiu);

            // Petició al backend
            using var bl = Serveis.GetBLOperation<ICursAcademicSet>();
            return await bl.FromPredicate(Parms);
        }

        protected override CursAcademicRowViewModel CreaFila(Dtoo.CursAcademic dto)
            => new(Serveis, dto, MyItems, ModeLookup);

        // Filtre
        private bool _NomesActius = false;
        public bool NomesActius
        {
            get => _NomesActius;
            set => this.RaiseAndSetIfChanged(ref _NomesActius, value);
        }

        // Crear item
        public ICommand Create { get; }
        public Interaction<CursAcademicCreateViewModel, Dtoo.CursAcademic?> ShowDialog { get; }


    }
}
