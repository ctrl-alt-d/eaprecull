using System;

namespace DTO.i.DTOs
{
    public class CursAcademicCreateParms : IDtoi
    {
        public CursAcademicCreateParms(int anyInici, bool esElCursActual)
        {
            AnyInici = anyInici;
            EsElCursActual = esElCursActual;
        }

        public int AnyInici {get; }
        public bool EsElCursActual {get; }

    }
}
