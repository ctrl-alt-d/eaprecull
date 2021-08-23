using System;

namespace BusinessLayer.Services.ImportAllHelpers
{
    public class ActuacioDataRow
    {
        public ActuacioDataRow(string nom, string cognoms, DateTime dataNaixement, string centreActual, string etapaActual, string nuvellActual, DateTime? dataInformeNESENEE, string observacionsNESENEE, DateTime? dataInformeNESENoNEE, string observacionsNESENoNEE, string tipusActuacio, DateTime momentDeLactuacio, string cursActuacio, string centreActuacio, string etapaActuacio, string nivellActuacio, int duradaActuacio, string descripcioActuacio)
        {
            Nom = nom;
            Cognoms = cognoms;
            DataNaixement = dataNaixement;
            CentreActual = centreActual;
            EtapaActual = etapaActual;
            NuvellActual = nuvellActual;
            DataInformeNESENEE = dataInformeNESENEE;
            ObservacionsNESENEE = observacionsNESENEE;
            DataInformeNESENoNEE = dataInformeNESENoNEE;
            ObservacionsNESENoNEE = observacionsNESENoNEE;
            TipusActuacio = tipusActuacio;
            MomentDeLactuacio = momentDeLactuacio;
            CursActuacio = cursActuacio;
            CentreActuacio = centreActuacio;
            EtapaActuacio = etapaActuacio;
            NivellActuacio = nivellActuacio;
            DuradaActuacio = duradaActuacio;
            DescripcioActuacio = descripcioActuacio;
        }

        public enum Camps: int
        {
            Nom = 1,
            Cognoms,
            DataNaixement,
            CentreActual,
            EtapaActual,
            NuvellActual,
            DataInformeNESENEE,
            ObservacionsNESENEE,
            DataInformeNESENoNEE,
            ObservacionsNESENoNEE,
            TipusActuacio,
            MomentDeLactuacio,
            CursActuacio,
            CentreActuacio,
            EtapaActuacio,
            NivellActuacio,
            DuradaActuacio,
            DescripcioActuacio,
        };

        public string Nom { get; }
        public string Cognoms { get; }
        public DateTime DataNaixement { get; }
        public string CentreActual { get; }
        public string EtapaActual { get; }
        public string NuvellActual { get; }
        public DateTime? DataInformeNESENEE { get; }
        public string ObservacionsNESENEE { get; }
        public DateTime? DataInformeNESENoNEE { get; }
        public string ObservacionsNESENoNEE { get; }
        public string TipusActuacio { get; }
        public DateTime MomentDeLactuacio { get; }
        public string CursActuacio { get; }
        public string CentreActuacio { get; }
        public string EtapaActuacio { get; }
        public string NivellActuacio { get; }
        public int DuradaActuacio { get; }
        public string DescripcioActuacio { get; }

    }

}