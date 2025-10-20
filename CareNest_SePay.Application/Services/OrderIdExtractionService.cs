using Microsoft.Extensions.Logging;

namespace CareNest_SePay.Application.Services
{
    public class OrderIdExtractionService
    {
        private readonly ILogger<OrderIdExtractionService> _logger;

        public OrderIdExtractionService(ILogger<OrderIdExtractionService> logger)
        {
            _logger = logger;
        }

        public string? ExtractOrderId(string transactionContent)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(transactionContent))
                {
                    _logger.LogWarning("TransactionContent is null or empty");
                    return null;
                }

                // Format: "104638553801 0936993764 Thanh toan don hang 5378fca0c3f74e7db1c905dd763ca7c7"
                // Extract the last part (OrderId)
                var parts = transactionContent.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                
                if (parts.Length < 4)
                {
                    _logger.LogWarning($"Invalid TransactionContent format: {transactionContent}");
                    return null;
                }

                var orderId = parts[parts.Length - 1]; // Last part is OrderId
                
                _logger.LogInformation($"Extracted OrderId: {orderId} from TransactionContent: {transactionContent}");
                
                return orderId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error extracting OrderId from TransactionContent: {transactionContent}");
                return null;
            }
        }
    }
}
