using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CareNest_SePay.Application.Interfaces.CQRS;
using CareNest_SePay.Application.Interfaces.Services;
using CareNest_SePay.Application.Features.Commands;
using CareNest_SePay.Domain.Entities;
using CareNest_SePay.Application.DTOs;
using System.Threading.Channels;

namespace CareNest_SePay.Application.Services
{
    public class WebhookBackgroundService : BackgroundService, IWebhookBackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WebhookBackgroundService> _logger;
        private readonly Channel<WebhookProcessingRequest> _webhookChannel;

        public WebhookBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<WebhookBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _webhookChannel = Channel.CreateUnbounded<WebhookProcessingRequest>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Webhook Background Service started");

            await foreach (var webhookRequest in _webhookChannel.Reader.ReadAllAsync(stoppingToken))
            {
                _logger.LogInformation($"Background service received webhook: {webhookRequest.TransactionId}");
                try
                {
                    await ProcessWebhookAsync(webhookRequest);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing webhook in background: {webhookRequest.TransactionId}");
                }
            }
        }

        public async Task QueueWebhookAsync(WebhookProcessingRequest request)
        {
            await _webhookChannel.Writer.WriteAsync(request);
            _logger.LogInformation($"Webhook queued for background processing: {request.TransactionId}");
        }

        private async Task ProcessWebhookAsync(WebhookProcessingRequest request)
        {
            var startTime = DateTime.UtcNow;
            
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dispatcher = scope.ServiceProvider.GetRequiredService<IUseCaseDispatcher>();

                _logger.LogInformation($"Processing webhook in background - ID: {request.TransactionId}");

                var command = new ProcessWebhookCommand
                {
                    WebhookPayload = request.WebhookPayload,
                    ApiKey = request.ApiKey,
                    Signature = request.Signature
                };

                var transaction = await dispatcher.DispatchAsync<ProcessWebhookCommand, SepayTransaction>(command);

                var processingTime = DateTime.UtcNow - startTime;
                _logger.LogInformation($"Background webhook processed successfully - Transaction ID: {transaction.TransactionId}, Processing time: {processingTime.TotalMilliseconds}ms");
            }
            catch (Exception ex)
            {
                var processingTime = DateTime.UtcNow - startTime;
                _logger.LogError(ex, $"Error processing webhook in background after {processingTime.TotalMilliseconds}ms - Transaction ID: {request.TransactionId}");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _webhookChannel.Writer.Complete();
            await base.StopAsync(cancellationToken);
            _logger.LogInformation("Webhook Background Service stopped");
        }
    }

    public class WebhookProcessingRequest
    {
        public long TransactionId { get; set; }
        public SepayWebhookPayload WebhookPayload { get; set; } = null!;
        public string ApiKey { get; set; } = string.Empty;
        public string? Signature { get; set; }
        public DateTime QueuedAt { get; set; } = DateTime.UtcNow;
    }
}
