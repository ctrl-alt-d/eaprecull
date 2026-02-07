using BusinessLayer.Abstract;
using BusinessLayer.Abstract.Exceptions;
using BusinessLayer.Abstract.Generic;
using CommonInterfaces;
using DataLayer;
using DTO.i;
using DTO.o.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BusinessLayer.Common
{
    public abstract class BLUpdate<TModel, TParm, TDTOo>
        : BLOperation,
         IUpdate<TDTOo, TParm>
            where TDTOo : IDTOo, IEtiquetaDescripcio
            where TParm : IDtoi, IId
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


        protected abstract Task PreUpdate(TModel model, TParm parm);
        protected abstract Task UpdateModel(TModel model, TParm parm);
        protected abstract Task PostUpdate(TModel model, TParm parm);
        protected abstract void ResetReferences(TModel model);
        protected abstract Expression<Func<TModel, TDTOo>> ToDto { get; }

        public virtual async Task<OperationResult<TDTOo>> Update(
            TParm parm
            )
        {
            try
            {
                //
                var model = await Perfection<TModel>(parm.Id);
                //
                ResetReferences(model);
                //
                await PreUpdate(model, parm);
                //
                await UpdateModel(model, parm);
                //
                await PostUpdate(model, parm);
                //
                await GetContext().SaveChangesAsync();
                //
                var dto = await Model2Dto(model);
                //
                return new(dto);
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

        protected virtual Task<TDTOo> Model2Dto(TModel model)
            =>
            GetCollection()
            .Where(x => x.Id == model.Id)
            .Select(ToDto)
            .FirstAsync();

    }
}