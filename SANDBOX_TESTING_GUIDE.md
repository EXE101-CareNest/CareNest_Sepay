# 🧪 Hướng dẫn Test Sandbox SePay

## 📋 Tổng quan

Hướng dẫn này sẽ giúp bạn test hệ thống SePay trên môi trường sandbox một cách hiệu quả và an toàn.

## 🚀 Bước 1: Cấu hình Sandbox

### 1.1 Cập nhật cấu hình

Đảm bảo file `appsettings.Development.json` có cấu hình sandbox:

```json
{
  "Sepay": {
    "ApiKey": "sandbox-api-key-here",
    "SecretKey": "sandbox-secret-key-here",
    "WebhookUrl": "/api/sepay/webhook",
    "AccountNumber": "sandbox-account-number",
    "BaseUrl": "https://sandbox.sepay.vn/api",
    "Environment": "Sandbox"
  }
}
```

### 1.2 Khởi động ứng dụng

```bash
cd CareNest_SePay
dotnet run --environment Development
```

Ứng dụng sẽ chạy tại: `https://localhost:7000`

## 🧪 Bước 2: Test Cases

### 2.1 Test Sandbox Transaction Creation

**Endpoint**: `POST /api/sepay/test/sandbox`

**Mục đích**: Tạo transaction test và gửi webhook

**Request Body**:

```json
{
  "amount": 100000,
  "description": "Test transaction for sandbox environment"
}
```

**Response**:

```json
{
  "success": true,
  "message": "Sandbox test completed",
  "data": {
    "transaction": {
      "transactionId": 123456,
      "gateway": "Sepay",
      "transactionDate": 1704067200,
      "accountNumber": "sandbox-account-number",
      "subAccount": "TEST",
      "amountIn": 100000,
      "amountOut": 0,
      "accumulated": 100000,
      "code": "TEST_CODE",
      "transactionContent": "Test transaction for sandbox environment",
      "referenceNumber": "TEST_20240101120000",
      "status": "Pending"
    },
    "webhookSent": true,
    "message": "Sandbox test completed successfully"
  }
}
```

### 2.2 Test Webhook Processing

**Endpoint**: `POST /api/sepay/test/webhook`

**Mục đích**: Test xử lý webhook data

**Request Body**:

```json
{
  "transactionId": 123456,
  "amount": 50000,
  "accountNumber": "TEST_ACCOUNT_001",
  "description": "Test webhook processing"
}
```

### 2.3 Test Real Webhook Simulation

**Endpoint**: `POST /api/sepay/webhook`

**Mục đích**: Simulate webhook từ SePay

**Headers**:

```
Content-Type: application/json
Authorization: Bearer sandbox-api-key-here
X-Sepay-Signature: test-signature-here
```

**Request Body**:

```json
{
  "transactionId": 789012,
  "transactionDate": 1704067200,
  "accountNumber": "1234567890",
  "subAccount": "SUB001",
  "amountIn": 75000,
  "amountOut": 0,
  "accumulated": 75000,
  "code": "PAYMENT_CODE",
  "transactionContent": "Payment for order #12345",
  "referenceNumber": "REF_20240101_001",
  "status": "Completed"
}
```

## 🔍 Bước 3: Test Queries

### 3.1 Lấy Transaction theo ID

**Endpoint**: `GET /api/sepay/transactions/{id}`

**Ví dụ**: `GET /api/sepay/transactions/123456`

### 3.2 Lấy Transactions theo Date Range

**Endpoint**: `GET /api/sepay/transactions`

**Query Parameters**:

- `startDate`: 2024-01-01
- `endDate`: 2024-01-31
- `pageIndex`: 1
- `pageSize`: 10

**Ví dụ**: `GET /api/sepay/transactions?startDate=2024-01-01&endDate=2024-01-31&pageIndex=1&pageSize=10`

### 3.3 Cập nhật Transaction Status

**Endpoint**: `PUT /api/sepay/transactions/{id}/status`

**Request Body**:

```json
{
  "status": "Completed",
  "errorMessage": null
}
```

## 🛠️ Bước 4: Sử dụng HTTP Test File

### 4.1 Mở file test

Sử dụng file `test-sandbox.http` đã được tạo sẵn với các test cases:

1. **Test Sandbox Transaction Creation**
2. **Test Webhook Processing**
3. **Test Real Webhook Simulation**
4. **Test Queries**
5. **Test Error Scenarios**

### 4.2 Chạy tests

1. Mở file `test-sandbox.http` trong VS Code
2. Click "Send Request" cho từng test case
3. Kiểm tra response và logs

## 📊 Bước 5: Kiểm tra Logs

### 5.1 Application Logs

Logs được lưu trong thư mục `logs/`:

- File: `logs/sepay-YYYYMMDD.txt`
- Format: Structured logging với Serilog

### 5.2 Console Logs

Khi chạy `dotnet run`, logs sẽ hiển thị trên console:

```
info: CareNest_SePay.Controllers.SepayWebhookController[0]
      Starting sandbox test
info: CareNest_SePay.Infrastructure.Services.PaymentService[0]
      Creating test transaction in Sandbox environment
info: CareNest_SePay.Infrastructure.Services.PaymentService[0]
      Test transaction created: {...}
```

## 🚨 Bước 6: Test Error Scenarios

### 6.1 Invalid Transaction ID

```http
GET /api/sepay/transactions/invalid-id
```

### 6.2 Invalid Date Range

```http
GET /api/sepay/transactions?startDate=invalid-date&endDate=2024-01-31
```

### 6.3 Negative Amount

```json
{
  "amount": -1000,
  "description": "Negative amount test"
}
```

### 6.4 Webhook without Signature

```http
POST /api/sepay/webhook
Content-Type: application/json

{
  "transactionId": 111111,
  "amountIn": 50000,
  "status": "Pending"
}
```

## 🔧 Bước 7: Cấu hình Database

### 7.1 Kiểm tra Connection String

Đảm bảo PostgreSQL đang chạy và connection string đúng:

```json
{
  "ConnectionStrings": {
    "PostgresConnection": "Host=localhost;Port=5432;Username=postgres;Password=123456;Database=carenest-sepay-dev;"
  }
}
```

### 7.2 Chạy Migrations

```bash
dotnet ef database update --project CareNest_SePay.Infrastructure --startup-project CareNest_SePay
```

## 📈 Bước 8: Performance Testing

### 8.1 Test với nhiều transactions

```bash
# Sử dụng curl để test multiple requests
for i in {1..10}; do
  curl -X POST https://localhost:7000/api/sepay/test/sandbox \
    -H "Content-Type: application/json" \
    -d "{\"amount\": $((RANDOM % 100000 + 10000)), \"description\": \"Test $i\"}"
done
```

### 8.2 Test concurrent webhooks

```bash
# Test multiple webhooks simultaneously
curl -X POST https://localhost:7000/api/sepay/webhook \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer sandbox-api-key-here" \
  -H "X-Sepay-Signature: test-signature-here" \
  -d '{"transactionId": 111111, "amountIn": 10000, "status": "Pending"}' &

curl -X POST https://localhost:7000/api/sepay/webhook \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer sandbox-api-key-here" \
  -H "X-Sepay-Signature: test-signature-here" \
  -d '{"transactionId": 222222, "amountIn": 20000, "status": "Pending"}' &
```

## 🔐 Bước 9: Security Testing

### 9.1 Test Authentication

```http
# Test without API key
POST /api/sepay/webhook
Content-Type: application/json

{
  "transactionId": 999999,
  "amountIn": 50000
}
```

### 9.2 Test Signature Validation

```http
# Test with invalid signature
POST /api/sepay/webhook
Content-Type: application/json
Authorization: Bearer sandbox-api-key-here
X-Sepay-Signature: invalid-signature

{
  "transactionId": 888888,
  "amountIn": 30000
}
```

## 📝 Bước 10: Monitoring và Debugging

### 10.1 Kiểm tra Database

```sql
-- Xem tất cả transactions
SELECT * FROM "SepayTransactions" ORDER BY "CreatedAt" DESC;

-- Xem transactions theo status
SELECT "Status", COUNT(*) FROM "SepayTransactions" GROUP BY "Status";

-- Xem transactions theo ngày
SELECT DATE("CreatedAt"), COUNT(*) FROM "SepayTransactions"
WHERE "CreatedAt" >= CURRENT_DATE - INTERVAL '7 days'
GROUP BY DATE("CreatedAt");
```

### 10.2 Health Check

```http
GET /health
```

## 🎯 Best Practices

### 1. **Test Data Management**

- Sử dụng test data có thể dự đoán được
- Clean up test data sau khi test
- Sử dụng unique identifiers cho mỗi test run

### 2. **Error Handling**

- Test tất cả error scenarios
- Verify error messages và status codes
- Check logs cho error details

### 3. **Performance**

- Test với realistic data volumes
- Monitor response times
- Test concurrent requests

### 4. **Security**

- Test authentication và authorization
- Verify signature validation
- Test với malicious input

## 🚀 Production Readiness Checklist

- [ ] Tất cả test cases pass
- [ ] Error handling hoạt động đúng
- [ ] Logs được ghi đầy đủ
- [ ] Database migrations chạy thành công
- [ ] Performance đạt yêu cầu
- [ ] Security tests pass
- [ ] Documentation đầy đủ

## 📞 Support

Nếu gặp vấn đề trong quá trình test:

1. Kiểm tra logs trong `logs/` folder
2. Verify database connection
3. Check configuration files
4. Review error messages trong response

---

**Lưu ý**: Sandbox environment chỉ dành cho testing. Không sử dụng sandbox data cho production.
