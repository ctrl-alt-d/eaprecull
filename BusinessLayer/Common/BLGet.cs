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
    public abstract class BLGet<TModel, TDTOo> 
        :BLOperation,
         IGet<TDTOo> 
            where TDTOo: IDTOo, IEtiquetaDescripcio
            where TModel: class, IModel, IId

    {
        public BLGet(IDbContextFactory<AppDbContext> appDbContextFactory)
        :base(appDbContextFactory)
        {
        }

        protected virtual ValueTask<TModel> GetById(int id) 
            =>
            GetContext()
            .Set<TModel>()
            .FindAsync(id);

        protected abstract TDTOo ToDto(TModel model );
        
        public virtual async Task<OperationResult<TDTOo>> FromId(
            int id
            ) 
            =>
            new (
                ToDto( await GetById(id) )
            );
        
    }
}