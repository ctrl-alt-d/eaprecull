using BusinessLayer.Abstract;
using BusinessLayer.Abstract.Generic;
using CommonInterfaces;
using DataLayer;
using DataModels.Models.Interfaces;
using DTO.i;
using DTO.o.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLayer.Common
{
    public abstract class BLGetItems<TModel, TParm, TDTOo>
        :BLOperation,
         IGetItems<TParm, TDTOo>
            where TDTOo : IDTOo, IEtiquetaDescripcio
            where TParm : IDtoi
            where TModel : class, IModel, IId

    {
        public BLGetItems(IDbContextFactory<AppDbContext> appDbContextFactory)
        : base(appDbContextFactory)
        {
        }

        protected virtual IQueryable<TModel> GetAllModels()
            =>
            AppDbContextFactory
            .CreateDbContext()
            .Set<TModel>();

        protected abstract IQueryable<TModel> GetModels(TParm request);

        protected abstract TDTOo ToDto(TModel model );

        public virtual Task<OperationResults<TDTOo>> GetItems(
            TParm request
            )
            =>
            Task.FromResult(
                new OperationResults<TDTOo>(
                    GetModels(request)
                    .Select(ToDto)
                    .ToList()
                )
            );

    }
}