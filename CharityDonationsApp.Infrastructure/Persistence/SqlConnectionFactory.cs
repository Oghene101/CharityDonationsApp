using System.Data;
using CharityDonationsApp.Application.Common.Contracts.Abstractions;
using Microsoft.Data.SqlClient;

namespace CharityDonationsApp.Infrastructure.Persistence;

public class SqlConnectionFactory(
    string connectionString) : IDbConnectionFactory
{
    public IDbConnection CreateConnection() => new SqlConnection(connectionString);
}