using BusinessLayer.Abstract.Generic;
using Dtoo = DTO.o.DTOs;
using System.Threading.Tasks;

namespace BusinessLayer.Abstract.Services
{
    public interface IImportAll : IBLOperation
    {
        Task<OperationResult<Dtoo.ImportAllResult>> Run(string path);
    }

}