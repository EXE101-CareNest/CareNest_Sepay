# 📋 Checklist Environment Variables cho Koyeb

Danh sách đầy đủ các biến môi trường cần set trong Koyeb để deploy ứng dụng.

---

## ✅ BẮT BUỘC (Required) - Phải có để app chạy được

### 1. ASP.NET Core Settings

```env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
```

**Ghi chú:**
- Set làm **Environment Variable** thường (không cần Secret)
- `ASPNETCORE_ENVIRONMENT=Production` là **QUAN TRỌNG NHẤT** - phải có để app load `appsettings.Production.json`

---

### 2. Database Configuration

**Chọn một trong 2 cách:**

#### Option 1: DatabaseSettings (Khuyến nghị) ⭐

```env
DatabaseSettings__Ip=c18qegamsgjut6.cluster-czrs8kj4isg7.us-east-1.rds.amazonaws.com
DatabaseSettings__Port=5432
DatabaseSettings__User=ud1hq49h2ce600
DatabaseSettings__Password=pe1d22abda9cee1c8923edca8da5ae0c09c1e34d739e3f018a6546c76ab28a188
DatabaseSettings__Database=ddqb3tis2mt3kg
```

**Trong Koyeb:**
- `DatabaseSettings__Ip` → Environment Variable thường
- `DatabaseSettings__Port` → Environment Variable thường (hoặc để default)
- `DatabaseSettings__User` → Environment Variable thường hoặc Secret (tùy)
- `DatabaseSettings__Password` → **SECRET** ⚠️
- `DatabaseSettings__Database` → Environment Variable thường

#### Option 2: ConnectionStrings (Backward Compatibility)

```env
ConnectionStrings__PostgresConnection=Host=c18qegamsgjut6.cluster-czrs8kj4isg7.us-east-1.rds.amazonaws.com;Port=5432;Username=ud1hq49h2ce600;Password=pe1d22abda9cee1c8923edca8da5ae0c09c1e34d739e3f018a6546c76ab28a188;Database=ddqb3tis2mt3kg;SSL Mode=Require;
```

**Trong Koyeb:**
- `ConnectionStrings__PostgresConnection` → **SECRET** ⚠️

---

### 3. SePay Configuration (Bắt buộc)

```env
Sepay__ApiKey=B5BUXOXHITVUY51ISJWMKAB1EOFNWVT7VMQGKG34L4NLO8VSMRGA3ZC9QEKDBFNR
Sepay__SecretKey=HUYNHGIABAO
Sepay__BaseUrl=https://api.sepay.vn
```

**Trong Koyeb:**
- `Sepay__ApiKey` → **SECRET** ⚠️
- `Sepay__SecretKey` → **SECRET** ⚠️
- `Sepay__BaseUrl` → Environment Variable thường

**Ghi chú:**
- Production: `https://api.sepay.vn`
- Sandbox: `https://sandbox.sepay.vn` (cho testing)

---

## ⚠️ QUAN TRỌNG (Important) - Nên set để đảm bảo đầy đủ chức năng

### 4. SePay Additional Settings

```env
Sepay__Environment=Production
Sepay__WebhookUrl=/api/sepay/webhook
```

**Trong Koyeb:**
- `Sepay__Environment` → Environment Variable thường
- `Sepay__WebhookUrl` → Environment Variable thường (có default)

---

### 5. External API Services

```env
APIService__BaseUrlOrder=https://your-order-service.com
```

**Trong Koyeb:**
- `APIService__BaseUrlOrder` → Environment Variable thường

**Ghi chú:**
- Chỉ cần set nếu app cần gọi Order Service API
- Nếu không có, một số tính năng có thể không hoạt động

---

## ❌ TÙY CHỌN (Optional) - Có thể bỏ qua

### 6. SePay Optional Settings

```env
Sepay__MerchantGUID=A000000775
Sepay__MerchantName=CareNest
Sepay__AccountNumber=1234567890
Sepay__AccountName=CareNest
Sepay__BankCode=VPBANK
```

**Trong Koyeb:**
- Tất cả → Environment Variable thường (không cần Secret)

---

## 📝 Tổng hợp - Copy & Paste vào Koyeb

### Minimum Setup (Tối thiểu để chạy):

**Environment Variables (Normal):**
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
DatabaseSettings__Ip=c18qegamsgjut6.cluster-czrs8kj4isg7.us-east-1.rds.amazonaws.com
DatabaseSettings__Port=5432
DatabaseSettings__User=ud1hq49h2ce600
DatabaseSettings__Database=ddqb3tis2mt3kg
Sepay__BaseUrl=https://api.sepay.vn
```

**Secrets (Click vào "Secret" checkbox):**
```
DatabaseSettings__Password=pe1d22abda9cee1c8923edca8da5ae0c09c1e34d739e3f018a6546c76ab28a188
Sepay__ApiKey=B5BUXOXHITVUY51ISJWMKAB1EOFNWVT7VMQGKG34L4NLO8VSMRGA3ZC9QEKDBFNR
Sepay__SecretKey=HUYNHGIABAO
```

---

### Recommended Setup (Khuyến nghị):

**Environment Variables (Normal):**
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
DatabaseSettings__Ip=c18qegamsgjut6.cluster-czrs8kj4isg7.us-east-1.rds.amazonaws.com
DatabaseSettings__Port=5432
DatabaseSettings__User=ud1hq49h2ce600
DatabaseSettings__Database=ddqb3tis2mt3kg
Sepay__BaseUrl=https://api.sepay.vn
Sepay__Environment=Production
Sepay__WebhookUrl=/api/sepay/webhook
APIService__BaseUrlOrder=https://your-order-service.com
```

**Secrets (Click vào "Secret" checkbox):**
```
DatabaseSettings__Password=pe1d22abda9cee1c8923edca8da5ae0c09c1e34d739e3f018a6546c76ab28a188
Sepay__ApiKey=B5BUXOXHITVUY51ISJWMKAB1EOFNWVT7VMQGKG34L4NLO8VSMRGA3ZC9QEKDBFNR
Sepay__SecretKey=HUYNHGIABAO
```

---

### Full Setup (Đầy đủ):

**Environment Variables (Normal):**
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
DatabaseSettings__Ip=c18qegamsgjut6.cluster-czrs8kj4isg7.us-east-1.rds.amazonaws.com
DatabaseSettings__Port=5432
DatabaseSettings__User=ud1hq49h2ce600
DatabaseSettings__Database=ddqb3tis2mt3kg
Sepay__BaseUrl=https://api.sepay.vn
Sepay__Environment=Production
Sepay__WebhookUrl=/api/sepay/webhook
Sepay__MerchantGUID=A000000775
Sepay__MerchantName=CareNest
Sepay__AccountNumber=1234567890
Sepay__AccountName=CareNest
Sepay__BankCode=VPBANK
APIService__BaseUrlOrder=https://your-order-service.com
```

**Secrets (Click vào "Secret" checkbox):**
```
DatabaseSettings__Password=pe1d22abda9cee1c8923edca8da5ae0c09c1e34d739e3f018a6546c76ab28a188
Sepay__ApiKey=B5BUXOXHITVUY51ISJWMKAB1EOFNWVT7VMQGKG34L4NLO8VSMRGA3ZC9QEKDBFNR
Sepay__SecretKey=HUYNHGIABAO
```

---

## ⚠️ Lưu ý quan trọng:

1. **Format Environment Variables:**
   - Dùng `__` (2 dấu gạch dưới) để phân cách nested sections
   - Ví dụ: `DatabaseSettings__Ip` ✅
   - KHÔNG dùng: `DatabaseSettings:Ip` ❌ hoặc `DatabaseSettings_Ip` ❌

2. **Secrets trong Koyeb:**
   - Click vào checkbox "Secret" khi tạo biến
   - Secrets sẽ được mã hóa và không hiển thị trong logs
   - Luôn set password và API keys làm Secret

3. **Values:**
   - **KHÔNG có khoảng trắng** sau dấu `=`
   - Ví dụ: `ASPNETCORE_ENVIRONMENT=Production` ✅
   - KHÔNG: `ASPNETCORE_ENVIRONMENT = Production` ❌

4. **Database Configuration:**
   - Chỉ chọn **MỘT** trong 2 cách: DatabaseSettings HOẶC ConnectionStrings
   - Khuyến nghị dùng DatabaseSettings (dễ quản lý hơn)

---

## ✅ Checklist khi Set:

### Trong Koyeb Dashboard:

1. [ ] Vào App Settings → Environment Variables
2. [ ] Add từng biến một theo danh sách trên
3. [ ] Check "Secret" cho các biến nhạy cảm:
   - [ ] `DatabaseSettings__Password`
   - [ ] `Sepay__ApiKey`
   - [ ] `Sepay__SecretKey`
4. [ ] Verify tất cả biến đã được add
5. [ ] Save changes
6. [ ] Restart app (nếu cần)
7. [ ] Check logs để verify:
   - [ ] `Application started. Environment: Production`
   - [ ] Không có lỗi database connection
   - [ ] Không có lỗi missing configuration

---

## 🔍 Cách Verify sau khi Set:

### 1. Check Logs trong Koyeb:

```bash
# Tìm trong logs:
"Application started. Environment: Production"  ✅
# Nếu thấy "Environment: Development" → chưa set đúng ASPNETCORE_ENVIRONMENT
```

### 2. Test Health Endpoint:

```bash
curl https://your-app-name.koyeb.app/health
# Kết quả mong đợi: "Healthy"
```

### 3. Test Database Connection:

- Xem logs có lỗi: "Failed to connect to database" không
- Nếu có lỗi → kiểm tra lại DatabaseSettings hoặc ConnectionStrings

---

## 📞 Troubleshooting:

**App không start:**
- Kiểm tra có set `ASPNETCORE_ENVIRONMENT=Production` chưa
- Kiểm tra có đủ các biến bắt buộc chưa
- Xem logs để tìm lỗi cụ thể

**Database connection failed:**
- Verify database credentials đúng chưa
- Check database có accessible từ Koyeb không
- Verify format connection string (nếu dùng ConnectionStrings)

**SePay API không hoạt động:**
- Check `Sepay__ApiKey` và `Sepay__SecretKey` đã set chưa
- Verify `Sepay__BaseUrl` đúng chưa (Production vs Sandbox)

---

**Last Updated**: 2025-01-17

