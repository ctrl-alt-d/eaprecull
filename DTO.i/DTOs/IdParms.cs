using CommonInterfaces;

namespace DTO.i.DTOs
{
    public class IdParms : IDtoi, IId
    {
        public IdParms(int id)
        {
            Id = id;
        }
        public int Id { get; }
    }
}
