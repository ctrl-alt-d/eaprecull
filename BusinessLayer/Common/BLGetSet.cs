using BusinessLayer.Abstract;
using BusinessLayer.Abstract.Generic;
using CommonInterfaces;
using DataLayer;
using DataModels.Models.Interfaces;
using DTO.i;
using DTO.o.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLayer.Common
{
    public abstract class BLGetSet<TModel, TParm, TDTOo>
        : BLOperation,
         IGetSet<TParm, TDTOo>
            where TDTOo : IDTOo, IEtiquetaDescripcio
            where TParm : IDtoi
            where TModel : class, IModel, IId

    {
        public BLGetSet(IDbContextFactory<AppDbContext> appDbContextFactory)
        : base(appDbContextFactory)
        {
        }

        protected virtual IQueryable<TModel> GetAllModels()
            =>
            GetContext()
            .Set<TModel>();

        protected abstract IQueryable<TModel> GetModels(TParm request);

        protected abstract Func<TModel, TDTOo> ToDto {get;}

        public virtual async Task<OperationResults<TDTOo>> FromPredicate(
            TParm request
            )
        {
            //var todto = ToDto();
            return new OperationResults<TDTOo>(
                await
                GetModels(request)
                .Select(x => ToDto(x))
                .ToListAsync()
            );
        }

    }
}