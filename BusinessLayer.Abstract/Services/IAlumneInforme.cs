using BusinessLayer.Abstract.Generic;
using dtoo = DTO.o.DTOs;
using System.Threading.Tasks;

namespace BusinessLayer.Abstract.Services
{
    public interface IAlumneInforme: IBLOperation
    {
        Task<StringOperationResult> Run(string? path, int alumneId);
    }

}