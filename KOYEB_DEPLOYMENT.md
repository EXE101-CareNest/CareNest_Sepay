# Hướng dẫn Deploy CareNest_SePay lên Koyeb

Hướng dẫn chi tiết để deploy ứng dụng CareNest_SePay lên Koyeb platform.

## 📋 Yêu cầu tiên quyết

- Tài khoản Koyeb (đăng ký tại https://www.koyeb.com)
- Repository GitHub/GitLab chứa source code
- Thông tin database PostgreSQL (đã có sẵn hoặc tạo mới)
- Thông tin SePay API credentials

## 🚀 Các phương pháp Deploy

Koyeb hỗ trợ 2 cách deploy chính:

### Phương pháp 1: Deploy từ Git Repository (Khuyến nghị)

Đây là phương pháp đơn giản và tự động nhất, phù hợp cho CI/CD.

#### Bước 1: Chuẩn bị Repository

1. Đảm bảo code đã được push lên GitHub/GitLab
2. File `Dockerfile` đã có trong root của repository
3. File `koyeb.yaml` (optional) để cấu hình tự động

#### Bước 2: Tạo Koyeb App từ Git

1. Đăng nhập vào Koyeb Dashboard: https://app.koyeb.com
2. Click **"Create App"**
3. Chọn tab **"GitHub"** hoặc **"GitLab"**
4. Chọn repository chứa code
5. Chọn branch (thường là `main` hoặc `dev`)
6. Build settings:
   - **Type**: Dockerfile
   - **Dockerfile path**: `Dockerfile`
   - **Dockerfile context**: `.` (root)
7. Click **"Deploy"**

#### Bước 3: Cấu hình Environment Variables

**⚠️ QUAN TRỌNG: Cấu hình môi trường Production**

Khi bạn set `ASPNETCORE_ENVIRONMENT=Production`, ASP.NET Core sẽ tự động:
1. Load `appsettings.json` (base configuration)
2. Load `appsettings.Production.json` (override cho production)
3. Override bằng Environment Variables (priority cao nhất)

File `appsettings.Production.json` đã được setup sẵn trong code với:
- Cấu hình logging phù hợp cho production
- Serilog chỉ output ra Console (Koyeb sẽ capture logs)
- Loại bỏ file logging (vì container không persist files)

**Tất cả sensitive data (API keys, connection strings) sẽ được set qua Environment Variables trong Koyeb.**

---

Trong Koyeb App settings, thêm các environment variables sau:

**Required (Set làm Environment Variable thường):**

```env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
```

**⚠️ QUAN TRỌNG: `ASPNETCORE_ENVIRONMENT=Production` là BẮT BUỘC để app load `appsettings.Production.json`**

**Secrets (click vào "Secret" để encrypt - BẮT BUỘC phải set):**

**Option 1: Dùng DatabaseSettings (Khuyến nghị)**
```env
# Database Settings
DatabaseSettings__Ip=c18qegamsgjut6.cluster-czrs8kj4isg7.us-east-1.rds.amazonaws.com
DatabaseSettings__Port=5432
DatabaseSettings__User=ud1hq49h2ce600
DatabaseSettings__Password=pe1d22abda9cee1c8923edca8da5ae0c09c1e34d739e3f018a6546c76ab28a188
DatabaseSettings__Database=ddqb3tis2mt3kg

# SePay
Sepay__ApiKey=your-sepay-api-key
Sepay__SecretKey=your-sepay-secret-key
Sepay__BaseUrl=https://api.sepay.vn
APIService__BaseUrlOrder=your-order-service-url
```

**Option 2: Dùng ConnectionStrings (Backward compatibility)**
```env
ConnectionStrings__PostgresConnection=Host=your-db-host;Port=5432;Username=your-username;Password=your-password;Database=your-database;SSL Mode=Require;
Sepay__ApiKey=your-sepay-api-key
Sepay__SecretKey=your-sepay-secret-key
Sepay__BaseUrl=https://api.sepay.vn
APIService__BaseUrlOrder=your-order-service-url
```

**Optional (có thể set nếu cần override):**

```env
Sepay__WebhookUrl=/api/sepay/webhook
Sepay__MerchantGUID=your-merchant-guid
Sepay__MerchantName=CareNest
Sepay__AccountNumber=your-account-number
Sepay__BankCode=VPBANK
```

**📝 Lưu ý về cấu hình:**
- Trong `appsettings.Production.json`, các giá trị sensitive đã được để trống (`""`)
- Koyeb Environment Variables sẽ **OVERRIDE** tất cả giá trị trong file config
- Format environment variable: `Section__SubSection__Key=Value` (dùng 2 dấu `__`)

#### Bước 4: Cấu hình Port và Health Check

1. Trong App settings, tìm phần **"HTTP"**
2. Set **Port**: `8080`
3. Trong **"Health Check"**:
   - **Path**: `/health`
   - **Interval**: `30` seconds
   - **Timeout**: `10` seconds
   - **Initial Timeout**: `40` seconds

### Phương pháp 2: Deploy từ Docker Image

Nếu bạn muốn build Docker image riêng và push lên Docker Hub.

#### Bước 1: Build và Push Docker Image

```powershell
# Login to Docker Hub
docker login

# Build the image
docker build -t yourusername/carenest-sepay:latest .

# Push to Docker Hub
docker push yourusername/carenest-sepay:latest
```

#### Bước 2: Tạo Koyeb App từ Docker Image

1. Trong Koyeb Dashboard, click **"Create App"**
2. Chọn **"Docker Image"**
3. Nhập image name: `yourusername/carenest-sepay:latest`
4. Set port: `8080`
5. Cấu hình Environment Variables như Phương pháp 1
6. Click **"Deploy"**

## ⚙️ Cấu hình SePay Webhook

Sau khi deploy thành công:

1. Lấy URL của ứng dụng từ Koyeb (ví dụ: `https://carenest-sepay-xxxxx.koyeb.app`)
2. Đăng nhập vào SePay Dashboard
3. Vào phần **Webhook Settings**
4. Set Webhook URL: `https://your-app-name.koyeb.app/api/sepay/webhook`
5. Đảm bảo webhook sử dụng đúng API Key đã config trong Koyeb

## ✅ Kiểm tra Deployment

### 1. Kiểm tra Health Endpoint

```bash
curl https://your-app-name.koyeb.app/health
```

Kết quả mong đợi: `Healthy`

### 2. Kiểm tra API Endpoints

```bash
# Test Swagger (chỉ trong Development)
curl https://your-app-name.koyeb.app/swagger

# Test API endpoint
curl https://your-app-name.koyeb.app/api/payment/test
```

### 3. Kiểm tra Logs

Trong Koyeb Dashboard:
1. Vào **"Apps"** → Chọn app của bạn
2. Click tab **"Logs"**
3. Xem real-time logs để kiểm tra errors

## 🔧 Cấu hình Database

### Option 1: Sử dụng Database hiện có

Nếu bạn đã có PostgreSQL database, chỉ cần thêm connection string vào Koyeb Secrets:

```env
ConnectionStrings__PostgresConnection=Host=your-db-host;Port=5432;Username=postgres;Password=your-password;Database=carenest-sepay;SSL Mode=Require;
```

### Option 2: Tạo Koyeb Database (Managed PostgreSQL)

1. Trong Koyeb Dashboard, click **"Databases"**
2. Click **"Create Database"**
3. Chọn **"PostgreSQL"**
4. Chọn region
5. Sau khi tạo, Koyeb sẽ tự động tạo connection string
6. Copy connection string và thêm vào App Secrets

**Lưu ý**: Bạn vẫn cần chạy migrations để tạo tables:

```bash
# Chạy migrations (có thể làm qua Koyeb Console hoặc local)
dotnet ef database update --project CareNest_SePay.Infrastructure --startup-project CareNest_SePay
```

## 📊 Monitoring và Troubleshooting

### Monitoring trong Koyeb

Koyeb Dashboard cung cấp:
- **Metrics**: CPU, Memory, Network usage
- **Logs**: Real-time application logs
- **Health Status**: Container health check status
- **Deployments**: Lịch sử các lần deploy

### Common Issues và Solutions

#### 1. Database Connection Failed

**Triệu chứng**: 
- App không start
- Logs hiển thị: "Failed to connect to database"

**Giải pháp**:
- Kiểm tra connection string format
- Đảm bảo database cho phép connections từ Koyeb IPs
- Kiểm tra username/password
- Thử thêm `SSL Mode=Require;` vào connection string

#### 2. Webhook không nhận được requests

**Triệu chứng**:
- SePay gửi webhook nhưng không thấy trong logs

**Giải pháp**:
- Verify webhook URL trong SePay dashboard
- Kiểm tra Authorization header matches với `Sepay__ApiKey`
- Xem logs trong Koyeb để kiểm tra incoming requests
- Test webhook endpoint bằng Postman/curl

#### 3. Container không start

**Triệu chứng**:
- App status: "Failed" hoặc "Crashing"

**Giải pháp**:
- Xem logs trong Koyeb để tìm lỗi
- Kiểm tra tất cả required environment variables đã được set
- Verify Dockerfile build thành công
- Kiểm tra health check endpoint có hoạt động không

#### 4. Health Check Failed

**Triệu chứng**:
- Health check status: "Unhealthy"

**Giải pháp**:
- Test `/health` endpoint manually: `curl https://your-app.koyeb.app/health`
- Kiểm tra port configuration (phải là 8080)
- Xem application logs để tìm errors
- Tăng `initial_timeout` nếu app cần thời gian khởi động lâu

#### 5. App không load Production settings

**Triệu chứng**:
- App chạy nhưng config không đúng (ví dụ: vẫn dùng Development settings)
- Logs không match với production config

**Giải pháp**:
- ✅ **QUAN TRỌNG**: Phải set `ASPNETCORE_ENVIRONMENT=Production` trong Koyeb Environment Variables
- Kiểm tra trong Koyeb App Settings → Environment Variables
- Xem logs khi app start, nó sẽ log environment name: `"Application started. Environment: Production"`
- Nếu thấy `Environment: Development`, nghĩa là bạn chưa set environment variable đúng
- Verify: Trong Koyeb logs, tìm dòng `"Now listening on: http://[::]:8080"` và check environment

**Cách kiểm tra environment trong runtime:**
```bash
# Test endpoint để xem environment
curl https://your-app.koyeb.app/api/test/env
# Hoặc xem trong Koyeb logs khi app khởi động
```

## 🔐 Security Best Practices

1. **Never commit secrets**: Tất cả sensitive data phải được lưu trong Koyeb Secrets
2. **Use HTTPS**: Koyeb tự động cung cấp HTTPS cho domain của bạn
3. **Rotate credentials**: Thường xuyên đổi API keys và passwords
4. **Monitor logs**: Kiểm tra logs thường xuyên để phát hiện suspicious activities
5. **Environment separation**: Sử dụng separate apps cho dev/staging/production

## 🔄 Auto-deploy từ Git

Koyeb tự động deploy khi có commit mới:

1. Push code lên branch được connect (thường là `main`)
2. Koyeb tự động:
   - Build Docker image từ Dockerfile
   - Run health checks
   - Deploy new version
3. Nếu deploy thành công, traffic được route đến version mới
4. Nếu deploy fail, giữ nguyên version cũ

## 📝 Environment Variables Reference

Xem file **`ENVIRONMENT_VARIABLES.md`** để có danh sách đầy đủ và chi tiết tất cả các biến môi trường.

### Quick Reference:

| Variable | Required | Description | Example |
|----------|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | ✅ | Environment name | `Production` |
| `ASPNETCORE_URLS` | ✅ | URLs để app listen | `http://+:8080` |
| `ConnectionStrings__PostgresConnection` | ✅ | PostgreSQL connection string | `Host=...;Port=5432;...` |
| `Sepay__ApiKey` | ✅ | SePay API Key | `your-api-key` |
| `Sepay__SecretKey` | ✅ | SePay Secret Key | `your-secret-key` |
| `Sepay__BaseUrl` | ✅ | SePay API base URL | `https://api.sepay.vn` |
| `APIService__BaseUrlOrder` | ⚠️ | Order service URL | `https://api.example.com` |
| `Sepay__WebhookUrl` | ❌ | Webhook path | `/api/sepay/webhook` |
| `Sepay__MerchantGUID` | ❌ | Merchant GUID | `A000000775` |

**📖 Xem chi tiết đầy đủ trong `ENVIRONMENT_VARIABLES.md`**

## 🎯 Next Steps

Sau khi deploy thành công:

1. ✅ Test tất cả API endpoints
2. ✅ Configure SePay webhook URL
3. ✅ Monitor application logs
4. ✅ Setup alerts trong Koyeb (nếu cần)
5. ✅ Document deployment process cho team

## 📞 Support

Nếu gặp vấn đề:
1. Xem Koyeb Documentation: https://www.koyeb.com/docs
2. Check application logs trong Koyeb Dashboard
3. Verify environment variables và secrets
4. Test endpoints manually bằng curl/Postman

---

**Last Updated**: 2025-01-17
