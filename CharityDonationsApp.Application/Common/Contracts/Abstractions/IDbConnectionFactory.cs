using System.Data;

namespace CharityDonationsApp.Application.Common.Contracts.Abstractions;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}