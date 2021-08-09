using System.Threading.Tasks;
using CommonInterfaces;
using DTO.i;
using DTO.o.Interfaces;

namespace BusinessLayer.Abstract.Generic
{
    public interface IGetSet<TParm, TDTOo>
        :IBLOperation
            where TDTOo : IDTOo, IEtiquetaDescripcio
            where TParm : IDtoi
    {
        Task<OperationResults<TDTOo>> FromPredicate(TParm request);
    }
}