using BusinessLayer.Abstract.Generic;
using dtoo = DTO.o.DTOs;
using System.Threading.Tasks;
using DTO.o.DTOs;

namespace BusinessLayer.Abstract.Services
{
    public interface IPivotActuacions : IBLOperation
    {
        Task<OperationResult<SaveResult>> Run();
    }

}