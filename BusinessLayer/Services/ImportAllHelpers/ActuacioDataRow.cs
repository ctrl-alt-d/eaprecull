namespace BusinessLayer.Services.ImportAllHelpers
{
    public class ActuacioDataRow
    {
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
        public ActuacioDataRow(string nom, string cognoms, string dataNaixement, string centreActual, string etapaActual, string nuvellActual, string dataInformeNESENEE, string observacionsNESENEE, string dataInformeNESENoNEE, string observacionsNESENoNEE, string tipusActuacio, string momentDeLactuacio, string cursActuacio, string centreActuacio, string etapaActuacio, string nivellActuacio, string duradaActuacio, string descripcioActuacio)
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

        public string Nom { get; }
        public string Cognoms { get; }
        public string DataNaixement { get; }
        public string CentreActual { get; }
        public string EtapaActual { get; }
        public string NuvellActual { get; }
        public string DataInformeNESENEE { get; }
        public string ObservacionsNESENEE { get; }
        public string DataInformeNESENoNEE { get; }
        public string ObservacionsNESENoNEE { get; }
        public string TipusActuacio { get; }
        public string MomentDeLactuacio { get; }
        public string CursActuacio { get; }
        public string CentreActuacio { get; }
        public string EtapaActuacio { get; }
        public string NivellActuacio { get; }
        public string DuradaActuacio { get; }
        public string DescripcioActuacio { get; }

    }

}