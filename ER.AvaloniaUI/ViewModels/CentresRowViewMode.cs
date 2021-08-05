using System.Reactive;
using ReactiveUI;
using dtoo = DTO.o.DTOs;
using CommonInterfaces;
using System.Threading.Tasks;
using ER.AvaloniaUI.Services;
using BusinessLayer.Abstract.Services;

namespace ER.AvaloniaUI.ViewModels
{
    public class CentresRowViewMode : ViewModelBase, IEtiquetaDescripcio, IId
    {

        public CentresRowViewMode(dtoo.Centre centreDto)
        {
            Etiqueta = centreDto.Etiqueta;
            Descripcio = centreDto.Descripcio;
            _Estat = centreDto.EsActiu ? "Activat" : "Desactivat";
            Id = centreDto.Id;
            DoTheThing = ReactiveCommand.CreateFromTask( RunTheThing ); 
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
        public int Id {get;}

        protected async Task RunTheThing()  
        {
            using  var bl = SuperContext.GetBLOperation<ICentreActivaDesactiva>();

            var centreDto = ( await bl.Toggle(Id)).Data!;

            Etiqueta = centreDto.Etiqueta;
            Descripcio = centreDto.Descripcio;
            Estat = centreDto.EsActiu ? "Activat" : "Desactivat";
        }

    }
}
