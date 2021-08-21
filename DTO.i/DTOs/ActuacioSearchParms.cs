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
            DateTime? ambDataActuacioAnteriorA = null,
            DateTime? ambDataActuacioPosteriorA = null)
        {
            Take = take;
            Skip = skip;
            SearchString = searchString;
            AlumneId = alumneId;
            TipusActuacioId = tipusActuacioId;
            CursActuacioId = cursActuacioId;
            CentreId = centreId;
            AmbDataActuacioAnteriorA = ambDataActuacioAnteriorA;
            AmbDataActuacioPosteriorA = ambDataActuacioPosteriorA;
        }

        public int Take {get;}
        public int Skip {get;}

        // SearchString: Alumne (nom, cognoms i tag), Centre, Descripcio, Tipus act, Curs.
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
        public DateTime? AmbDataActuacioAnteriorA {get;}
        public DateTime? AmbDataActuacioPosteriorA {get;}

    }
}
