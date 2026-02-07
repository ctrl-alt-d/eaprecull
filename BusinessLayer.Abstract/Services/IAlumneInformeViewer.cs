using BusinessLayer.Abstract.Generic;
using System.Threading.Tasks;
using DTO.o.DTOs;

namespace BusinessLayer.Abstract.Services
{
    /// <summary>
    /// Servei per obtenir les dades de l'informe d'un alumne per mostrar per pantalla
    /// </summary>
    public interface IAlumneInformeViewer : IBLOperation
    {
        Task<OperationResult<AlumneInformeViewerData>> Run(int alumneId);
    }
}
