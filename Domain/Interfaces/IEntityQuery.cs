using System.Linq.Expressions;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Services.Interfaces;

public interface IEntityQuery<TEntity> where TEntity : class
{
    public int From { get; set; }
    public int Count { get; set; }
    [JsonIgnore]
    public Expression<Func<TEntity, bool>> AsExpression { get; }

    public IQueryable<TEntity> ToQueryable(IQueryable<TEntity>? queryable, bool asNonTracking);
}