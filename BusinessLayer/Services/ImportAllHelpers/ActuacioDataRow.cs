using System;

namespace BusinessLayer.Services.ImportAllHelpers
{
    public class ActuacioDataRow
    {
        public ActuacioDataRow(string nom, string cognoms, DateTime dataNaixement, string centreActual, string etapaActual, string nivellActual, DateTime? dataInformeNESENEE, string observacionsNESENEE, DateTime? dataInformeNESENoNEE, string observacionsNESENoNEE, string tipusActuacio, string observacionsTipusActuacio, DateTime momentDeLactuacio, string cursActuacio, string centreActuacio, string etapaActuacio, string nivellActuacio, int duradaActuacio, string descripcioActuacio, string acords)
        {
            Nom = nom;
            Cognoms = cognoms;
            DataNaixement = dataNaixement;
            CentreActual = centreActual;
            EtapaActual = etapaActual;
            NivellActual = nivellActual;
            DataInformeNESENEE = dataInformeNESENEE;
            ObservacionsNESENEE = observacionsNESENEE;
            DataInformeNESENoNEE = dataInformeNESENoNEE;
            ObservacionsNESENoNEE = observacionsNESENoNEE;
            TipusActuacio = tipusActuacio;
            ObservacionsTipusActuacio = observacionsTipusActuacio;
            MomentDeLactuacio = momentDeLactuacio;
            CursActuacio = cursActuacio;
            CentreActuacio = centreActuacio;
            EtapaActuacio = etapaActuacio;
            NivellActuacio = nivellActuacio;
            DuradaActuacio = duradaActuacio;
            DescripcioActuacio = descripcioActuacio;
            Acords = acords;
        }

        public enum Camps: int
        {
            Nom = 4,
            Cognoms = 3,
            DataNaixement = 2,
            CentreActual = 20,
            EtapaActual = 21,
            NivellActual = 22,
            DataInformeNESENEE = 23,
            ObservacionsNESENEE = 24,
            DataInformeNESENoNEE = 25,
            ObservacionsNESENoNEE = 26,
            TipusActuacio = 10,
            ObservacionsTipusActuacio = 11,
            MomentDeLactuacio = 6,
            CursActuacio = 5,
            CentreActuacio = 7,
            EtapaActuacio = 27,
            NivellActuacio = 9,
            DuradaActuacio = 15,
            DescripcioActuacio = 12,
            Acords = 13
        };

        public string Nom { get; }
        public string Cognoms { get; }
        public DateTime DataNaixement { get; }
        public string CentreActual { get; }
        public string EtapaActual { get; }
        public string NivellActual { get; }
        public DateTime? DataInformeNESENEE { get; }
        public string ObservacionsNESENEE { get; }
        public DateTime? DataInformeNESENoNEE { get; }
        public string ObservacionsNESENoNEE { get; }
        public string TipusActuacio { get; }
        public string ObservacionsTipusActuacio { get; }
        public DateTime MomentDeLactuacio { get; }
        public string CursActuacio { get; }
        public string CentreActuacio { get; }
        public string EtapaActuacio { get; }
        public string NivellActuacio { get; }
        public int DuradaActuacio { get; }
        public string DescripcioActuacio { get; }
        public string Acords { get; }

    }

}