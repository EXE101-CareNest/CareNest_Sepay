using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

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

                _logger.LogInformation($"Attempting to extract OrderId from: {transactionContent}");

                // Method 1: Try to extract UUID/GUID pattern (32 characters, contains letters and numbers)
                var uuidPattern = @"\b[a-fA-F0-9]{8}-?[a-fA-F0-9]{4}-?[a-fA-F0-9]{4}-?[a-fA-F0-9]{4}-?[a-fA-F0-9]{12}\b";
                var uuidMatch = Regex.Match(transactionContent, uuidPattern);
                if (uuidMatch.Success)
                {
                    var orderId = uuidMatch.Value.Replace("-", ""); // Remove dashes if any
                    _logger.LogInformation($"Extracted OrderId (UUID pattern): {orderId}");
                    return orderId;
                }

                // Method 2: Try to find 32-character hex string (without dashes)
                var hex32Pattern = @"\b[a-fA-F0-9]{32}\b";
                var hex32Match = Regex.Match(transactionContent, hex32Pattern);
                if (hex32Match.Success)
                {
                    var orderId = hex32Match.Value.ToLower();
                    _logger.LogInformation($"Extracted OrderId (32-char hex): {orderId}");
                    return orderId;
                }

                // Method 3: Legacy format - "104638553801 0936993764 Thanh toan don hang 5378fca0c3f74e7db1c905dd763ca7c7"
                // Extract the last part as OrderId
                var parts = transactionContent.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 4)
                {
                    var lastPart = parts[parts.Length - 1];
                    // Check if last part looks like an OrderId (at least 16 characters, alphanumeric)
                    if (lastPart.Length >= 16 && Regex.IsMatch(lastPart, @"^[a-zA-Z0-9]+$"))
                    {
                        _logger.LogInformation($"Extracted OrderId (legacy format): {lastPart}");
                        return lastPart;
                    }
                }

                // Method 4: Split by dots and look for OrderId in the middle parts
                // Format: "MBVCB.11398821034.728033.Thanh toan don hang abe6682b4696476e9dbf9a8c93dcc0a9.CT tu..."
                if (transactionContent.Contains('.'))
                {
                    var dotParts = transactionContent.Split('.', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in dotParts)
                    {
                        var trimmedPart = part.Trim();
                        // Look for 32-character hex string in each part
                        var hexMatch = Regex.Match(trimmedPart, @"\b[a-fA-F0-9]{32}\b");
                        if (hexMatch.Success)
                        {
                            var orderId = hexMatch.Value.ToLower();
                            _logger.LogInformation($"Extracted OrderId (dot-separated format): {orderId}");
                            return orderId;
                        }
                    }
                }

                _logger.LogWarning($"Could not extract OrderId from TransactionContent: {transactionContent}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error extracting OrderId from TransactionContent: {transactionContent}");
                return null;
            }
        }
    }
}
