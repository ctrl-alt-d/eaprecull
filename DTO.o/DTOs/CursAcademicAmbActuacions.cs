namespace DTO.o.DTOs
{
    public class CursAcademicAmbActuacions : CursAcademic
    {
        public CursAcademicAmbActuacions(int id, int anyInici, string nom, bool esActiu, string etiqueta, string descripcio, int nombreActuacions)
            : base(id, anyInici, nom, esActiu, etiqueta, descripcio)
        {
            NombreActuacions = nombreActuacions;
        }

        // Nombre d'actuacions en aquest curs
        public int NombreActuacions { get; }
    }
}
