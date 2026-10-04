using System.Security.Cryptography;
using CharityDonationsApp.Application.Common.Contracts.Abstractions;
using CharityDonationsApp.Domain.Constants;
using CharityDonationsApp.Infrastructure.Configurations;
using CharityDonationsApp.Infrastructure.Persistence.DbContexts;
using CharityDonationsApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Testcontainers.MsSql;

namespace CharityDonationsApp.Api.IntegrationTests;

public class ApiFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint>, IAsyncLifetime where TEntryPoint : class
{
    private readonly MsSqlContainer _database = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    private readonly string _encryptionKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    private readonly string _jwtPrivateKey;
    private readonly string _jwtPublicKey;

    public ApiFactory()
    {
        using var rsa = RSA.Create(2048);
        _jwtPrivateKey = rsa.ExportPkcs8PrivateKeyPem();
        _jwtPublicKey = rsa.ExportSubjectPublicKeyInfoPem();
    }

    private string ConnectionString => new SqlConnectionStringBuilder(_database.GetConnectionString())
    {
        InitialCatalog = "CharityDonationsApp_IntegrationTests"
    }.ConnectionString;

    public async Task InitializeAsync()
    {
        await _database.StartAsync();
        await MigrateDatabaseAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Host configuration is handed to the API as command-line arguments, so it is already in place
        // when Program reads settings while registering services, and it overrides every other source.
        builder.ConfigureHostConfiguration(config => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = ConnectionString,
            [$"{JwtSettings.Path}:Issuer"] = "CharityDonationsApp.IntegrationTests",
            [$"{JwtSettings.Path}:Audience"] = "CharityDonationsApp.IntegrationTests",
            [$"{JwtSettings.Path}:PrivateKey"] = _jwtPrivateKey,
            [$"{JwtSettings.Path}:PublicKey"] = _jwtPublicKey,
            [$"{JwtSettings.Path}:ExpireMinutes"] = "15",
            [$"{JwtSettings.Path}:RefreshTokenExpireDays"] = "7",
            [$"{EncryptionSettings.Path}:Key"] = _encryptionKey,
            [$"{AuthSettings.Path}:MaxFailedAttempts"] = "3",
            [$"{AuthSettings.Path}:BaseLockoutMinutes"] = "15",
            [$"{AuthSettings.Path}:LockoutMultiplier"] = "2",
            [$"{AuthSettings.Path}:MaxLockoutMinutes"] = "240",
            [$"{EmailSettings.Path}:ConfirmEmailEndpoint"] = "https://localhost/confirm-email?",
            [$"{ApiEndpoints.Path}:ResetPasswordEndpoint"] = "https://localhost/reset-password?",
            [$"{SeedSettings.Path}:User:Email"] = TestUser.Email,
            [$"{SeedSettings.Path}:User:Password"] = TestUser.Password
        }));

        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // services.RemoveAll<IConfigureOptions<AuthenticationOptions>>();

            services.RemoveAll<IEmailService>();
            services.AddSingleton<IEmailService, NoOpEmailService>();

            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, AuthHandler>("Test", _ => { });

            services.AddAuthorizationBuilder()
                .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes("Test")
                    .Build())
                .AddPolicy(Roles.Admin,
                    policy => policy.RequireAssertion(_ => true));
        });
    }

    // The API seeds on startup but does not apply migrations, so the schema has to exist before the host is built.
    private async Task MigrateDatabaseAsync()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IEncryptionProvider>(
            new AesEncryptionProvider(Options.Create(new EncryptionSettings { Key = _encryptionKey })));
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(ConnectionString));

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
    }
}
