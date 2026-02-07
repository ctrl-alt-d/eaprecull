using CommonInterfaces;
using DTO.o.Interfaces;

namespace DTO.o.DTOs;

public class EtiquetaDescripcio : IEtiquetaDescripcio, IDTOo
{
    public EtiquetaDescripcio(string etiqueta, string descripcio)
    {
        Etiqueta = etiqueta;
        Descripcio = descripcio;
    }

    public string Etiqueta {get; }

    public string Descripcio {get; }
}
