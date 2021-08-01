using System;
using System.Threading.Tasks;
using DataLayer;
using DataModels.Models.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Common
{
    public abstract class BLOperation: IDisposable
    {
        public readonly IDbContextFactory<AppDbContext> AppDbContextFactory;

        public BLOperation(IDbContextFactory<AppDbContext> appDbContextFactory)
        {
            AppDbContextFactory = appDbContextFactory;
        }

        private AppDbContext? _DbContext;
        private bool disposedValue;

        protected AppDbContext GetContext()
        {
            _DbContext ??=
                AppDbContextFactory
                .CreateDbContext();

            return _DbContext;
        }

        protected ValueTask<TTarget> Perfection<TTarget>(int id)
            where TTarget: class, IModel
        {
            return
                GetContext()
                .Set<TTarget>()
                .FindAsync(id);
        }

        protected async ValueTask<TTarget?> Perfection<TTarget>(int? id)
            where TTarget: class, IModel
        {
            if (!id.HasValue) 
                return (TTarget?) null;

            return 
                await GetContext()
                .Set<TTarget>()
                .FindAsync(id);
        }

        //
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _DbContext?.Dispose();
                }
                
                _DbContext = null;
                disposedValue = true;
            }
        }
        
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}