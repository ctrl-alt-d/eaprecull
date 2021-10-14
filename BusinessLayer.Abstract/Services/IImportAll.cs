using BusinessLayer.Abstract.Generic;
using dtoo = DTO.o.DTOs;
using System.Threading.Tasks;

namespace BusinessLayer.Abstract.Services
{
    public interface IImportAll : IBLOperation
    {
        Task<OperationResult<dtoo.ImportAllResult>> Run(string path);
    }

}