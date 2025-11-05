using Serilog;
using CareNest_SePay.Infrastructure.Persistence;
using CareNest_SePay.Application.Interfaces.CQRS;
using CareNest_SePay.Application.Interfaces.UOW;
using CareNest_SePay.Application.Interfaces.Services;
using CareNest_SePay.Application.UseCases;
using CareNest_SePay.Application.Features.Handlers;
using CareNest_SePay.Application.Features.Commands;
using CareNest_SePay.Application.Features.Queries;
using CareNest_SePay.Infrastructure.Persistence.Repositories;
using CareNest_SePay.Infrastructure.Persistence.UOW;
using CareNest_SePay.Infrastructure.Services;
using CareNest_SePay.Application.Services;
using CareNest_SePay.Extensions;
using CareNest_SePay.Domain.Entities;
using CareNest_SePay.Application.Common;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container
builder.Services.AddControllers();

// Database Settings
builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection("DatabaseSettings")
);  

var dbSettings = builder.Configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>();
string connectionString;

// Priority: DATABASE_URL (Heroku/Koyeb) > DatabaseSettings > ConnectionStrings
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL")
                 ?? builder.Configuration["DATABASE_URL"];

if (!string.IsNullOrWhiteSpace(databaseUrl))
{
    try
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':', 2);
        var user = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
        var host = uri.Host;
        var port = uri.IsDefaultPort ? 5432 : uri.Port;
        var database = uri.AbsolutePath.Trim('/');

        connectionString = $"Host={host};Port={port};Username={user};Password={password};Database={database};SSL Mode=Require;Trust Server Certificate=true;";
        Log.Information("DB config: Using DATABASE_URL. Host={Host}, Database={Database}", host, database);
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException("Invalid DATABASE_URL format. Expected postgres://user:pass@host:port/db", ex);
    }
}
else if (dbSettings != null && !string.IsNullOrEmpty(dbSettings.Ip))
{
    connectionString = dbSettings.BuildConnectionString();
    Log.Information("DB config: Using DatabaseSettings. Host={Host}, Database={Database}", dbSettings.Ip, dbSettings.Database);
}
else
{
    // Fallback to ConnectionStrings if DatabaseSettings not provided
    connectionString = builder.Configuration.GetConnectionString("PostgresConnection") 
        ?? throw new InvalidOperationException("Database configuration not found. Please configure either DatabaseSettings or ConnectionStrings:PostgresConnection");
    // Try to extract host and db name for visibility (best-effort, no secrets)
    var hostPart = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries)
        .FirstOrDefault(p => p.Trim().StartsWith("Host=", StringComparison.OrdinalIgnoreCase)) ?? "Host=(unknown)";
    var dbPart = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries)
        .FirstOrDefault(p => p.Trim().StartsWith("Database=", StringComparison.OrdinalIgnoreCase)) ?? "Database=(unknown)";
    Log.Information("DB config: Using ConnectionStrings. {HostPart}, {DbPart}", hostPart, dbPart);
}

// Validate connection string to fail fast with clear guidance
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Database connection string is empty. Set DatabaseSettings__Ip, __Port, __User, __Password, __Database (recommended) or provide ConnectionStrings__PostgresConnection as a fallback.");
}

builder.Services.AddDbContext<CareNestDbContext>(options =>
    options.UseNpgsql(connectionString));

// CQRS
builder.Services.AddScoped<IUseCaseDispatcher, UseCaseDispatcher>();

// Command Handlers
builder.Services.AddScoped<ICommandHandler<ProcessWebhookCommand, SepayTransaction>, ProcessWebhookCommandHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateTransactionStatusCommand, SepayTransaction>, UpdateTransactionStatusCommandHandler>();

// Query Handlers
builder.Services.AddScoped<IQueryHandler<GetTransactionByIdQuery, SepayTransaction?>, GetTransactionByIdQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetTransactionsByDateRangeQuery, PageResult<SepayTransaction>>, GetTransactionsByDateRangeQueryHandler>();

// Repository & UOW
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Application Services (Business Logic)
builder.Services.AddScoped<IPaymentService, CareNest_SePay.Application.Services.PaymentService>();
builder.Services.AddScoped<CareNest_SePay.Application.Services.IQRCodeService, CareNest_SePay.Application.Services.QRCodeService>();
builder.Services.AddScoped<CareNest_SePay.Application.Services.OrderIdExtractionService>();

// Background Services for Async Processing
builder.Services.AddSingleton<WebhookBackgroundService>();
builder.Services.AddSingleton<IWebhookBackgroundService>(provider => provider.GetRequiredService<WebhookBackgroundService>());
builder.Services.AddHostedService<WebhookBackgroundService>(provider => provider.GetRequiredService<WebhookBackgroundService>());

// APIService for external API calls
builder.Services.Configure<APIServiceOption>(
    builder.Configuration.GetSection("APIService")
);
builder.Services.AddHttpClient<IAPIService, APIService>();

// Infrastructure Services (External API calls)
builder.Services.AddScoped<ISepayAPIService, SepayAPIService>();

// HttpClient for external API calls
builder.Services.AddHttpClient<ISepayAPIService, SepayAPIService>();

// Global Exception Handling
builder.Services.AddGlobalExceptionHandling();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
// Enable Swagger in Development or if explicitly enabled in configuration
var enableSwagger = app.Environment.IsDevelopment() || 
                    app.Configuration.GetValue<bool>("EnableSwagger", true);

if (enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CareNest SePay API v1");
        c.RoutePrefix = "swagger";
    });
}

// Optionally apply EF Core migrations on startup (controlled by RunMigrations config)
var runMigrations = app.Configuration.GetValue<bool>("RunMigrations", false);
if (runMigrations)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<CareNestDbContext>();
    dbContext.Database.Migrate();
}

// Global Exception Handling
app.UseGlobalExceptionHandling();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Health Check endpoint
app.MapHealthChecks("/health");

app.Run();
