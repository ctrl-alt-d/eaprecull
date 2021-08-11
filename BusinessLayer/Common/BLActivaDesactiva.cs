using BusinessLayer.Abstract;
using BusinessLayer.Abstract.Exceptions;
using BusinessLayer.Abstract.Generic;
using CommonInterfaces;
using DataLayer;
using DataModels.Models.Interfaces;
using DTO.o.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLayer.Common
{
    public abstract class BLActivaDesactiva<TModel, TDTOo>
        :BLOperation,
         IActivaDesactiva<TDTOo>
            where TDTOo: IDTOo, IEtiquetaDescripcio, IActiu
            where TModel : class, IModel, IId, IActivable

    {
        public BLActivaDesactiva(IDbContextFactory<AppDbContext> appDbContextFactory)
        : base(appDbContextFactory)
        {
        }

        protected virtual IQueryable<TModel> GetCollection()
            =>
            GetContext()
            .Set<TModel>();

        protected abstract Task Pre(TModel model);
        protected abstract Task Post(TModel model);
        protected abstract TDTOo ToDto(TModel model );

        protected virtual async Task<OperationResult<TDTOo>> Update(
            int id,
            bool? esActiu = null
            )
        {
            try
            {
                //
                var model = await Perfection<TModel>(id);
                //
                await Pre(model);
                //
                esActiu ??= model.EsActiu;
                if (esActiu.Value) model.SetInactiu();
                if (!esActiu.Value) model.SetActiu();
                //
                await Post(model);
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

        public Task<OperationResult<TDTOo>> Activa(int id)
        {
            return Update(id, true);
        }

        public Task<OperationResult<TDTOo>> Desactiva(int id)
        {
            return Update(id, false);
        }

        public Task<OperationResult<TDTOo>> Toggle(int id)
        {
            return Update(id);
        }
    }
}