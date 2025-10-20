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

// Database
builder.Services.AddDbContext<CareNestDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Global Exception Handling
app.UseGlobalExceptionHandling();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
