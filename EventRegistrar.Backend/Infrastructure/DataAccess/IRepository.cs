using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EventRegistrar.Backend.Infrastructure.DataAccess
{
    public interface IRepository<TEntity> : IQueryable<TEntity>
        where TEntity : Entity
    {
        Task<TEntity> Get(Expression<Func<TEntity, bool>> predicate);

        Task<TEntity> GetById(Guid id);

        Task InsertOrUpdateEntity(TEntity entity, CancellationToken cancellationToken);

        EntityEntry<TEntity> Remove(TEntity entity);

        void Remove(Expression<Func<TEntity, bool>> predicate);
    }
}