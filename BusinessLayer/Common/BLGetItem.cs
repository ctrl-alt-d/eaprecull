using BusinessLayer.Abstract;
using BusinessLayer.Abstract.Generic;
using CommonInterfaces;
using DataLayer;
using DataModels.Models.Interfaces;
using DTO.o.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BusinessLayer.Common
{
    public abstract class BLGetItem<TModel, TDTOo> 
        :BLOperation,
         IGetItem<TDTOo> 
            where TDTOo: IDTOo, IEtiquetaDescripcio
            where TModel: class, IModel, IId

    {
        public BLGetItem(IDbContextFactory<AppDbContext> appDbContextFactory)
        :base(appDbContextFactory)
        {
        }

        protected virtual ValueTask<TModel> GetById(int id) 
            =>
            AppDbContextFactory
            .CreateDbContext()
            .Set<TModel>()
            .FindAsync(id);

        protected abstract TDTOo ToDto(TModel parm );

        public virtual async Task<OperationResult<TDTOo>> GetItem(
            int id
            ) 
            =>
            new (
                ToDto( await GetById(id) )
            );
        
    }
}