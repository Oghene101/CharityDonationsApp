using CharityDonationsApp.Application.Common.Contracts.Abstractions.Repositories;
using CharityDonationsApp.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace CharityDonationsApp.Infrastructure.Persistence.Repositories;

public class Repository<TEntity>(
    AppDbContext context) : IRepository<TEntity> where TEntity : class
{
    private readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        => await _dbSet.AddAsync(entity, cancellationToken);
}