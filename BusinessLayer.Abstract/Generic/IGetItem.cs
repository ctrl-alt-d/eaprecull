using System.Threading.Tasks;
using CommonInterfaces;
using DTO.o.Interfaces;

namespace BusinessLayer.Abstract.Generic
{
    public interface IGetItem<TDTOo>
            where TDTOo: IDTOo, IEtiquetaDescripcio
        
    {
        Task<OperationResult<TDTOo>> GetItem(int id);
    }
}