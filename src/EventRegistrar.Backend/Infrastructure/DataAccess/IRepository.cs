﻿using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EventRegistrar.Backend.Infrastructure.DataAccess;

public interface IRepository<TEntity> : IQueryable<TEntity>
    where TEntity : Entity
{
    Task<TEntity?> Get(Expression<Func<TEntity, bool>> predicate);
    Task<TEntity?> GetById(Guid id);
    Task InsertOrUpdateEntity(TEntity entity, CancellationToken cancellationToken = default);
    TEntity InsertObjectTree(TEntity rootEntity);
    EntityEntry<TEntity> Remove(TEntity entity);
    void Remove(Expression<Func<TEntity, bool>> predicate);
}