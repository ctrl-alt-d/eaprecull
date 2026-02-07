using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BusinessLayer.Abstract;
using BusinessLayer.Abstract.Generic;
using CommonInterfaces;
using DTO.i;
using DTO.o.Interfaces;

namespace BusinessLayer.Common
{
    /// <summary>
    /// Interfície per a serveis Set que permeten passar una projecció personalitzada.
    /// Està a BusinessLayer (no a BusinessLayer.Abstract) perquè necessita conèixer TModel.
    /// </summary>
    public interface ISetProjectable<TParm, TModel, TDefaultDTOo>
        : ISet<TParm, TDefaultDTOo>
            where TDefaultDTOo : IDTOo, IEtiquetaDescripcio
            where TParm : IDtoi
            where TModel : class, IModel, IId
    {
        Task<OperationResults<TDTOo>> FromPredicateProjected<TDTOo>(
            TParm request,
            Expression<Func<TModel, TDTOo>> projection)
            where TDTOo : IDTOo, IEtiquetaDescripcio;

        Task<OperationResult<TDTOo>> FromIdProjected<TDTOo>(
            int id,
            Expression<Func<TModel, TDTOo>> projection)
            where TDTOo : IDTOo, IEtiquetaDescripcio;
    }
}
