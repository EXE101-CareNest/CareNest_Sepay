using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using CareNest_SePay.Application.Interfaces.Services;
using CareNest_SePay.Domain.Entities;
using CareNest_SePay.Domain.Commons.Enums;
using CareNest_SePay.Domain.Commons.Constants;
using System.Text;

namespace CareNest_SePay.Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentService> _logger;
        private readonly HttpClient _httpClient;

        public PaymentService(IConfiguration configuration, ILogger<PaymentService> logger, HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<SepayTransaction> ProcessPaymentAsync(object webhookData)
        {
            try
            {
                var jsonData = JsonConvert.SerializeObject(webhookData);
                _logger.LogInformation($"Processing payment webhook: {jsonData}");

                // Parse webhook data (this would be specific to Sepay's webhook format)
                var transaction = ParseWebhookData(webhookData);

                return transaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment webhook");
                throw;
            }
        }

        public async Task<SepayTransaction> UpdateTransactionStatusAsync(int transactionId, string status)
        {
            // This would typically involve calling Sepay's API to update transaction status
            // For now, we'll just log the action
            _logger.LogInformation($"Updating transaction {transactionId} status to {status}");
            
            // Return a mock transaction - in real implementation, this would call Sepay API
            return new SepayTransaction
            {
                TransactionId = transactionId,
                Status = Enum.TryParse<TransactionStatus>(status, out var parsedStatus) ? parsedStatus : TransactionStatus.Pending
            };
        }

        public async Task<bool> ValidateWebhookSignatureAsync(string signature, string payload)
        {
            try
            {
                // In a real implementation, you would validate the signature using Sepay's secret key
                // This is a simplified version
                var secretKey = _configuration["Sepay:SecretKey"];
                
                if (string.IsNullOrEmpty(secretKey))
                {
                    _logger.LogWarning("Sepay secret key not configured");
                    return false;
                }

                // Here you would implement the actual signature validation logic
                // For now, we'll just return true if signature is provided
                return !string.IsNullOrEmpty(signature);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating webhook signature");
                return false;
            }
        }

        public async Task<SepayTransaction> CreateTestTransactionAsync(decimal amount, string description = "Test Transaction")
        {
            try
            {
                var baseUrl = _configuration["Sepay:BaseUrl"];
                var apiKey = _configuration["Sepay:ApiKey"];
                var environment = _configuration["Sepay:Environment"];

                _logger.LogInformation($"Creating test transaction in {environment} environment");

                // Create test transaction data
                var testTransaction = new SepayTransaction
                {
                    TransactionId = new Random().Next(100000, 999999),
                    Gateway = PaymentGateway.Sepay,
                    TransactionDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    AccountNumber = _configuration["Sepay:AccountNumber"],
                    SubAccount = "TEST",
                    AmountIn = amount,
                    AmountOut = 0,
                    Accumulated = amount,
                    Code = "TEST_CODE",
                    TransactionContent = description,
                    ReferenceNumber = $"TEST_{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
                    Body = JsonConvert.SerializeObject(new { test = true, amount = amount }),
                    Status = TransactionStatus.Pending
                };

                _logger.LogInformation($"Test transaction created: {JsonConvert.SerializeObject(testTransaction)}");

                return testTransaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating test transaction");
                throw;
            }
        }

        public async Task<bool> SendTestWebhookAsync(SepayTransaction transaction)
        {
            try
            {
                var webhookUrl = _configuration["Sepay:WebhookUrl"];
                var baseUrl = _configuration["Sepay:BaseUrl"];
                var environment = _configuration["Sepay:Environment"];

                _logger.LogInformation($"Sending test webhook for transaction {transaction.TransactionId} in {environment}");

                // In sandbox mode, we can simulate webhook calls
                if (environment == "Sandbox")
                {
                    _logger.LogInformation("Sandbox mode: Simulating webhook call");
                    
                    // Simulate webhook processing
                    await Task.Delay(1000); // Simulate network delay
                    
                    _logger.LogInformation($"Test webhook sent successfully for transaction {transaction.TransactionId}");
                    return true;
                }

                // In production, you would send actual webhook to your endpoint
                var webhookData = new
                {
                    transactionId = transaction.TransactionId,
                    transactionDate = transaction.TransactionDate,
                    accountNumber = transaction.AccountNumber,
                    subAccount = transaction.SubAccount,
                    amountIn = transaction.AmountIn,
                    amountOut = transaction.AmountOut,
                    accumulated = transaction.Accumulated,
                    code = transaction.Code,
                    transactionContent = transaction.TransactionContent,
                    referenceNumber = transaction.ReferenceNumber,
                    status = transaction.Status.ToString()
                };

                var json = JsonConvert.SerializeObject(webhookData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Add headers
                content.Headers.Add("X-Sepay-Signature", GenerateTestSignature(json));
                content.Headers.Add("Authorization", $"Bearer {_configuration["Sepay:ApiKey"]}");

                var response = await _httpClient.PostAsync($"{baseUrl}{webhookUrl}", content);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Test webhook sent successfully for transaction {transaction.TransactionId}");
                    return true;
                }
                else
                {
                    _logger.LogError($"Failed to send test webhook: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test webhook");
                return false;
            }
        }

        private string GenerateTestSignature(string payload)
        {
            // In sandbox mode, generate a simple test signature
            var secretKey = _configuration["Sepay:SecretKey"];
            var combined = $"{payload}{secretKey}";
            
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
                return Convert.ToHexString(hash).ToLower();
            }
        }

        private SepayTransaction ParseWebhookData(object webhookData)
        {
            try
            {
                var jsonData = JsonConvert.SerializeObject(webhookData);
                var dynamicData = JsonConvert.DeserializeObject<dynamic>(jsonData);

                // Parse the webhook data according to Sepay's format
                // This is a simplified version - you would need to adjust based on actual Sepay webhook format
                var transaction = new SepayTransaction
                {
                    TransactionId = dynamicData?.transactionId ?? 0,
                    Gateway = PaymentGateway.Sepay,
                    TransactionDate = dynamicData?.transactionDate ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    AccountNumber = dynamicData?.accountNumber?.ToString() ?? string.Empty,
                    SubAccount = dynamicData?.subAccount?.ToString() ?? string.Empty,
                    AmountIn = decimal.TryParse(dynamicData?.amountIn?.ToString(), out decimal amountIn) ? amountIn : 0,
                    AmountOut = decimal.TryParse(dynamicData?.amountOut?.ToString(), out decimal amountOut) ? amountOut : 0,
                    Accumulated = decimal.TryParse(dynamicData?.accumulated?.ToString(), out decimal accumulated) ? accumulated : 0,
                    Code = dynamicData?.code?.ToString() ?? string.Empty,
                    TransactionContent = dynamicData?.transactionContent?.ToString() ?? string.Empty,
                    ReferenceNumber = dynamicData?.referenceNumber?.ToString() ?? string.Empty,
                    Body = jsonData,
                    Status = TransactionStatus.Pending
                };

                return transaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing webhook data");
                throw;
            }
        }
    }
}
