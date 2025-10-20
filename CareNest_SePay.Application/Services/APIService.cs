using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using CareNest_SePay.Application.Interfaces.Services;
using CareNest_SePay.Application.Common;
using System.Text;
using System.Text.Json;

namespace CareNest_SePay.Application.Services
{
    public class APIService : IAPIService
    {
        private readonly HttpClient _httpClient;
        private readonly APIServiceOption _option;
        private readonly ILogger<APIService> _logger;

        public APIService(HttpClient httpClient, IOptions<APIServiceOption> option, ILogger<APIService> logger)
        {
            _httpClient = httpClient;
            _option = option.Value;
            _logger = logger;
        }

        public async Task<ResponseResult<T>> GetAsync<T>(string serviceType, string endpoint)
        {
            try
            {
                var baseUrl = GetBaseUrl(serviceType);
                var fullUrl = $"{baseUrl}{endpoint}";
                
                _logger.LogInformation($"Calling API: {fullUrl}");
                
                var response = await _httpClient.GetAsync(fullUrl);
                var content = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var data = JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return ResponseResult<T>.Success(data!);
                }
                else
                {
                    return ResponseResult<T>.Failure($"API call failed with status {response.StatusCode}: {content}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"API call failed for {serviceType}:{endpoint}");
                return ResponseResult<T>.Failure($"API call failed: {ex.Message}");
            }
        }

        public async Task<ResponseResult<T>> PostAsync<T>(string serviceType, string endpoint, object data)
        {
            try
            {
                var baseUrl = GetBaseUrl(serviceType);
                var fullUrl = $"{baseUrl}{endpoint}";
                
                _logger.LogInformation($"Calling API: {fullUrl}");
                
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(fullUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return ResponseResult<T>.Success(result!);
                }
                else
                {
                    return ResponseResult<T>.Failure($"API call failed with status {response.StatusCode}: {responseContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"API call failed for {serviceType}:{endpoint}");
                return ResponseResult<T>.Failure($"API call failed: {ex.Message}");
            }
        }

        public async Task<ResponseResult<T>> PutAsync<T>(string serviceType, string endpoint, object data)
        {
            try
            {
                var baseUrl = GetBaseUrl(serviceType);
                var fullUrl = $"{baseUrl}{endpoint}";
                
                _logger.LogInformation($"Calling API: {fullUrl}");
                
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PutAsync(fullUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return ResponseResult<T>.Success(result!);
                }
                else
                {
                    return ResponseResult<T>.Failure($"API call failed with status {response.StatusCode}: {responseContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"API call failed for {serviceType}:{endpoint}");
                return ResponseResult<T>.Failure($"API call failed: {ex.Message}");
            }
        }

        public async Task<ResponseResult<T>> DeleteAsync<T>(string serviceType, string endpoint)
        {
            try
            {
                var baseUrl = GetBaseUrl(serviceType);
                var fullUrl = $"{baseUrl}{endpoint}";
                
                _logger.LogInformation($"Calling API: {fullUrl}");
                
                var response = await _httpClient.DeleteAsync(fullUrl);
                var content = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return ResponseResult<T>.Success(result!);
                }
                else
                {
                    return ResponseResult<T>.Failure($"API call failed with status {response.StatusCode}: {content}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"API call failed for {serviceType}:{endpoint}");
                return ResponseResult<T>.Failure($"API call failed: {ex.Message}");
            }
        }

        private string GetBaseUrl(string serviceType)
        {
            return serviceType.ToLower() switch
            {
                "order" => _option.BaseUrlOrder,
                _ => throw new ArgumentException($"Service type '{serviceType}' không hợp lệ!", nameof(serviceType))
            };
        }
    }
}
