using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace EventRegistrar.Backend.Infrastructure.DataAccess
{
    public class Queryable<TEntity> : IQueryable<TEntity>, IAsyncEnumerable<TEntity>
        where TEntity : class
    {
        private readonly DbSet<TEntity> _dbSet;

        public Queryable(DbContext dbContext)
        {
            _dbSet = dbContext.Set<TEntity>();
        }

        public Type ElementType => ((IQueryable)_dbSet).ElementType;

        public Expression Expression => ((IQueryable)_dbSet).Expression;

        public IQueryProvider Provider => ((IQueryable)_dbSet).Provider;

        public IEnumerator<TEntity> GetEnumerator()
        {
            return _dbSet.AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IAsyncEnumerator<TEntity> IAsyncEnumerable<TEntity>.GetEnumerator()
        {
            return ((IAsyncEnumerableAccessor<TEntity>)_dbSet).AsyncEnumerable.GetEnumerator();
        }
    }
}