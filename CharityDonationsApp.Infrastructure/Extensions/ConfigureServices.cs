using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using CharityDonationsApp.Application.Common.Contracts.Abstractions;
using CharityDonationsApp.Application.Common.Contracts.Abstractions.Repositories;
using CharityDonationsApp.Domain.Constants;
using CharityDonationsApp.Domain.Entities;
using CharityDonationsApp.Infrastructure.Configurations;
using CharityDonationsApp.Infrastructure.Persistence;
using CharityDonationsApp.Infrastructure.Persistence.DbContexts;
using CharityDonationsApp.Infrastructure.Persistence.Repositories;
using CharityDonationsApp.Infrastructure.Services;
using CharityDonationsApp.Infrastructure.Services.Integrations.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Polly;

namespace CharityDonationsApp.Infrastructure.Extensions;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions();
        services.AddDbRegistration(config);
        services.AddJwtAuth(config);
        services.AddServices();
        services.AddHttpClientExtensions(config);
        return services;
    }

    private static void AddDbRegistration(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        services.AddSingleton<IDbConnectionFactory>(_ =>
            new SqlConnectionFactory(config.GetConnectionString("DefaultConnection")!));

        services.AddIdentity<User, IdentityRole<Guid>>(
                options =>
                {
                    options.User.RequireUniqueEmail = true;
                    options.Password.RequiredLength = 8;
                })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
    }

    private static void AddJwtAuth(this IServiceCollection services, IConfiguration config)
    {
        var jwtSettings = config.GetSection("Security:Jwt").Get<JwtSettings>()!;
        var rsa = RSA.Create();
        rsa.ImportFromPem(jwtSettings.PublicKey);

        services.AddAuthentication(opts =>
            {
                opts.DefaultScheme =
                    opts.DefaultAuthenticateScheme =
                        opts.DefaultChallengeScheme =
                            opts.DefaultSignInScheme =
                                opts.DefaultForbidScheme =
                                    opts.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opts => opts.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new RsaSecurityKey(rsa),
                ClockSkew = TimeSpan.Zero // no extra time beyond expiration
            });

        services.AddAuthorization();
        // services.AddAuthorizationBuilder()
        //     .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
    }

    private static void AddOptions(this IServiceCollection services)
    {
        services.AddOptions<JwtSettings>()
            .BindConfiguration(JwtSettings.Path)
            .ValidateOnStart();

        services.AddOptions<ApiEndpoints>()
            .BindConfiguration(ApiEndpoints.Path)
            .ValidateOnStart();

        services.AddOptions<EncryptionSettings>()
            .BindConfiguration(EncryptionSettings.Path)
            .ValidateOnStart();

        services.AddOptions<EmailSettings>()
            .BindConfiguration(EmailSettings.Path)
            .ValidateOnStart();

        services.AddOptions<AlatPaySettings>()
            .BindConfiguration(AlatPaySettings.Path)
            .ValidateOnStart();
    }

    private static void AddServices(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddHttpContextAccessor();
        services.AddHostedService<QueuedHostedService>();
        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        services.AddSingleton<IEncryptionProvider, AesEncryptionProvider>();
        services.AddScoped<IUtilityService, UtilityService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IApiClient, ApiClient>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IKycVerificationRepository, KycVerificationRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    private static void AddHttpClientExtensions(this IServiceCollection services, IConfiguration config)
    {
        services.AddHttpClient();

        services.AddHttpClient(HttpClientNames.AlatPayClient, client =>
            {
                var baseUrl = config[AlatPaySettings.Path + ":BaseUrl"]!;
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            })
            .AddHttpMessageHandler<AuthHeaderHandler<AlatPayAuthHeaderProvider>>()
            .ConfigureStandardPolicies(services);
    }

    private static IHttpClientBuilder ConfigureStandardPolicies(this IHttpClientBuilder builder,
        IServiceCollection services)
    {
        using var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<ApiClient>>();

        // Configurable values (would ideally come from config)
        var retryCount = 3;
        var breakDuration = TimeSpan.FromSeconds(30);
        var handledEventsBeforeBreaking = 5;
        var timeoutDuration = TimeSpan.FromSeconds(30);

        var retryPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.RequestTimeout || (int)msg.StatusCode >= 500)
            .WaitAndRetryAsync(
                retryCount,
                attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                (outcome, timespan, retryAttempt, context) =>
                {
                    logger.LogWarning(
                        $"Retry {retryAttempt} after {timespan.TotalSeconds}s due to: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
                });

        var circuitBreakerPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.RequestTimeout || (int)msg.StatusCode >= 500)
            .CircuitBreakerAsync(
                handledEventsBeforeBreaking,
                breakDuration,
                (outcome, breakDelay) =>
                {
                    logger.LogError(
                        $"Circuit broken! Breaking for {breakDelay.TotalSeconds}s due to: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
                },
                () => logger.LogInformation("Circuit reset."),
                () => logger.LogInformation("Circuit is half-open, next call is a trial."));

        var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(timeoutDuration);

        return builder
            .AddPolicyHandler(retryPolicy)
            .AddPolicyHandler(circuitBreakerPolicy)
            .AddPolicyHandler(timeoutPolicy)
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
                MaxConnectionsPerServer = 10
            });
    }
}