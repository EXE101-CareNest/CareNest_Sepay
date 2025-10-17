using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CareNest_SePay.Application.Interfaces.Services;
using CareNest_SePay.Domain.Entities;
using CareNest_SePay.Domain.Commons.Enums;
using Newtonsoft.Json;
using System.Text;

namespace CareNest_SePay.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentService> _logger;
        private readonly ISepayAPIService _sepayAPIService;

        public PaymentService(
            IConfiguration configuration, 
            ILogger<PaymentService> logger,
            ISepayAPIService sepayAPIService)
        {
            _configuration = configuration;
            _logger = logger;
            _sepayAPIService = sepayAPIService;
        }

        public Task<SepayTransaction> ProcessPaymentAsync(object webhookData)
        {
            try
            {
                var jsonData = JsonConvert.SerializeObject(webhookData);
                _logger.LogInformation($"Processing payment webhook: {jsonData}");

                // Parse webhook data (this would be specific to Sepay's webhook format)
                var transaction = ParseWebhookData(webhookData);

                return Task.FromResult(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment webhook");
                throw;
            }
        }

        public Task<SepayTransaction> UpdateTransactionStatusAsync(int transactionId, string status)
        {
            // This would typically involve calling Sepay's API to update transaction status
            // For now, we'll just log the action
            _logger.LogInformation($"Updating transaction {transactionId} status to {status}");
            
            // Return a mock transaction - in real implementation, this would call Sepay API
            var transaction = new SepayTransaction
            {
                TransactionId = transactionId,
                Status = Enum.TryParse<TransactionStatus>(status, out var parsedStatus) ? parsedStatus : TransactionStatus.Pending
            };
            
            return Task.FromResult(transaction);
        }

        public Task<bool> ValidateWebhookSignatureAsync(string signature, string payload)
        {
            try
            {
                // In a real implementation, you would validate the signature using Sepay's secret key
                // This is a simplified version
                var secretKey = _configuration["Sepay:SecretKey"];
                
                if (string.IsNullOrEmpty(secretKey))
                {
                    _logger.LogWarning("Sepay secret key not configured");
                    return Task.FromResult(false);
                }

                // Here you would implement the actual signature validation logic
                // For now, we'll just return true if signature is provided
                return Task.FromResult(!string.IsNullOrEmpty(signature));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating webhook signature");
                return Task.FromResult(false);
            }
        }

        public Task<SepayTransaction> CreateTestTransactionAsync(decimal amount, string description = "Test Transaction")
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
                    AccountNumber = _configuration["Sepay:AccountNumber"] ?? "TEST_ACCOUNT",
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

                return Task.FromResult(testTransaction);
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

                var signature = GenerateTestSignature(JsonConvert.SerializeObject(webhookData));
                var success = await _sepayAPIService.SendWebhookAsync($"{baseUrl}{webhookUrl}", webhookData, signature);
                
                if (success)
                {
                    _logger.LogInformation($"Test webhook sent successfully for transaction {transaction.TransactionId}");
                    return true;
                }
                else
                {
                    _logger.LogError($"Failed to send test webhook for transaction {transaction.TransactionId}");
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

                _logger.LogInformation($"Parsing SePay webhook data: {jsonData}");

                // Parse the webhook data according to SePay's webhook format
                // Based on SePay documentation: https://sepay.vn/lap-trinh-cong-thanh-toan.html
                var transaction = new SepayTransaction
                {
                    TransactionId = GetLongValue(dynamicData?.transactionId) ?? 0,
                    Gateway = PaymentGateway.Sepay,
                    TransactionDate = GetLongValue(dynamicData?.transactionDate) ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    AccountNumber = dynamicData?.accountNumber?.ToString() ?? string.Empty,
                    SubAccount = dynamicData?.subAccount?.ToString() ?? string.Empty,
                    AmountIn = GetDecimalValue(dynamicData?.amountIn),
                    AmountOut = GetDecimalValue(dynamicData?.amountOut),
                    Accumulated = GetDecimalValue(dynamicData?.accumulated),
                    Code = dynamicData?.code?.ToString() ?? string.Empty,
                    TransactionContent = dynamicData?.transactionContent?.ToString() ?? string.Empty,
                    ReferenceNumber = dynamicData?.referenceNumber?.ToString() ?? string.Empty,
                    Body = jsonData,
                    Status = DetermineTransactionStatus(dynamicData),
                    ProcessedAt = DateTime.UtcNow
                };

                _logger.LogInformation($"Parsed transaction: ID={transaction.TransactionId}, Amount={transaction.AmountIn}, Status={transaction.Status}");

                return transaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing webhook data");
                throw;
            }
        }

        private long? GetLongValue(dynamic value)
        {
            if (value == null) return null;
            
            if (long.TryParse(value.ToString(), out long result))
                return result;
                
            return null;
        }

        private decimal GetDecimalValue(dynamic value)
        {
            if (value == null) return 0;
            
            if (decimal.TryParse(value.ToString(), out decimal result))
                return result;
                
            return 0;
        }

        private TransactionStatus DetermineTransactionStatus(dynamic webhookData)
        {
            try
            {
                // Dựa trên tài liệu SePay, xác định trạng thái giao dịch
                var status = webhookData?.status?.ToString()?.ToLower();
                var amountIn = GetDecimalValue(webhookData?.amountIn);
                var amountOut = GetDecimalValue(webhookData?.amountOut);

                // Nếu có tiền vào (amountIn > 0) và không có tiền ra (amountOut = 0)
                if (amountIn > 0 && amountOut == 0)
                {
                    return TransactionStatus.Completed;
                }
                
                // Nếu có tiền ra (amountOut > 0)
                if (amountOut > 0)
                {
                    return TransactionStatus.Refunded;
                }

                // Dựa trên status field nếu có
                return status switch
                {
                    "completed" or "success" => TransactionStatus.Completed,
                    "failed" or "error" => TransactionStatus.Failed,
                    "cancelled" or "cancel" => TransactionStatus.Cancelled,
                    "processing" => TransactionStatus.Processing,
                    _ => TransactionStatus.Pending
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error determining transaction status, defaulting to Pending");
                return TransactionStatus.Pending;
            }
        }
    }
}
