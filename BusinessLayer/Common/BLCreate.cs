using BusinessLayer.Abstract;
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
    public abstract class BLCreate<TModel, TParm, TDTOo> : BLOperation
            where TDTOo: IDTOo, IEtiquetaDescripcio
            where TParm: IDtoi
            where TModel : class, IModel, IId

    {
        public BLCreate(IDbContextFactory<AppDbContext> appDbContextFactory)
        : base(appDbContextFactory)
        {
        }

        public virtual IQueryable<TModel> GetAllModels()
            =>
            AppDbContextFactory
            .CreateDbContext()
            .Set<TModel>();

        public abstract TModel SetValues(TParm parm );

        public virtual async Task<OperationResult<TDTOo>> Execute(
            TParm parm,
            Func<TModel, TDTOo> toDto
            )
            {
                var model = SetValues(parm);
                using var ctx = AppDbContextFactory.CreateDbContext();                
                ctx.Add(model);
                await ctx.SaveChangesAsync();
                return new( toDto(model) );
            }
    }
}