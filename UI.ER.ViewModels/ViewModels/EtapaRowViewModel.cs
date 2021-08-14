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
    public class EtapaRowViewModel : ViewModelBase, IEtiquetaDescripcio, IId
    {

        protected dtoo.Etapa Model {get;}
        public EtapaRowViewModel(dtoo.Etapa EtapaDto, Action<IIdEtiquetaDescripcio>? modeLookup = null)
        {

            ModeLookup = modeLookup;
            Model = EtapaDto;
            _Etiqueta = EtapaDto.Etiqueta;
            _Descripcio = EtapaDto.Descripcio;
            _Estat = EtapaDto.EsActiu ? "Activat" : "Desactivat";
            _EsActiu = EtapaDto.EsActiu;
            Id = EtapaDto.Id;
            DoTheThing = ReactiveCommand.CreateFromTask( RunTheThing );
            Seleccionar = ReactiveCommand.Create( RunSeleccionar );

            // ----
            ShowDialog = new Interaction<EtapaUpdateViewModel, dtoo.Etapa?>();

            Update = ReactiveCommand.CreateFromTask(async () =>
            {
                var update = new EtapaUpdateViewModel(Id);

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
            using var bl = SuperContext.GetBLOperation<IEtapaActivaDesactiva>();

            var data = (await bl.Toggle(Id)).Data!;

            DTO2ModelView(data);
        }

        private void DTO2ModelView(dtoo.Etapa data)
        {
            Etiqueta = data.Etiqueta;
            Descripcio = data.Descripcio;
            Estat = data.EsActiu ? "Activat" : "Desactivat";
            EsActiu = data.EsActiu;
        }

        // ----------------------
        public ICommand Update { get; }
        public Interaction<EtapaUpdateViewModel, dtoo.Etapa?> ShowDialog { get; }

        // -----------------------
        public ReactiveCommand<Unit, Unit> Seleccionar { get; } 
        protected void RunSeleccionar()
        {
            ModeLookup?.Invoke(Model);
        }


    }
}
