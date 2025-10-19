# CareNest Order Service - APIService Integration Guide

## Tổng quan

Dự án CareNest Order Service sử dụng một kiến trúc microservices với APIService để giao tiếp với các service khác trong hệ thống. APIService được thiết kế để cung cấp một interface thống nhất cho việc gọi API đến các service khác nhau.

## Kiến trúc APIService

### 1. Interface IAPIService

```csharp
public interface IAPIService
{
    Task<ResponseResult<T>> GetAsync<T>(string serviceType, string endpoint);
    Task<ResponseResult<T>> PostAsync<T>(string serviceType, string endpoint, object data);
    Task<ResponseResult<T>> PutAsync<T>(string serviceType, string endpoint, object data);
    Task<ResponseResult<T>> DeleteAsync<T>(string serviceType, string endpoint);
}
```

### 2. Implementation APIService

APIService sử dụng HttpClient để thực hiện các HTTP request và tự động xử lý:
- Serialization/Deserialization JSON
- Error handling
- Response mapping
- Base URL resolution

## Cấu hình BaseUrl

### 1. APIServiceOption Class

```csharp
public class APIServiceOption
{
    public string BaseUrlOrderDetail { get; set; } = string.Empty;
    public string BaseUrlShop { get; set; } = string.Empty;
    public string BaseUrlAddress { get; set; } = string.Empty;
    public string BaseUrlProduct { get; set; } = string.Empty;
    public string BaseUrlAuthorize { get; set; } = string.Empty;
}
```

### 2. Cấu hình trong appsettings.json

```json
{
  "APIService": {
    "BaseUrlOrderDetail": "http://192.168.0.25:8080",
    "BaseUrlShop": "http://192.168.0.18:8080",
    "BaseUrlAddress": "http://192.168.0.26:8080",
    "BaseUrlProduct": "http://192.168.0.17:8080",
    "BaseUrlAuthorize": "http://192.168.0.12:8080"
  }
}
```

### 3. Đăng ký Service trong Program.cs

```csharp
// Đăng ký APIServiceOption
builder.Services.Configure<APIServiceOption>(
    builder.Configuration.GetSection("APIService")
);

// Đăng ký IAPIService với HttpClient
builder.Services.AddHttpClient<IAPIService, APIService>();
```

## Cách sử dụng APIService

### 1. Dependency Injection

```csharp
public class CreateCommandHandler : ICommandHandler<CreateCommand, Order>
{
    private readonly IAPIService _apiService;

    public CreateCommandHandler(IAPIService apiService)
    {
        _apiService = apiService;
    }
}
```

### 2. Các loại Service Type được hỗ trợ

APIService hỗ trợ các service type sau:
- `"orderdetail"` → BaseUrlOrderDetail
- `"shop"` → BaseUrlShop  
- `"address"` → BaseUrlAddress
- `"product"` → BaseUrlProduct
- `"authorize"` → BaseUrlAuthorize

### 3. Ví dụ sử dụng

#### GET Request
```csharp
// Lấy thông tin shop
var shopResult = await _apiService.GetAsync<ShopDto>("shop", $"/api/Shop/{shopId}");
if (shopResult.IsSuccess)
{
    var shopData = shopResult.Data;
    // Xử lý dữ liệu
}
else
{
    // Xử lý lỗi
    throw new BadRequestException($"Shop không tồn tại: {shopResult.Message}");
}
```

#### POST Request
```csharp
// Tạo OrderDetail
var payload = new
{
    productDetailId = item.ProductDetailId,
    orderId = order.Id,
    quantity = item.Quantity
};

var result = await _apiService.PostAsync<JsonElement>("orderdetail", "/api/OrderDetail", payload);
if (!result.IsSuccess)
{
    throw new Exception(result.Message ?? "Tạo OrderDetail thất bại");
}
```

#### PUT Request
```csharp
// Cập nhật dữ liệu
var updateData = new { name = "New Name" };
var result = await _apiService.PutAsync<object>("shop", $"/api/Shop/{shopId}", updateData);
```

#### DELETE Request
```csharp
// Xóa dữ liệu
var result = await _apiService.DeleteAsync<object>("shop", $"/api/Shop/{shopId}");
```

## Response Format

### 1. ApiResponse<T> (Từ external service)

```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}
```

### 2. ResponseResult<T> (Internal response)

```csharp
public class ResponseResult<T>
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
}
```

## Error Handling

APIService tự động xử lý các trường hợp lỗi:

1. **HTTP Error Status Codes**: Trả về ResponseResult với IsSuccess = false
2. **JSON Deserialization Errors**: Bắt exception và trả về error message
3. **Network Errors**: Bắt exception và trả về error message
4. **Invalid Service Type**: Throw ArgumentException

## Best Practices

### 1. Validation trước khi gọi API
```csharp
if (!string.IsNullOrWhiteSpace(command.ShopId))
{
    var shopCheckResult = await _apiService.GetAsync<ShopDto>("shop", $"/api/Shop/{command.ShopId}");
    if (!shopCheckResult.IsSuccess)
    {
        throw new BadRequestException($"Shop không hợp lệ: {shopCheckResult.Message}");
    }
}
```

### 2. Error Handling
```csharp
var result = await _apiService.GetAsync<ShopDto>("shop", $"/api/Shop/{shopId}");
if (!result.IsSuccess)
{
    // Log error hoặc throw exception tùy theo business logic
    _logger.LogError($"Failed to get shop {shopId}: {result.Message}");
    return null;
}
```

### 3. Async/Await Pattern
```csharp
// Luôn sử dụng async/await
public async Task<Order> HandleAsync(CreateCommand command)
{
    var shopResult = await _apiService.GetAsync<ShopDto>("shop", $"/api/Shop/{command.ShopId}");
    // ...
}
```

## Cách áp dụng cho Solution khác

### 1. Tạo APIServiceOption
```csharp
public class APIServiceOption
{
    public string BaseUrlService1 { get; set; } = string.Empty;
    public string BaseUrlService2 { get; set; } = string.Empty;
    // Thêm các service khác...
}
```

### 2. Cập nhật APIService.GetBaseUrl()
```csharp
private string GetBaseUrl(string serviceType)
{
    return serviceType.ToLower() switch
    {
        "service1" => _option.BaseUrlService1,
        "service2" => _option.BaseUrlService2,
        // Thêm mapping cho service mới
        _ => throw new ArgumentException($"Service type '{serviceType}' không hợp lệ!", nameof(serviceType))
    };
}
```

### 3. Cấu hình appsettings.json
```json
{
  "APIService": {
    "BaseUrlService1": "http://your-service1-url:port",
    "BaseUrlService2": "http://your-service2-url:port"
  }
}
```

### 4. Đăng ký trong Program.cs
```csharp
builder.Services.Configure<APIServiceOption>(
    builder.Configuration.GetSection("APIService")
);
builder.Services.AddHttpClient<IAPIService, APIService>();
```

## Monitoring và Logging

### 1. Thêm Logging
```csharp
public async Task<ResponseResult<T>> GetAsync<T>(string serviceType, string endpoint)
{
    try
    {
        var baseUrl = GetBaseUrl(serviceType);
        var fullUrl = $"{baseUrl}{endpoint}";
        
        _logger.LogInformation($"Calling API: {fullUrl}");
        
        var response = await _httpClient.GetAsync(fullUrl);
        // ...
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"API call failed for {serviceType}:{endpoint}");
        return ResponseResult<T>.Failure($"API call failed: {ex.Message}");
    }
}
```

### 2. Health Checks
```csharp
// Thêm health check cho các external services
builder.Services.AddHealthChecks()
    .AddCheck<APIServiceHealthCheck>("api-services");
```

## Troubleshooting

### 1. Common Issues

**Lỗi "Service type không hợp lệ"**
- Kiểm tra serviceType có đúng chính tả không
- Đảm bảo đã thêm mapping trong GetBaseUrl()

**Lỗi "Connection refused"**
- Kiểm tra BaseUrl có đúng không
- Kiểm tra service đích có đang chạy không
- Kiểm tra firewall/network connectivity

**Lỗi JSON Deserialization**
- Kiểm tra DTO class có match với response format không
- Kiểm tra PropertyNameCaseInsensitive setting

### 2. Debug Tips

```csharp
// Thêm logging để debug
_logger.LogInformation($"Calling {serviceType} at {fullUrl}");
_logger.LogInformation($"Response: {content}");
```

## Kết luận

APIService trong CareNest Order Service cung cấp một cách tiếp cận thống nhất và dễ sử dụng để giao tiếp với các microservices khác. Với kiến trúc này, việc thêm service mới hoặc thay đổi endpoint trở nên đơn giản và maintainable.

Để áp dụng cho solution khác, chỉ cần:
1. Copy APIService và IAPIService
2. Tạo APIServiceOption phù hợp
3. Cấu hình BaseUrl trong appsettings
4. Đăng ký service trong DI container
5. Sử dụng trong business logic
