using System.Threading.Tasks;
using CommonInterfaces;
using DTO.i;
using DTO.i.DTOs;
using DTO.o.Interfaces;

namespace BusinessLayer.Abstract.Generic
{
    public interface IActivaDesactiva<TDTOo>     
            where TDTOo: IDTOo, IEtiquetaDescripcio, IActiu
    {
        Task<OperationResult<TDTOo>> Activa(int id);
        Task<OperationResult<TDTOo>> Desactiva(int id);
        Task<OperationResult<TDTOo>> Togle(int id);
    }
}