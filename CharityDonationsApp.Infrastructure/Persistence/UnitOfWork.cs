using System.Data;
using CharityDonationsApp.Application.Common.Contracts.Abstractions;
using CharityDonationsApp.Application.Common.Contracts.Abstractions.Repositories;
using CharityDonationsApp.Domain.Entities;
using CharityDonationsApp.Infrastructure.Persistence.DbContexts;
using CharityDonationsApp.Infrastructure.Persistence.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage;
using Transaction = System.Transactions.Transaction;

namespace CharityDonationsApp.Infrastructure.Persistence;

public class UnitOfWork(
    AppDbContext context,
    IDbConnectionFactory connectionFactory) : IUnitOfWork, IAsyncDisposable
{
    private IDbConnection? _connection;
    private IDbContextTransaction? _transaction;

    private IDbConnection Connection => _connection ??= connectionFactory.CreateConnection();

    # region Repositories

    private IRefreshTokenRepository? _refreshTokensRead;
    private IRepository<RefreshToken>? _refreshTokensWrite;
    private IKycVerificationRepository? _kycVerificationsRead;
    private IRepository<KycVerification>? _kycVerificationsWrite;
    private IAddressRepository? _addressesRead;
    private IRepository<Address>? _addressesWrite;

    #region RefreshTokens

    public IRefreshTokenRepository RefreshTokensReadRepository =>
        _refreshTokensRead ??= new RefreshTokenRepository(connectionFactory);

    public IRepository<RefreshToken> RefreshTokensWriteRepository =>
        _refreshTokensWrite ??= new Repository<RefreshToken>(context);

    #endregion

    #region KycVerifications

    public IKycVerificationRepository KycVerificationsReadRepository =>
        _kycVerificationsRead ??= new KycVerificationRepository(connectionFactory);

    public IRepository<KycVerification> KycVerificationsWriteRepository =>
        _kycVerificationsWrite ??= new Repository<KycVerification>(context);

    #endregion

    #region Addresses

    public IAddressRepository AddressesReadRepository =>
        _addressesRead ??= new AddressRepository(connectionFactory);

    public IRepository<Address> AddressesWriteRepository =>
        _addressesWrite ??= new Repository<Address>(context);

    #endregion

    # endregion

    # region Transaction support (EF + Dapper can share the same transaction if needed)

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null) throw new InvalidOperationException("There is already an active transaction.");
        _transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var sqlConnection = (SqlConnection)Connection;
        if (sqlConnection.State == ConnectionState.Closed) await sqlConnection.OpenAsync(cancellationToken);
        sqlConnection.EnlistTransaction(Transaction.Current!);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null) throw new InvalidOperationException("There is no active transaction.");
        await SaveChangesAsync(cancellationToken);
        await _transaction.CommitAsync(cancellationToken);
        await DisposeTransactionAsync();
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null) throw new InvalidOperationException("There is no active transaction.");
        await _transaction.RollbackAsync(cancellationToken);
        await DisposeTransactionAsync();
    }

    private async Task DisposeTransactionAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await context.SaveChangesAsync(cancellationToken);


    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null) await _transaction.DisposeAsync();
        _connection?.Dispose();
    }

    #endregion
}