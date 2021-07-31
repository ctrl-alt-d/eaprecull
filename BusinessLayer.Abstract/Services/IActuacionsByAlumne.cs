using BusinessLayer.Abstract.Generic;
using DTO.i.DTOs;
using DTO.o.DTOs;

namespace BusinessLayer.Abstract.Services
{
    public interface IActuacionsByAlumne: IQuery<Actuacio, GetActuacioByAlumneParms>
    {        
    }

}