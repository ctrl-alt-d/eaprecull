using System.Collections.ObjectModel;
using System.Reactive;
using System.Linq;
using System.Threading.Tasks;
using BusinessLayer.Abstract.Services;
using CommonInterfaces;
using UI.ER.AvaloniaUI.Services;
using ReactiveUI;
using dtoo = DTO.o.DTOs;

namespace UI.ER.AvaloniaUI.ViewModels
{
    public class CursAcademicRowViewModel : ViewModelBase, IEtiquetaDescripcio, IId
    {

        public CursAcademicRowViewModel(dtoo.CursAcademic dto, ObservableCollection<CursAcademicRowViewModel> llista)
        {
            _Etiqueta = dto.Etiqueta;
            _Descripcio = dto.Descripcio;
            _EsActiu = dto.EsActiu;
            Id = dto.Id;
            Llista = llista;
            DoTheThing = ReactiveCommand.CreateFromTask( RunTheThing ); 
        }

        ObservableCollection<CursAcademicRowViewModel> Llista;

        public ReactiveCommand<Unit, Unit> DoTheThing { get; } 

        private string _Etiqueta = string.Empty;
        public string Etiqueta
        {
            get { return _Etiqueta; }
            protected set { this.RaiseAndSetIfChanged(ref _Etiqueta, value); }
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
            set { this.RaiseAndSetIfChanged(ref _EsActiu, value); }
        }

        public int Id {get;}

        protected async Task RunTheThing()  
        {
            using  var bl = SuperContext.GetBLOperation<ICursAcademicActivaDesactiva>();

            var dto = ( await bl.Toggle(Id)).Data!;

            // Si tot ha anat bé, la restar serà inactiu.
            Llista
            .Where(x=>x.Id != dto.Id)
            .ToList()
            .ForEach(x=>x.EsActiu = false);

            //
            Etiqueta = dto.Etiqueta;
            Descripcio = dto.Descripcio;
            EsActiu = dto.EsActiu;
        }

    }
}