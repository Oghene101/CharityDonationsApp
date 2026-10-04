# Charity Donations App

A .NET 9 Web API backend for a charity donations platform. It currently covers user onboarding: authentication, KYC (BVN, NIN and address capture) and a small set of admin operations. The domain model for fundraising events, wallets and transactions, and an ALATPay payment client, are in place but not yet exposed through endpoints.

## Features

- **Authentication** — sign-up with email confirmation, sign-in, refresh tokens, change / forgot / reset password
- **JWT access tokens** signed with RSA (RS256), plus refresh tokens stored in the database
- **Progressive account lockout** — repeated failed sign-ins lock the account for increasingly longer periods
- **KYC** — attach a BVN, NIN and addresses to a user profile; BVN and NIN are AES-256 encrypted at rest and indexed by SHA-256 hash
- **Admin tools** — resend a user's email confirmation, end a user's lockout early
- **Role-based authorization** with `Admin` and `User` roles
- **Consistent error responses** as RFC 7807 problem details via a global exception handler
- **Background email sending** through an in-process queue and hosted service
- **Resilient outbound HTTP** (retry, circuit breaker, timeout) for third-party integrations
- **Interactive API docs** with Scalar

## Tech stack

| Area | Technology |
| --- | --- |
| Framework | ASP.NET Core 9 minimal APIs, organised with [Carter](https://github.com/CarterCommunity/Carter) modules |
| Application layer | MediatR (commands + pipeline behaviours), FluentValidation |
| Persistence | SQL Server, EF Core 9 (writes), Dapper (reads), ASP.NET Core Identity |
| Security | JWT bearer authentication (RSA), AES-256 field encryption |
| Email | MailKit (SMTP) |
| Resilience | Polly |
| API docs | Microsoft.AspNetCore.OpenApi + Scalar |
| Testing | xUnit, FluentAssertions, `Microsoft.AspNetCore.Mvc.Testing`, Testcontainers (SQL Server) |

Package versions are managed centrally in `Directory.Packages.props`; shared build settings live in `Directory.Build.props`.

## Project structure

The solution follows Clean Architecture, with dependencies pointing inwards:

```
CharityDonationsApp.sln
├── CharityDonationsApp.Domain                 Entities, enums, constants, attributes
├── CharityDonationsApp.Application            Use cases (MediatR commands), validators, contracts, abstractions
├── CharityDonationsApp.Infrastructure         EF Core, Dapper repositories, migrations, Identity, JWT, email, integrations
├── CharityDonationsApp.Presentation           API host: Carter modules, middleware, OpenAPI
└── CharityDonationsApp.Api.IntegrationTests   Endpoint tests using WebApplicationFactory
```

Each use case lives in a single file under `Application/Features/<Area>/Commands`, containing its `Command`, `Handler` and `Validator`. A request flows through:

```
Carter module → MediatR → ValidationBehaviour → Handler → UnitOfWork / services
```

## Getting started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download) (a newer SDK also builds the solution, but the .NET 9 runtime is needed to run it)
- A SQL Server instance
- The EF Core CLI: `dotnet tool install --global dotnet-ef`
- An SMTP account for sending emails (for example Gmail with an app password)
- Docker, for the integration tests and for building the container image

### 1. Clone

```bash
git clone https://github.com/Oghene101/CharityDonationsApp.git
cd CharityDonationsApp
```

### 2. Configure

Non-secret defaults (JWT issuer, audience and lifetimes, and the lockout policy) are in `appsettings.json`. Everything else is a secret or specific to your environment and has to be supplied before the API will start:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=CharityDonationsApp;User Id=sa;Password=<password>;TrustServerCertificate=True"
  },
  "Security": {
    "Jwt": {
      "PrivateKey": "-----BEGIN PRIVATE KEY-----\n...\n-----END PRIVATE KEY-----",
      "PublicKey": "-----BEGIN PUBLIC KEY-----\n...\n-----END PUBLIC KEY-----"
    },
    "Encryption": {
      "Key": "<base64 string that decodes to at least 32 bytes>"
    }
  },
  "Email": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "SenderName": "Charity Donations",
    "SenderEmail": "<sender address>",
    "AppPassword": "<smtp password>",
    "ConfirmEmailEndpoint": "https://your-frontend/confirm-email?"
  },
  "ApiEndpoints": {
    "ResetPasswordEndpoint": "https://your-frontend/reset-password?"
  },
  "Integrations": {
    "AlatPay": {
      "BaseUrl": "<ALATPay base url>",
      "ApiKey": "<subscription key>",
      "InitializeCardEndpoint": "<path>",
      "AuthenticateCardEndpoint": "<path>"
    }
  }
}
```

The connection string, the JWT keys and the encryption key are needed at startup; the rest are needed by the features that use them.

Notes:

- `ConfirmEmailEndpoint` and `ResetPasswordEndpoint` are the links sent in emails. `email=...&token=...` is appended directly, so they should end with `?`.
- Lockout duration is `BaseLockoutMinutes × LockoutMultiplier^(lockouts − 1)`, capped at `MaxLockoutMinutes`. With the defaults in `appsettings.json`: 15, 30, 60, 120, then 240 minutes.
- SMTP connects using STARTTLS.

Generate the JWT key pair and the encryption key with OpenSSL:

```bash
openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048 -out private.pem
openssl rsa -in private.pem -pubout -out public.pem
openssl rand -base64 32
```

Keep these values out of source control (`.gitignore` already excludes `*.pem`, `.env` and `appsettings.Local.json`). The API project is set up for [user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets), which is the recommended way to supply them locally:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<connection string>" --project CharityDonationsApp.Presentation
dotnet user-secrets set "Security:Jwt:PrivateKey" "$(cat private.pem)" --project CharityDonationsApp.Presentation
dotnet user-secrets set "Security:Jwt:PublicKey" "$(cat public.pem)" --project CharityDonationsApp.Presentation
dotnet user-secrets set "Security:Encryption:Key" "<base64 key>" --project CharityDonationsApp.Presentation
# ...and so on for the remaining keys
```

Outside development, use environment variables with `__` as the section separator (for example `Security__Jwt__PublicKey`).

### 3. Create the database

Migrations are not applied automatically:

```bash
dotnet ef database update \
  --project CharityDonationsApp.Infrastructure \
  --startup-project CharityDonationsApp.Presentation
```

### 4. Run

```bash
dotnet run --project CharityDonationsApp.Presentation --launch-profile https
```

The API listens on `https://localhost:7295` and `http://localhost:5136`.

- API reference (Scalar): `https://localhost:7295/scalar`
- OpenAPI document (Development only): `https://localhost:7295/openapi/v1.json`

On startup the app makes sure the `Admin` and `User` roles exist. It also creates one account per role when credentials are configured under `Seed:Admin` / `Seed:User` and the role has no members yet. `appsettings.Development.json` configures these for local development:

| Role | Email | Password |
| --- | --- | --- |
| Admin | `admin@example.com` | `Admin@123` |
| User | `user@example.com` | `User@123` |

No accounts are seeded in other environments unless you set them, for example `Seed__Admin__Email` and `Seed__Admin__Password` for a first admin.

### Docker

Build the image from the repository root:

```bash
docker build -f CharityDonationsApp.Presentation/Dockerfile -t charity-donations-api .
```

The container listens on port 8080, runs in the `Production` environment and takes its settings from environment variables:

```bash
docker run --rm -p 8080:8080 \
  -e "ConnectionStrings__DefaultConnection=<connection string>" \
  -e "Security__Jwt__PrivateKey=$(cat private.pem)" \
  -e "Security__Jwt__PublicKey=$(cat public.pem)" \
  -e "Security__Encryption__Key=<base64 key>" \
  charity-donations-api
```

## API

All routes are prefixed with `/api`. Authenticated routes expect an `Authorization: Bearer <access token>` header.

### Authentication — `/api/auth`

| Method | Route | Auth | Description |
| --- | --- | --- | --- |
| POST | `/sign-up` | Anonymous | Register a user and send a confirmation email |
| POST | `/confirm-email` | Anonymous | Confirm an email address with the emailed token |
| POST | `/sign-in` | Anonymous | Sign in; returns access and refresh tokens |
| POST | `/refresh-token` | Anonymous | Exchange an access/refresh token pair for a new pair |
| POST | `/change-password` | Required | Change the signed-in user's password |
| POST | `/forgot-password` | Anonymous | Email a password reset link |
| POST | `/reset-password` | Anonymous | Reset a password with the emailed token |

### KYC — `/api/kyc`

| Method | Route | Auth | Description |
| --- | --- | --- | --- |
| POST | `/add-bvn` | Required | Attach an 11-digit BVN to the signed-in user |
| POST | `/add-nin` | Required | Attach an 11-digit NIN to the signed-in user |
| POST | `/add-address` | Required | Add an address to the signed-in user |

### Admin — `/api/admin`

| Method | Route | Auth | Description |
| --- | --- | --- | --- |
| POST | `/send-email-confirmation` | Admin | Resend a user's email confirmation |
| PATCH | `/fast-forward-lockout` | Admin | End a user's lockout immediately |

### Responses

Successful responses share one envelope:

```json
{
  "isSuccess": true,
  "statusCode": 200,
  "message": "Completed successfully",
  "errors": [],
  "data": {
    "userId": "0198f3c2-...",
    "roles": ["User"],
    "token": {
      "accessToken": "eyJhbGciOiJSUzI1NiIs...",
      "expireMinutes": 15,
      "refreshToken": "..."
    }
  }
}
```

Failures are returned as `application/problem+json`. Validation failures are a `400` with an `errors` dictionary keyed by field; other failures carry the relevant status code (`400`, `401`, `403`, `404`, `500`).

Passwords must be at least 8 characters and include an uppercase letter, a lowercase letter, a digit and a special character.

## Testing

```bash
dotnet test
```

Docker must be running. The integration tests need no configuration and never touch your development database: the test fixture

- starts a throwaway SQL Server container with [Testcontainers](https://dotnet.testcontainers.org/) and applies the migrations to it;
- generates its own JWT key pair and encryption key;
- seeds the account the tests sign in with (see `TestUser.cs`);
- replaces the email service with a no-op, so nothing is sent.

All test classes share one API host and one database through an xUnit collection fixture, so they run one after another.

## License

[MIT](LICENSE) © 2025 Ogheneruemu Karieren
