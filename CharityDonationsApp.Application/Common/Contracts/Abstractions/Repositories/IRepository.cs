namespace CharityDonationsApp.Application.Common.Contracts.Abstractions.Repositories;

public interface IRepository<TEntity> where TEntity : class
{
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
}