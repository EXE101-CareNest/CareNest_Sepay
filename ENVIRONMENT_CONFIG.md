# Cấu hình Môi trường (Environment Configuration)

## 📖 Cách ASP.NET Core load Configuration

ASP.NET Core load configuration theo thứ tự ưu tiên (từ thấp đến cao):

1. **appsettings.json** - Base configuration
2. **appsettings.{Environment}.json** - Environment-specific configuration
3. **Environment Variables** - Highest priority, override tất cả

### Ví dụ với Production:

Khi `ASPNETCORE_ENVIRONMENT=Production`, ASP.NET Core sẽ:

1. Load `appsettings.json` (base)
2. Load `appsettings.Production.json` (override, merge với base)
3. Apply Environment Variables (override cao nhất)

## 🔧 File Structure

```
CareNest_SePay/
├── appsettings.json                    # Base config (có thể có test values)
├── appsettings.Development.json        # Development config
└── appsettings.Production.json         # Production config (không có sensitive data)
```

## ⚙️ appsettings.Production.json

File này chứa:
- ✅ **Cấu hình structure** - Định nghĩa các sections cần thiết
- ✅ **Logging config** - Production-optimized logging
- ✅ **Default values** - Giá trị mặc định (non-sensitive)
- ❌ **KHÔNG có sensitive data** - API keys, passwords, connection strings

**Lý do**: Sensitive data sẽ được set qua Environment Variables trong Koyeb (secure hơn)

## 🔐 Environment Variables Format

ASP.NET Core sử dụng format `Section__SubSection__Key` (2 dấu `__`):

### Ví dụ:

```json
{
  "Sepay": {
    "ApiKey": "value"
  }
}
```

Environment variable:
```env
Sepay__ApiKey=your-actual-key
```

### Nested example:

```json
{
  "ConnectionStrings": {
    "PostgresConnection": "value"
  }
}
```

Environment variable:
```env
ConnectionStrings__PostgresConnection=Host=...;Port=5432;...
```

## ✅ Checklist khi Deploy lên Koyeb

### Bước 1: Set Environment Variable
```env
ASPNETCORE_ENVIRONMENT=Production
```
⚠️ **KHÔNG có khoảng trắng** sau dấu `=`

### Bước 2: Verify trong Logs

Khi app start, bạn sẽ thấy trong logs:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://[::]:8080
info: Microsoft.Hosting.Lifetime[0]
      Application started. Environment: Production  👈 Check dòng này
```

Nếu thấy `Environment: Development`, nghĩa là bạn chưa set đúng.

### Bước 3: Set all Required Secrets

Tất cả sensitive data phải được set qua Koyeb Secrets:

```env
# Connection String
ConnectionStrings__PostgresConnection=Host=...;...

# SePay Credentials
Sepay__ApiKey=your-key
Sepay__SecretKey=your-secret

# Other Services
APIService__BaseUrlOrder=https://...
```

## 🧪 Test Configuration

### Cách 1: Kiểm tra trong Code (tạm thời để debug)

Thêm endpoint tạm trong controller:

```csharp
[HttpGet("config/test")]
public IActionResult TestConfig()
{
    return Ok(new {
        Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
        SepayBaseUrl = _configuration["Sepay:BaseUrl"],
        HasApiKey = !string.IsNullOrEmpty(_configuration["Sepay:ApiKey"])
    });
}
```

### Cách 2: Xem trong Koyeb Logs

1. Vào Koyeb Dashboard → Your App → Logs
2. Tìm dòng "Application started. Environment: ..."
3. Check tất cả config đã load đúng chưa

## ⚠️ Common Mistakes

### ❌ Sai: Không set ASPNETCORE_ENVIRONMENT
- App sẽ dùng `Development` (default)
- `appsettings.Production.json` sẽ KHÔNG được load

### ❌ Sai: Set sai format environment variable
```env
# SAI
Sepay:ApiKey=value
Sepay_ApiKey=value

# ĐÚNG
Sepay__ApiKey=value  (2 dấu __)
```

### ❌ Sai: Commit sensitive data vào appsettings.Production.json
- Nên để trống (`""`) hoặc không có key đó
- Set qua Environment Variables trong Koyeb

## 📝 Best Practices

1. ✅ **Luôn set `ASPNETCORE_ENVIRONMENT`** trong Koyeb
2. ✅ **Never commit secrets** vào file config
3. ✅ **Use Koyeb Secrets** cho sensitive data
4. ✅ **Verify environment** trong logs sau khi deploy
5. ✅ **Test endpoints** để đảm bảo config đúng

## 🔄 Config Priority Summary

```
Priority 1 (Highest): Environment Variables (Koyeb)
         ↓
Priority 2: appsettings.Production.json
         ↓
Priority 3 (Lowest): appsettings.json
```

**Ví dụ:**
- `appsettings.json` có `Sepay__BaseUrl=https://sandbox.sepay.vn`
- `appsettings.Production.json` có `Sepay__BaseUrl=https://api.sepay.vn`
- Environment Variable: `Sepay__BaseUrl=https://custom.sepay.vn`

**Kết quả cuối cùng:** `https://custom.sepay.vn` (từ Environment Variable)

---

**Last Updated**: 2025-01-17

