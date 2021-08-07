using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

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

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<TEntity> GetEnumerator() => DbSet.AsEnumerable().GetEnumerator();

        IAsyncEnumerator<TEntity> IAsyncEnumerable<TEntity>.GetAsyncEnumerator(System.Threading.CancellationToken cancellationToken)
        {
            return DbSet.AsAsyncEnumerable<TEntity>().GetAsyncEnumerator();
        }
    }
}