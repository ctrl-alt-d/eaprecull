using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DataLayer;
using DataModels.Models.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Common
{
    public abstract class BLOperation : IDisposable
    {
        public readonly IDbContextFactory<AppDbContext> AppDbContextFactory;

        public BLOperation(IDbContextFactory<AppDbContext> appDbContextFactory)
        {
            AppDbContextFactory = appDbContextFactory;
        }

        private AppDbContext? _DbContext;
        private bool disposedValue;

        protected virtual AppDbContext GetContext()
        {
            _DbContext ??=
                AppDbContextFactory
                .CreateDbContext();

            return _DbContext;
        }

        protected async virtual ValueTask<TTarget> Perfection<TTarget>(int id)
            where TTarget : class, IModel
        {
            var model =
                await 
                GetContext()
                .Set<TTarget>()
                .FindAsync(id);

            var result =
                model ?? 
                throw new Exception($"{typeof(TTarget).FullName} no trobat per id {id}");

            return result;
        }

        protected virtual async ValueTask<TTarget?> Perfection<TTarget>(int? id)
            where TTarget : class, IModel
        {
            if (!id.HasValue)
                return (TTarget?)null;

            var model =
                await GetContext()
                .Set<TTarget>()
                .FindAsync(id);

            return model;
        }

        protected virtual Task LoadReference<TTarget, TProperty>(TTarget model, Expression<Func<TTarget, TProperty?>> propertyExpression)
            where TTarget : class, IModel
            where TProperty : class, IModel
            =>
            GetContext().Entry(model).Reference(propertyExpression).LoadAsync();

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