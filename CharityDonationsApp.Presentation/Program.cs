using Carter;
using CharityDonationsApp.Application.Extensions;
using CharityDonationsApp.Infrastructure.Extensions;
using CharityDonationsApp.Infrastructure.Persistence;
using CharityDonationsApp.Presentation.Extensions;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddPresentation()
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapScalarApiReference(options =>
{
    options.WithTitle("Charity Donations API")
        .WithTheme(ScalarTheme.BluePlanet)
        .WithTagSorter(TagSorter.Alpha)
        .WithOperationSorter(OperationSorter.Alpha)
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
        .AddPreferredSecuritySchemes("Bearer");
});

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapCarter();

using var scope = app.Services.CreateScope();
await DbSeeder.SeedAsync(scope.ServiceProvider);

app.Run();

public sealed class ApiEntryPoint
{
}