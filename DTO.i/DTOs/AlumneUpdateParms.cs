using System;
using CommonInterfaces;

namespace DTO.i.DTOs
{
    public class AlumneUpdateParms : IDtoi, IId
    {
        public AlumneUpdateParms(int id, string nom, string cognoms, DateTime? dataNaixement, int? centreActualId, int cursDarreraActualitacioDadesId, int? etapaActualId, string nivellActual, DateTime? dataInformeNESENEE, string observacionsNESENEE, DateTime? dataInformeNESENoNEE, string observacionsNESENoNEE, string tags, bool esActiu)
        {
            Id = id;
            Nom = nom;
            Cognoms = cognoms;
            DataNaixement = dataNaixement;
            CentreActualId = centreActualId;
            CursDarreraActualitacioDadesId = cursDarreraActualitacioDadesId;
            EtapaActualId = etapaActualId;
            NivellActual = nivellActual;
            DataInformeNESENEE = dataInformeNESENEE;
            ObservacionsNESENEE = observacionsNESENEE;
            DataInformeNESENoNEE = dataInformeNESENoNEE;
            ObservacionsNESENoNEE = observacionsNESENoNEE;
            Tags = tags;
            EsActiu = esActiu;
        }

        public int Id { get; }
        public string Nom { get; }
        public string Cognoms { get; }
        public DateTime? DataNaixement { get; }
        public int? CentreActualId { get; }
        public int CursDarreraActualitacioDadesId { get; }
        public int? EtapaActualId { get; }
        public string NivellActual { get; }
        public DateTime? DataInformeNESENEE { get; }
        public string ObservacionsNESENEE { get; }
        public DateTime? DataInformeNESENoNEE { get; }
        public string ObservacionsNESENoNEE { get; }
        public string Tags { get; }
        public bool EsActiu { get; }

    }
}
