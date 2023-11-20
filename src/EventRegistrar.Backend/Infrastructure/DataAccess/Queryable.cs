using System.Collections;
using System.Linq.Expressions;

namespace EventRegistrar.Backend.Infrastructure.DataAccess;

public class Queryable<TEntity>(DbContext dbContext) : IQueryable<TEntity>, IAsyncEnumerable<TEntity>
    where TEntity : class
{
    public Type ElementType => ((IQueryable)DbSet).ElementType;
    public Expression Expression => ((IQueryable)DbSet).Expression;
    public IQueryProvider Provider => ((IQueryable)DbSet).Provider;
    protected DbSet<TEntity> DbSet { get; } = dbContext.Set<TEntity>();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<TEntity> GetEnumerator()
    {
        return DbSet.AsEnumerable().GetEnumerator();
    }

    IAsyncEnumerator<TEntity> IAsyncEnumerable<TEntity>.GetAsyncEnumerator(CancellationToken cancellationToken)
    {
        return DbSet.AsAsyncEnumerable<TEntity>().GetAsyncEnumerator();
    }
}