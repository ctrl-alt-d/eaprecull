using System;
using CommonInterfaces;

namespace DTO.i.DTOs
{
    public class ActuacioUpdateParms : IDtoi, IId
    {
        public ActuacioUpdateParms(int id, int alumneId, int tipusActuacioId, string observacionsTipusActuacio, DateTime momentDeLactuacio, int cursActuacioId, int centreAlMomentDeLactuacioId, int etapaAlMomentDeLactuacioId, string nivellAlMomentDeLactuacio, int minutsDuradaActuacio, string descripcioActuacio)
        {
            Id = id;
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

        public int Id { get; }
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
