# Danh sách Biến Môi trường (Environment Variables)

Danh sách đầy đủ tất cả các biến môi trường cần thiết khi deploy lên Koyeb.

## 📋 Phân loại

### ✅ Bắt buộc (Required)

Những biến này **PHẢI** được set, nếu không app sẽ không hoạt động.

### ⚠️ Quan trọng (Important)

Những biến này nên được set để đảm bảo app hoạt động đúng.

### ❌ Tùy chọn (Optional)

Những biến này có thể bỏ qua nếu không cần thiết.

---

## 🔧 ASP.NET Core Settings

### ✅ `ASPNETCORE_ENVIRONMENT`
- **Mô tả**: Môi trường chạy ứng dụng
- **Giá trị**: `Production`
- **Bắt buộc**: ✅ **YES**
- **Ghi chú**: **QUAN TRỌNG** - Phải set để app load `appsettings.Production.json`

```env
ASPNETCORE_ENVIRONMENT=Production
```

### ✅ `ASPNETCORE_URLS`
- **Mô tả**: URLs để ứng dụng listen
- **Giá trị**: `http://+:8080`
- **Bắt buộc**: ✅ **YES**
- **Ghi chú**: Port phải match với port trong Koyeb (8080)

```env
ASPNETCORE_URLS=http://+:8080
```

---

## 💾 Database Connection

Có 2 cách để cấu hình database:

### Cách 1: DatabaseSettings (Khuyến nghị)

Tách riêng từng thông tin database, dễ quản lý hơn:

#### ✅ `DatabaseSettings__Ip`
- **Mô tả**: IP address hoặc hostname của database server
- **Bắt buộc**: ✅ **YES** (nếu dùng DatabaseSettings)
- **Ghi chú**: **PHẢI SET LÀM SECRET** nếu chứa thông tin nhạy cảm

```env
DatabaseSettings__Ip=c18qegamsgjut6.cluster-czrs8kj4isg7.us-east-1.rds.amazonaws.com
```

#### ✅ `DatabaseSettings__Port`
- **Mô tả**: Port của database (thường là 5432 cho PostgreSQL)
- **Giá trị mặc định**: `5432`
- **Bắt buộc**: ❌ Optional

```env
DatabaseSettings__Port=5432
```

#### ✅ `DatabaseSettings__User`
- **Mô tả**: Username để kết nối database
- **Bắt buộc**: ✅ **YES** (nếu dùng DatabaseSettings)
- **Ghi chú**: **PHẢI SET LÀM SECRET** trong Koyeb

```env
DatabaseSettings__User=ud1hq49h2ce600
```

#### ✅ `DatabaseSettings__Password`
- **Mô tả**: Password để kết nối database
- **Bắt buộc**: ✅ **YES** (nếu dùng DatabaseSettings)
- **Ghi chú**: **PHẢI SET LÀM SECRET** trong Koyeb

```env
DatabaseSettings__Password=pe1d22abda9cee1c8923edca8da5ae0c09c1e34d739e3f018a6546c76ab28a188
```

#### ✅ `DatabaseSettings__Database`
- **Mô tả**: Tên database
- **Bắt buộc**: ✅ **YES** (nếu dùng DatabaseSettings)

```env
DatabaseSettings__Database=ddqb3tis2mt3kg
```

**Ví dụ đầy đủ với AWS RDS:**
```env
DatabaseSettings__Ip=c18qegamsgjut6.cluster-czrs8kj4isg7.us-east-1.rds.amazonaws.com
DatabaseSettings__Port=5432
DatabaseSettings__User=ud1hq49h2ce600
DatabaseSettings__Password=pe1d22abda9cee1c8923edca8da5ae0c09c1e34d739e3f018a6546c76ab28a188
DatabaseSettings__Database=ddqb3tis2mt3kg
```

### Cách 2: ConnectionStrings (Backward Compatibility)

Vẫn hỗ trợ cách cũ để tương thích ngược:

#### ✅ `ConnectionStrings__PostgresConnection`
- **Mô tả**: Connection string đầy đủ cho PostgreSQL database
- **Bắt buộc**: ✅ **YES** (nếu KHÔNG dùng DatabaseSettings)
- **Format**: 
  ```
  Host=your-db-host;Port=5432;Username=your-username;Password=your-password;Database=your-database;SSL Mode=Require;
  ```
- **Ghi chú**: 
  - Dùng `__` (2 dấu gạch dưới) để phân cách nested sections
  - Nên thêm `SSL Mode=Require;` khi connect database cloud
  - **PHẢI SET LÀM SECRET** trong Koyeb
  - App sẽ tự động build connection string từ DatabaseSettings nếu có, nếu không mới dùng ConnectionStrings

```env
ConnectionStrings__PostgresConnection=Host=db.example.com;Port=5432;Username=postgres;Password=your-password;Database=carenest-sepay;SSL Mode=Require;
```

**Ví dụ với Koyeb Managed Database:**
```env
ConnectionStrings__PostgresConnection=Host=xxxxx.koyeb.app;Port=5432;Username=koyebuser;Password=xxx;Database=carenest_sepay;SSL Mode=Require;
```

### 📝 Lưu ý về Priority

App sẽ ưu tiên sử dụng theo thứ tự:
1. **DatabaseSettings** (nếu có đầy đủ Ip, User, Password, Database)
2. **ConnectionStrings__PostgresConnection** (fallback nếu DatabaseSettings không có)

**Khuyến nghị**: Dùng **DatabaseSettings** vì dễ quản lý và bảo mật hơn (có thể set từng field riêng biệt).

---

## 🔐 SePay Configuration

### ✅ `Sepay__ApiKey`
- **Mô tả**: API Key từ SePay để authenticate requests
- **Bắt buộc**: ✅ **YES**
- **Ghi chú**: **PHẢI SET LÀM SECRET** trong Koyeb
- **Sử dụng trong code**: 
  - `SepayAPIService.cs`: Line 27, 34, 72
  - `SepayWebhookController.cs`: Line 65
  - `PaymentService.cs`: Line 129

```env
Sepay__ApiKey=B5BUXOXHITVUY51ISJWMKAB1EOFNWVT7VMQGKG34L4NLO8VSMRGA3ZC9QEKDBFNR
```

### ✅ `Sepay__SecretKey`
- **Mô tả**: Secret Key từ SePay để validate webhook signatures
- **Bắt buộc**: ✅ **YES**
- **Ghi chú**: **PHẢI SET LÀM SECRET** trong Koyeb
- **Sử dụng trong code**: 
  - `PaymentService.cs`: Line 105, 225

```env
Sepay__SecretKey=HUYNHGIABAO
```

### ✅ `Sepay__BaseUrl`
- **Mô tả**: Base URL của SePay API
- **Giá trị mặc định**: `https://api.sepay.vn`
- **Bắt buộc**: ✅ **YES**
- **Ghi chú**: 
  - Production: `https://api.sepay.vn`
  - Sandbox: `https://sandbox.sepay.vn`
- **Sử dụng trong code**: 
  - `SepayAPIService.cs`: Line 26, 38
  - `PaymentService.cs`: Line 128, 168

```env
Sepay__BaseUrl=https://api.sepay.vn
```

### ⚠️ `Sepay__WebhookUrl`
- **Mô tả**: Path cho webhook endpoint (relative URL)
- **Giá trị mặc định**: `/api/sepay/webhook`
- **Bắt buộc**: ❌ Optional (có default)
- **Sử dụng trong code**: 
  - `PaymentService.cs`: Line 167

```env
Sepay__WebhookUrl=/api/sepay/webhook
```

### ⚠️ `Sepay__Environment`
- **Mô tả**: Môi trường SePay (Sandbox hoặc Production)
- **Giá trị**: `Production` hoặc `Sandbox`
- **Bắt buộc**: ❌ Optional (nhưng nên set)
- **Sử dụng trong code**: 
  - `PaymentService.cs`: Line 130, 169, 174

```env
Sepay__Environment=Production
```

### ❌ `Sepay__AccountNumber`
- **Mô tả**: Số tài khoản SePay
- **Bắt buộc**: ❌ Optional
- **Sử dụng trong code**: 
  - `PaymentService.cs`: Line 140 (cho test transactions)

```env
Sepay__AccountNumber=1234567890
```

### ❌ `Sepay__AccountName`
- **Mô tả**: Tên tài khoản
- **Giá trị mặc định**: `CareNest`
- **Bắt buộc**: ❌ Optional

```env
Sepay__AccountName=CareNest
```

### ❌ `Sepay__BankCode`
- **Mô tả**: Mã ngân hàng
- **Giá trị mặc định**: `VPBANK`
- **Bắt buộc**: ❌ Optional

```env
Sepay__BankCode=VPBANK
```

### ❌ `Sepay__MerchantGUID`
- **Mô tả**: Merchant GUID từ SePay
- **Bắt buộc**: ❌ Optional

```env
Sepay__MerchantGUID=A000000775
```

### ❌ `Sepay__MerchantName`
- **Mô tả**: Tên merchant
- **Giá trị mặc định**: `CareNest`
- **Bắt buộc**: ❌ Optional

```env
Sepay__MerchantName=CareNest
```

---

## 🌐 External API Services

### ⚠️ `APIService__BaseUrlOrder`
- **Mô tả**: Base URL của Order Service API
- **Bắt buộc**: ⚠️ **IMPORTANT** (nếu app cần gọi Order Service)
- **Sử dụng trong code**: 
  - `APIService.cs`: Qua `APIServiceOption.BaseUrlOrder`
  - Config trong `Program.cs`: Line 58-59

```env
APIService__BaseUrlOrder=https://api.carenest.com/orders
```

**Lưu ý**: 
- Nếu không set, app vẫn chạy nhưng các tính năng gọi Order Service sẽ không hoạt động
- Format: Full URL không có trailing slash (`/`)

---

## 📊 Tổng kết

### Minimum Required (Tối thiểu)

Những biến này **PHẢI** có để app chạy được:

**Option 1: Dùng DatabaseSettings (Khuyến nghị)**
```env
# ASP.NET Core
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080

# Database (DatabaseSettings)
DatabaseSettings__Ip=your-db-host
DatabaseSettings__Port=5432
DatabaseSettings__User=your-username
DatabaseSettings__Password=your-password
DatabaseSettings__Database=your-database

# SePay
Sepay__ApiKey=your-api-key
Sepay__SecretKey=your-secret-key
Sepay__BaseUrl=https://api.sepay.vn
```

**Option 2: Dùng ConnectionStrings (Backward compatibility)**
```env
# ASP.NET Core
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080

# Database (ConnectionStrings)
ConnectionStrings__PostgresConnection=Host=...;Port=5432;Username=...;Password=...;Database=...;

# SePay
Sepay__ApiKey=your-api-key
Sepay__SecretKey=your-secret-key
Sepay__BaseUrl=https://api.sepay.vn
```

### Recommended (Khuyến nghị)

Thêm các biến này để đảm bảo đầy đủ chức năng:

```env
# SePay
Sepay__Environment=Production
Sepay__WebhookUrl=/api/sepay/webhook
Sepay__MerchantGUID=your-merchant-guid

# External Services
APIService__BaseUrlOrder=https://your-order-service.com
```

### Full Configuration (Đầy đủ)

Tất cả các biến có thể set:

```env
# ASP.NET Core
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080

# Database (Option 1: DatabaseSettings - Khuyến nghị)
DatabaseSettings__Ip=your-db-host
DatabaseSettings__Port=5432
DatabaseSettings__User=your-username
DatabaseSettings__Password=your-password
DatabaseSettings__Database=your-database

# Database (Option 2: ConnectionStrings - Backward compatibility)
# ConnectionStrings__PostgresConnection=Host=...;Port=5432;Username=...;Password=...;Database=...;SSL Mode=Require;

# SePay - Required
Sepay__ApiKey=your-api-key
Sepay__SecretKey=your-secret-key
Sepay__BaseUrl=https://api.sepay.vn

# SePay - Optional
Sepay__Environment=Production
Sepay__WebhookUrl=/api/sepay/webhook
Sepay__AccountNumber=1234567890
Sepay__AccountName=CareNest
Sepay__BankCode=VPBANK
Sepay__MerchantGUID=A000000775
Sepay__MerchantName=CareNest

# External Services
APIService__BaseUrlOrder=https://api.carenest.com/orders
```

---

## 🔒 Security Notes

### Biến phải set làm **SECRET** trong Koyeb:

1. ✅ `DatabaseSettings__Password` - Database password (nếu dùng DatabaseSettings)
2. ✅ `ConnectionStrings__PostgresConnection` - Chứa password database (nếu dùng ConnectionStrings)
3. ✅ `Sepay__ApiKey` - API credentials
4. ✅ `Sepay__SecretKey` - Secret key

**Lưu ý**: Có thể set thêm các field khác của DatabaseSettings làm SECRET nếu cần bảo mật hơn.

### Biến có thể set làm Environment Variable thường:

- `ASPNETCORE_ENVIRONMENT`
- `ASPNETCORE_URLS`
- `Sepay__BaseUrl`
- `Sepay__Environment`
- `Sepay__WebhookUrl`
- Các biến không nhạy cảm khác

---

## 📝 Format Environment Variables trong Koyeb

### Cách set trong Koyeb Dashboard:

1. **Environment Variables** (normal):
   - Key: `ASPNETCORE_ENVIRONMENT`
   - Value: `Production`

2. **Secrets** (encrypted):
   - Key: `ConnectionStrings__PostgresConnection`
   - Value: `Host=...;Port=5432;...` (click vào "Secret" checkbox)

### Format cho Nested Sections:

ASP.NET Core sử dụng `__` (2 dấu gạch dưới) để phân cách:

```json
{
  "Section": {
    "SubSection": {
      "Key": "Value"
    }
  }
}
```

Environment Variable:
```env
Section__SubSection__Key=Value
```

**Ví dụ:**
```env
Sepay__ApiKey=value        ✅ Đúng
Sepay:ApiKey=value         ❌ Sai
Sepay_ApiKey=value         ❌ Sai
```

---

## ✅ Checklist khi Deploy

### Database Configuration:
- [ ] Chọn một trong 2 cách:
  - **Option 1 (Khuyến nghị)**: Set `DatabaseSettings__*` (Ip, Port, User, Password, Database)
  - **Option 2**: Set `ConnectionStrings__PostgresConnection`
- [ ] Set database password làm SECRET trong Koyeb

### ASP.NET Core:
- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Set `ASPNETCORE_URLS=http://+:8080`

### SePay:
- [ ] Set `Sepay__ApiKey` (as SECRET)
- [ ] Set `Sepay__SecretKey` (as SECRET)
- [ ] Set `Sepay__BaseUrl`

### Optional:
- [ ] Set `Sepay__Environment=Production` (nếu cần)
- [ ] Set `APIService__BaseUrlOrder` (nếu cần)

### Verification:
- [ ] Verify trong logs: `Application started. Environment: Production`
- [ ] Test database connection: Check logs có lỗi kết nối database không

---

**Last Updated**: 2025-01-17

