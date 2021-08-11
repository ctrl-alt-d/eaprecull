using BusinessLayer.Abstract;
using BusinessLayer.Abstract.Exceptions;
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
    public abstract class BLUpdate<TModel, TParm, TDTOo>
        :BLOperation,
         IUpdate<TDTOo, TParm>
            where TDTOo: IDTOo, IEtiquetaDescripcio
            where TParm: IDtoi, IId
            where TModel : class, IModel, IId

    {
        public BLUpdate(IDbContextFactory<AppDbContext> appDbContextFactory)
        : base(appDbContextFactory)
        {
        }

        protected virtual IQueryable<TModel> GetCollection()
            =>
            GetContext()
            .Set<TModel>();


        protected abstract Task PreUpdate(TModel model, TParm parm );
        protected abstract Task UpdateModel(TModel model, TParm parm );
        protected abstract Task PostUpdate(TModel model, TParm parm );
        protected abstract TDTOo ToDto(TModel model );

        public virtual async Task<OperationResult<TDTOo>> Update(
            TParm parm
            )
        {
            try
            {
                //
                var model = await Perfection<TModel>(parm.Id);
                //
                await PreUpdate(model, parm);
                //
                await UpdateModel(model, parm);
                //
                await PostUpdate(model, parm);
                //
                await GetContext().SaveChangesAsync();
                //
                return new( ToDto(model) );
            } 
            catch (BrokenRuleException br)
            {
                return new OperationResult<TDTOo>(br.BrokenRules);
            } 
            catch (Exception e)
            {
                throw new BrokenRuleException($"Error intern no esperat.", e);
            }
        }
    }
}