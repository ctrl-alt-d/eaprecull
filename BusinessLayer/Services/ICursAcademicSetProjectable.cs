using BusinessLayer.Abstract.Services;
using BusinessLayer.Common;
using Parms = DTO.i.DTOs;
using Dtoo = DTO.o.DTOs;
using Models = DataModels.Models;

namespace BusinessLayer.Services
{
    /// <summary>
    /// Interfície que estén ICursAcademicSet amb capacitat de projecció personalitzada.
    /// Permet usar FromPredicateProjected per obtenir DTOs diferents del per defecte.
    /// </summary>
    public interface ICursAcademicSetProjectable 
        : ICursAcademicSet, 
          ISetProjectable<Parms.EsActiuParms, Models.CursAcademic, Dtoo.CursAcademic>
    {
    }
}
