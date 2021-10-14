using BusinessLayer.Abstract.Generic;
using dtoo = DTO.o.DTOs;
using System.Threading.Tasks;
using DTO.o.DTOs;

namespace BusinessLayer.Abstract.Services
{
    public interface IAlumneInforme : IBLOperation
    {
        Task<OperationResult<SaveResult>> Run(int alumneId);
    }

}