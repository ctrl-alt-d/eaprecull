using CommonInterfaces;

namespace BusinessLayer.Abstract.Generic
{
    public interface IDelete<T> where T: IEtiquetaDescripcio
    {
        OperationResult<T> Delete(int id);
    }
}