using System.Linq.Expressions;

namespace CharityDonationsApp.Application.Common.Contracts.Abstractions.Repositories;

public interface IRepository<TEntity> where TEntity : class
{
    Task<TEntity?> FindAsync(object id, CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Update(TEntity entity, params Expression<Func<TEntity, object>>[] updatedProperties);
}