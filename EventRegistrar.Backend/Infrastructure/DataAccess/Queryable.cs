using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EventRegistrar.Backend.Infrastructure.DataAccess
{
    public class Queryable<TEntity> : IQueryable<TEntity>, IAsyncEnumerable<TEntity>
        where TEntity : class
    {
        public Queryable(DbContext dbContext)
        {
            DbSet = dbContext.Set<TEntity>();
        }

        public Type ElementType => ((IQueryable)DbSet).ElementType;
        public Expression Expression => ((IQueryable)DbSet).Expression;
        public IQueryProvider Provider => ((IQueryable)DbSet).Provider;
        protected DbSet<TEntity> DbSet { get; }

        public IEnumerator<TEntity> GetEnumerator()
        {
            return DbSet.AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IAsyncEnumerator<TEntity> IAsyncEnumerable<TEntity>.GetEnumerator()
        {
            return ((IAsyncEnumerableAccessor<TEntity>)DbSet).AsyncEnumerable.GetEnumerator();
        }
    }
}