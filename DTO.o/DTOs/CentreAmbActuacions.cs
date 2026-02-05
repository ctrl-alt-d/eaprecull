namespace DTO.o.DTOs
{
    public class CentreAmbActuacions : Centre
    {
        public CentreAmbActuacions(int id, string codi, string nom, bool esActiu, string etiqueta, string descripcio, int totalActuacions, int actuacionsCursActiu)
            : base(id, codi, nom, esActiu, etiqueta, descripcio)
        {
            TotalActuacions = totalActuacions;
            ActuacionsCursActiu = actuacionsCursActiu;
        }

        // Total d'actuacions en aquest centre (històric)
        public int TotalActuacions { get; }

        // Actuacions en el curs acadèmic actiu
        public int ActuacionsCursActiu { get; }
    }
}
