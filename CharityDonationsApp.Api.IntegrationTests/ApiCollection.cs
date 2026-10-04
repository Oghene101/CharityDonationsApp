namespace CharityDonationsApp.Api.IntegrationTests;

/// <summary>
/// Shares one API host and one database container across all test classes.
/// </summary>
[CollectionDefinition(Name)]
public class ApiCollection : ICollectionFixture<ApiFactory<ApiEntryPoint>>
{
    public const string Name = "Api";
}
