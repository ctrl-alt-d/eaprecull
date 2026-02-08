namespace BusinessLayer.Abstract.Generic
{
    public interface IServiceFactory
    {
        T GetBLOperation<T>() where T : IBLOperation;
    }
}
