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

namespace CareNest_SePay.Application.Features.Handlers
{
    public class ProcessWebhookCommandHandler : ICommandHandler<ProcessWebhookCommand, SepayTransaction>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProcessWebhookCommandHandler> _logger;

        public ProcessWebhookCommandHandler(
            IUnitOfWork unitOfWork,
            IPaymentService paymentService,
            IConfiguration configuration,
            ILogger<ProcessWebhookCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _paymentService = paymentService;
            _configuration = configuration;
            _logger = logger;
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

                return transaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook");
                throw;
            }
        }
    }
}
