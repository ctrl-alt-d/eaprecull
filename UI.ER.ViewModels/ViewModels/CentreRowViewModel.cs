using System.Reactive;
using ReactiveUI;
using dtoo = DTO.o.DTOs;
using CommonInterfaces;
using System.Threading.Tasks;
using UI.ER.AvaloniaUI.Services;
using BusinessLayer.Abstract.Services;
using BusinessLayer.Abstract;
using System.Windows.Input;
using System.Reactive.Linq;
using System;

namespace UI.ER.ViewModels.ViewModels
{
    public class CentreRowViewModel : ViewModelBase, IEtiquetaDescripcio, IId
    {

        public CentreRowViewModel(dtoo.Centre centreDto)
        {
            _Etiqueta = centreDto.Etiqueta;
            _Descripcio = centreDto.Descripcio;
            _Estat = centreDto.EsActiu ? "Activat" : "Desactivat";
            _EsActiu = centreDto.EsActiu;
            _ClasseAmagat = centreDto.EsActiu ? true : false;
            _ClasseVisible = centreDto.EsActiu;
            Id = centreDto.Id;
            DoTheThing = ReactiveCommand.CreateFromTask( RunTheThing );

            // ----
            ShowDialog = new Interaction<CentreUpdateViewModel, dtoo.Centre?>();

            Update = ReactiveCommand.CreateFromTask(async () =>
            {
                var update = new CentreUpdateViewModel(Id);

                var data = await ShowDialog.Handle(update);

                if (data != null) DTO2ModelView(data);
            });

        }

        internal void NomesActius(bool nomesActius)
        {
            ClasseDesapareix = nomesActius && !EsActiu;
            ClasseApareix = !nomesActius && !EsActiu;
            ClasseAmagat = false;
            ClasseVisible = EsActiu;
        }

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
            using var bl = SuperContext.GetBLOperation<ICentreActivaDesactiva>();

            var data = (await bl.Toggle(Id)).Data!;

            DTO2ModelView(data);
        }

        private void DTO2ModelView(dtoo.Centre data)
        {
            Etiqueta = data.Etiqueta;
            Descripcio = data.Descripcio;
            Estat = data.EsActiu ? "Activat" : "Desactivat";
            EsActiu = data.EsActiu;
        }

        // ----------------------
        public ICommand Update { get; }
        public Interaction<CentreUpdateViewModel, dtoo.Centre?> ShowDialog { get; }

        // 
        private bool _ClasseAmagat;
        public bool ClasseAmagat
        {
            get { return _ClasseAmagat; }
            set { this.RaiseAndSetIfChanged(ref _ClasseAmagat, value); }
        }

        private bool _ClasseVisible;
        public bool ClasseVisible
        {
            get { return _ClasseVisible; }
            set { this.RaiseAndSetIfChanged(ref _ClasseVisible, value); }
        }


        private bool _ClasseApareix = false;
        public bool ClasseApareix
        {
            get { return _ClasseApareix; }
            set { this.RaiseAndSetIfChanged(ref _ClasseApareix, value); }
        }

        private bool _ClasseDesapareix = false;
        public bool ClasseDesapareix
        {
            get { return _ClasseDesapareix; }
            set { this.RaiseAndSetIfChanged(ref _ClasseDesapareix, value); }
        }
    }
}
