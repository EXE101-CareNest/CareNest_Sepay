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

// Database
var dbSettings = builder.Configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>();
string connectionString;

// Priority: DatabaseSettings > ConnectionStrings (backward compatibility)
if (dbSettings != null && !string.IsNullOrEmpty(dbSettings.Ip))
{
    connectionString = dbSettings.BuildConnectionString();
}
else
{
    // Fallback to ConnectionStrings if DatabaseSettings not provided
    connectionString = builder.Configuration.GetConnectionString("PostgresConnection") 
        ?? throw new InvalidOperationException("Database configuration not found. Please configure either DatabaseSettings or ConnectionStrings:PostgresConnection");
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

// Global Exception Handling
app.UseGlobalExceptionHandling();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Health Check endpoint
app.MapHealthChecks("/health");

app.Run();
