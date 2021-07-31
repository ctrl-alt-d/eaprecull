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
    public abstract class BLCreate<TModel, TParm, TDTOo>
        :BLOperation,
         ICreate<TDTOo, TParm>
            where TDTOo: IDTOo, IEtiquetaDescripcio
            where TParm: IDtoi
            where TModel : class, IModel, IId

    {
        public BLCreate(IDbContextFactory<AppDbContext> appDbContextFactory)
        : base(appDbContextFactory)
        {
        }

        protected virtual IQueryable<TModel> GetAllModels()
            =>
            AppDbContextFactory
            .CreateDbContext()
            .Set<TModel>();

        protected abstract TModel SetValues(TParm parm );
        protected abstract TDTOo ToDto(TModel parm );

        public virtual async Task<OperationResult<TDTOo>> Create(
            TParm parm
            )
            {
                var model = SetValues(parm);
                using var ctx = AppDbContextFactory.CreateDbContext();                
                ctx.Add(model);
                await ctx.SaveChangesAsync();
                return new( ToDto(model) );
            }
    }
}