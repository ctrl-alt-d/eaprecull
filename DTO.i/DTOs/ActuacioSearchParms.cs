using System;
using DTO.i.Interfaces;

namespace DTO.i.DTOs
{
    public class ActuacioSearchParms : IDtoi, IPaginated
    {
        public ActuacioSearchParms(
            int take = 1000,
            int skip = 0,
            string searchString = "",
            int? alumneId = null,
            int? tipusActuacioId = null,
            int? cursActuacioId = null,
            int? centreId = null,
            DateTime? ambDarreraModificacioAnteriorA = null,
            DateTime? ambDarreraModificacioPosteriorA = null)
        {
            Take = take;
            Skip = skip;
            SearchString = searchString;
            AlumneId = alumneId;
            TipusActuacioId = tipusActuacioId;
            CursActuacioId = cursActuacioId;
            CentreId = centreId;
            AmbDarreraModificacioAnteriorA = ambDarreraModificacioAnteriorA;
            AmbDarreraModificacioPosteriorA = ambDarreraModificacioPosteriorA;
        }

        public int Take {get;}
        public int Skip {get;}

        // SearchString: Alumne, Centre, TagsAlumne, Descripcio
        public string SearchString {get;}

        // Alumne
        public int? AlumneId { get; }

        // Tipus Actuació
        public int? TipusActuacioId {get;}

        // Curs actuacio
        public int? CursActuacioId { get; } 

        // Centre
        public int? CentreId {get;}

        // Data Actuació
        public DateTime? AmbDarreraModificacioAnteriorA {get;}
        public DateTime? AmbDarreraModificacioPosteriorA {get;}

    }
}
