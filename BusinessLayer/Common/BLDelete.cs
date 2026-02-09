using BusinessLayer.Abstract;
using BusinessLayer.Abstract.Exceptions;
using BusinessLayer.Abstract.Generic;
using CommonInterfaces;
using DataLayer;
using DTO.o.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BusinessLayer.Common
{
    public abstract class BLDelete<TModel, TDTOo>
        : BLOperation,
         IDelete<TDTOo>
            where TDTOo : IDTOo, IEtiquetaDescripcio
            where TModel : class, IModel, IId

    {
        public BLDelete(IDbContextFactory<AppDbContext> appDbContextFactory)
        : base(appDbContextFactory)
        {
        }

        protected virtual IQueryable<TModel> GetCollection()
            =>
            GetContext()
            .Set<TModel>();

        protected abstract Task PreDelete(TModel model);
        protected abstract Task PostDelete(TModel model);
        protected abstract Expression<Func<TModel, TDTOo>> ToDto { get; }

        public async Task<OperationResult<TDTOo>> Delete(IId id)
        {
            try
            {
                //
                var model = await Perfection<TModel>(id.Id);
                //
                await PreDelete(model);
                //
                var dto = await Model2Dto(model);
                //
                GetContext().Set<TModel>().Remove(model);
                //
                await PostDelete(model);
                //
                await GetContext().SaveChangesAsync();
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
