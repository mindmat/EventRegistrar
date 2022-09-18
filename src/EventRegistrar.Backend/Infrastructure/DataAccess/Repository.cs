using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EventRegistrar.Backend.Infrastructure.DataAccess;

public class Repository<TEntity> : Queryable<TEntity>, IRepository<TEntity>
    where TEntity : Entity, new()
{
    private readonly DbContext _dbContext;

    public Repository(DbContext dbContext)
        : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<TEntity?> Get(Expression<Func<TEntity, bool>> predicate)
    {
        return DbSet.FirstOrDefaultAsync(predicate);
    }

    public Task<TEntity?> GetById(Guid id)
    {
        return DbSet.FirstOrDefaultAsync(entity => entity.Id == id);
    }

    public TEntity InsertObjectTree(TEntity rootEntity)
    {
        var entry = DbSet.Add(rootEntity);
        return entry.Entity;
    }

    public async Task InsertOrUpdateEntity(TEntity entity, CancellationToken cancellationToken = default)
    {
        // prevent empty Guid
        if (entity.Id == Guid.Empty)
        {
            entity.Id = Guid.NewGuid();
        }

        // add entity to context
        DbSet.Attach(entity);

        var entry = _dbContext.Entry(entity);
        var dbValues = await entry.GetDatabaseValuesAsync(cancellationToken);
        if (dbValues == null)
        {
            // new entity, INSERT
            entry.State = EntityState.Added;
        }
        else
        {
            // existing entity, UPDATE
            // in EF Core this lines resets the original values to the entity
            //entry.State = EntityState.Unchanged;

            // check with dbValues if modified or unchanged (DbContext sets state)
            entry.OriginalValues.SetValues(dbValues);

            // check concurrency
            // entry.CurrentValues[rowversionPropertyName] must be the value sent by the client in order that optimistic locking works
            entry.OriginalValues[nameof(Entity.RowVersion)] = entry.CurrentValues[nameof(Entity.RowVersion)];
        }
    }

    public EntityEntry<TEntity> Remove(TEntity entityToDelete)
    {
        // make sure the entity is in the context
        entityToDelete = DbSet.Find(entityToDelete.Id);

        return DbSet.Remove(entityToDelete);
    }

    public void Remove(Expression<Func<TEntity, bool>> predicate)
    {
        var entitiesToDelete = DbSet.Where(predicate);
        foreach (var entityToDelete in entitiesToDelete)
        {
            DbSet.Remove(entityToDelete);
        }
    }
}