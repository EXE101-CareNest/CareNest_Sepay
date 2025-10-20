using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using CareNest_SePay.Application.Interfaces.CQRS;
using CareNest_SePay.Application.Interfaces.UOW;
using CareNest_SePay.Application.Interfaces.Services;
using CareNest_SePay.Application.Features.Commands;
using CareNest_SePay.Domain.Entities;
using CareNest_SePay.Domain.Commons.Enums;
using CareNest_SePay.Domain.Commons.Constants;
using CareNest_SePay.Application.Services;

namespace CareNest_SePay.Application.Features.Handlers
{
    public class ProcessWebhookCommandHandler : ICommandHandler<ProcessWebhookCommand, SepayTransaction>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProcessWebhookCommandHandler> _logger;
        private readonly IAPIService _apiService;
        private readonly OrderIdExtractionService _orderIdExtractionService;

        public ProcessWebhookCommandHandler(
            IUnitOfWork unitOfWork,
            IPaymentService paymentService,
            IConfiguration configuration,
            ILogger<ProcessWebhookCommandHandler> logger,
            IAPIService apiService,
            OrderIdExtractionService orderIdExtractionService)
        {
            _unitOfWork = unitOfWork;
            _paymentService = paymentService;
            _configuration = configuration;
            _logger = logger;
            _apiService = apiService;
            _orderIdExtractionService = orderIdExtractionService;
        }

        public async Task<SepayTransaction> HandleAsync(ProcessWebhookCommand command)
        {
            try
            {
                // API Key is validated at controller level; no need to re-validate here

                // Validate webhook signature if provided
                if (!string.IsNullOrEmpty(command.Signature))
                {
                    var payload = JsonConvert.SerializeObject(command.WebhookData);
                    var isValidSignature = await _paymentService.ValidateWebhookSignatureAsync(command.Signature, payload);
                    if (!isValidSignature)
                    {
                        _logger.LogWarning("Invalid webhook signature");
                        throw new UnauthorizedAccessException("Invalid webhook signature");
                    }
                }

                _logger.LogInformation($"Processing webhook: {JsonConvert.SerializeObject(command.WebhookData)}");

                // Process the webhook data using parsed payload if available
                var transaction = command.WebhookPayload != null 
                    ? await _paymentService.ProcessPaymentAsync(command.WebhookPayload)
                    : await _paymentService.ProcessPaymentAsync(command.WebhookData);

                // Save to database
                await _unitOfWork.SepayTransactionRepository.AddAsync(transaction);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Webhook processed successfully. Transaction ID: {transaction.Id}");

                // Extract OrderId and call Order Service API
                await CallOrderServiceAPI(transaction);

                return transaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook");
                throw;
            }
        }

        private async Task CallOrderServiceAPI(SepayTransaction transaction)
        {
            try
            {
                // Extract OrderId from TransactionContent
                var orderId = _orderIdExtractionService.ExtractOrderId(transaction.TransactionContent);
                
                if (string.IsNullOrWhiteSpace(orderId))
                {
                    _logger.LogWarning($"Could not extract OrderId from TransactionContent: {transaction.TransactionContent}");
                    return;
                }

                _logger.LogInformation($"Calling Order Service API for OrderId: {orderId}");

                // Call Order Service API
                var result = await _apiService.PutAsync<object>("order", $"/api/Order/update-status-to-cancel/{orderId}", new { });
                
                if (result.IsSuccess)
                {
                    _logger.LogInformation($"Successfully updated Order status for OrderId: {orderId}");
                }
                else
                {
                    _logger.LogError($"Failed to update Order status for OrderId: {orderId}. Error: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calling Order Service API for Transaction: {transaction.Id}");
                // Don't throw exception here to avoid breaking the webhook processing
            }
        }
    }
}
