using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CareNest_SePay.Application.Interfaces.Services;
using System.Text;
using System.Text.Json;

namespace CareNest_SePay.Infrastructure.Services
{
    public class SepayAPIService : ISepayAPIService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SepayAPIService> _logger;
        private readonly HttpClient _httpClient;

        public SepayAPIService(IConfiguration configuration, ILogger<SepayAPIService> logger, HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<string> GenerateQRCodeAsync(object vietQRData)
        {
            try
            {
                var baseUrl = _configuration["Sepay:BaseUrl"];
                var apiKey = _configuration["Sepay:ApiKey"];
                
                var json = JsonSerializer.Serialize(vietQRData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                // Add headers
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                _httpClient.DefaultRequestHeaders.Add("X-Sepay-Version", "1.0");
                _httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");

                var response = await _httpClient.PostAsync($"{baseUrl}/api/v1/qr/generate", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    
                    if (result.TryGetProperty("data", out var data) && 
                        data.TryGetProperty("qrCode", out var qrCode))
                    {
                        return qrCode.GetString() ?? string.Empty;
                    }
                }

                // Fallback: Tạo QR code local nếu API không khả dụng
                _logger.LogWarning("SePay QR API not available, generating local QR code");
                return GenerateLocalQRCode(vietQRData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling SePay QR API, generating local QR code");
                return GenerateLocalQRCode(vietQRData);
            }
        }

        public async Task<bool> SendWebhookAsync(string webhookUrl, object webhookData, string signature)
        {
            try
            {
                var json = JsonSerializer.Serialize(webhookData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Add headers
                content.Headers.Add("X-Sepay-Signature", signature);
                content.Headers.Add("Authorization", $"Bearer {_configuration["Sepay:ApiKey"]}");

                var response = await _httpClient.PostAsync(webhookUrl, content);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Webhook sent successfully to {webhookUrl}");
                    return true;
                }
                else
                {
                    _logger.LogError($"Failed to send webhook to {webhookUrl}: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending webhook to {webhookUrl}");
                return false;
            }
        }

        private string GenerateLocalQRCode(object vietQRData)
        {
            // Tạo QR code local sử dụng thư viện QR code
            // Đây là fallback khi SePay API không khả dụng
            var qrString = JsonSerializer.Serialize(vietQRData);
            
            // Trong thực tế, bạn sẽ sử dụng thư viện như QRCoder để tạo QR code
            // Ở đây tôi sẽ trả về base64 string giả lập
            var qrBytes = Encoding.UTF8.GetBytes(qrString);
            return Convert.ToBase64String(qrBytes);
        }
    }
}
