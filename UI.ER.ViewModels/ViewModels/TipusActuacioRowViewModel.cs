using System.Reactive;
using ReactiveUI;
using dtoo = DTO.o.DTOs;
using CommonInterfaces;
using System.Threading.Tasks;
using UI.ER.AvaloniaUI.Services;
using BusinessLayer.Abstract.Services;
using System.Windows.Input;
using System.Reactive.Linq;
using System;

namespace UI.ER.ViewModels.ViewModels
{
    public class TipusActuacioRowViewModel : ViewModelBase, IEtiquetaDescripcio, IId
    {

        protected dtoo.TipusActuacio Model {get;}
        public TipusActuacioRowViewModel(dtoo.TipusActuacio TipusActuacioDto, Action<IIdEtiquetaDescripcio>? modeLookup = null)
        {

            ModeLookup = modeLookup;
            Model = TipusActuacioDto;
            _Etiqueta = TipusActuacioDto.Etiqueta;
            _Descripcio = TipusActuacioDto.Descripcio;
            _Estat = TipusActuacioDto.EsActiu ? "Activat" : "Desactivat";
            _EsActiu = TipusActuacioDto.EsActiu;
            Id = TipusActuacioDto.Id;
            DoTheThing = ReactiveCommand.CreateFromTask( RunTheThing );
            Seleccionar = ReactiveCommand.Create( RunSeleccionar );

            // ----
            ShowDialog = new Interaction<TipusActuacioUpdateViewModel, dtoo.TipusActuacio?>();

            Update = ReactiveCommand.CreateFromTask(async () =>
            {
                var update = new TipusActuacioUpdateViewModel(Id);

                var data = await ShowDialog.Handle(update);

                if (data != null) DTO2ModelView(data);
            });

        }

        public Action<IIdEtiquetaDescripcio>? ModeLookup {get; }
        public bool ModeLookupActivat => ModeLookup != null;
        public ReactiveCommand<Unit, Unit> DoTheThing { get; } 

        private string _Etiqueta = string.Empty;
        public string Etiqueta
        {
            get { return _Etiqueta; }
            protected set { this.RaiseAndSetIfChanged(ref _Etiqueta, value); }
        }

        private string _Estat = string.Empty;
        public string Estat
        {
            get { return _Estat; }
            protected set { this.RaiseAndSetIfChanged(ref _Estat, value); }
        }

        private string _Descripcio = string.Empty;
        public string Descripcio
        {
            get { return _Descripcio; }
            protected set { this.RaiseAndSetIfChanged(ref _Descripcio, value); }
        }

        private bool _EsActiu;
        public bool EsActiu
        {
            get { return _EsActiu; }
            protected set { this.RaiseAndSetIfChanged(ref _EsActiu, value); }
        }

        public int Id {get;}

        protected async Task RunTheThing()
        {
            using var bl = SuperContext.GetBLOperation<ITipusActuacioActivaDesactiva>();

            var data = (await bl.Toggle(Id)).Data!;

            DTO2ModelView(data);
        }

        private void DTO2ModelView(dtoo.TipusActuacio data)
        {
            Etiqueta = data.Etiqueta;
            Descripcio = data.Descripcio;
            Estat = data.EsActiu ? "Activat" : "Desactivat";
            EsActiu = data.EsActiu;
        }

        // ----------------------
        public ICommand Update { get; }
        public Interaction<TipusActuacioUpdateViewModel, dtoo.TipusActuacio?> ShowDialog { get; }

        // -----------------------
        public ReactiveCommand<Unit, Unit> Seleccionar { get; } 
        protected void RunSeleccionar()
        {
            ModeLookup?.Invoke(Model);
        }


    }
}
