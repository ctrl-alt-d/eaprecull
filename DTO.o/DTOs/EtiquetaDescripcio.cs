using CommonInterfaces;

namespace DTO.o.DTOs;

public class EtiquetaDescripcio : IEtiquetaDescripcio
{
    public EtiquetaDescripcio(string etiqueta, string descripcio)
    {
        Etiqueta = etiqueta;
        Descripcio = descripcio;
    }

    public string Etiqueta {get; }

    public string Descripcio {get; }
}
