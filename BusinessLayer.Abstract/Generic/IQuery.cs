using CommonInterfaces;
using DTO.i;

namespace BusinessLayer.Abstract.Generic
{
    public interface IQuery<T, P> 
        where T: IEtiquetaDescripcio
        where P: IDtoi
    {
        OperationResults<T> Query(P request);
    }
}