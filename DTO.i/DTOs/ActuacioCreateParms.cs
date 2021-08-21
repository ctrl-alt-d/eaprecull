using System;

namespace DTO.i.DTOs
{
    public class ActuacioCreateParms : IDtoi
    {
        public ActuacioCreateParms(int alumneId, int tipusActuacioId, string observacionsTipusActuacio, DateTime momentDeLactuacio, int cursActuacioId, int centreAlMomentDeLactuacioId, int etapaAlMomentDeLactuacioId, string nivellAlMomentDeLactuacio, int minutsDuradaActuacio, string descripcioActuacio)
        {
            AlumneId = alumneId;
            TipusActuacioId = tipusActuacioId;
            ObservacionsTipusActuacio = observacionsTipusActuacio;
            MomentDeLactuacio = momentDeLactuacio;
            CursActuacioId = cursActuacioId;
            CentreAlMomentDeLactuacioId = centreAlMomentDeLactuacioId;
            EtapaAlMomentDeLactuacioId = etapaAlMomentDeLactuacioId;
            NivellAlMomentDeLactuacio = nivellAlMomentDeLactuacio;
            MinutsDuradaActuacio = minutsDuradaActuacio;
            DescripcioActuacio = descripcioActuacio;
        }

        public int AlumneId { get; }
        public int TipusActuacioId { get; }
        public string ObservacionsTipusActuacio { get; }
        public DateTime MomentDeLactuacio { get; }
        public int CursActuacioId { get; } 
        public int CentreAlMomentDeLactuacioId { get; } 
        public int EtapaAlMomentDeLactuacioId { get; } 
        public string NivellAlMomentDeLactuacio { get; }
        public int MinutsDuradaActuacio { get; }
        public string DescripcioActuacio { get; } 


    }
}
