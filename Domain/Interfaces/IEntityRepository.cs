namespace WebApi.Services.Interfaces;

public interface IEntityRepository<TEntity, in TModel, TKey> where TEntity : class
{
    Task<List<TEntity>> QueryAsync(IEntityQuery<TEntity> query, bool asNonTracking = true, CancellationToken cancellationToken = default(CancellationToken));
    Task<int> QueryCountAsync(IEntityQuery<TEntity> query, CancellationToken cancellationToken = default(CancellationToken));
}