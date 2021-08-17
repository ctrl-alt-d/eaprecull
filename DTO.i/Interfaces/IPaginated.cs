namespace DTO.i.Interfaces
{
    public interface IPaginated
    {
        int Take {get; }
        int Skip {get; }
    }
}