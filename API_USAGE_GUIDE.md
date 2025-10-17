# Hướng dẫn sử dụng API SePay CareNest

## Tổng quan

Hệ thống CareNest SePay cung cấp 2 API chính để tích hợp với SePay:

1. **API tạo mã QR** - Cho phép tạo mã QR VietQR để khách hàng quét thanh toán
2. **API Webhook** - Nhận thông báo thanh toán từ SePay và cập nhật trạng thái giao dịch

## 1. API Tạo Mã QR

### Endpoint: `POST /api/payment/create-qr`

Tạo mã QR VietQR cho khách hàng quét thanh toán.

#### Request Body:
```json
{
  "amount": 100000,
  "description": "Thanh toán dịch vụ CareNest",
  "orderId": "ORDER_123456",
  "bankCode": "VPBANK",
  "accountNumber": "1234567890",
  "accountName": "CareNest"
}
```

#### Response:
```json
{
  "success": true,
  "message": "Tạo mã QR thành công",
  "data": {
    "qrCode": "base64-encoded-qr-code",
    "vietQRCode": "VietQR_ORDER_123456",
    "bankName": "SePay",
    "accountNumber": "1234567890",
    "accountName": "CareNest",
    "amount": 100000,
    "description": "Thanh toán dịch vụ CareNest",
    "orderId": "ORDER_123456",
    "createdAt": "2025-01-13T10:30:00Z",
    "expiresAt": "2025-01-13T10:45:00Z"
  }
}
```

### Endpoint: `GET /api/payment/qr-info/{orderId}`

Lấy thông tin QR code theo mã đơn hàng.

#### Response:
```json
{
  "success": true,
  "message": "Lấy thông tin QR code thành công",
  "data": {
    "qrCode": "QR_ORDER_123456_20250113103000",
    "vietQRCode": "VietQR_ORDER_123456",
    "bankName": "SePay",
    "accountNumber": "1234567890",
    "accountName": "CareNest",
    "amount": 100000,
    "description": "Thanh toán đơn hàng ORDER_123456",
    "orderId": "ORDER_123456",
    "createdAt": "2025-01-13T10:30:00Z",
    "expiresAt": "2025-01-13T10:45:00Z"
  }
}
```

### Endpoint: `GET /api/payment/payment-status/{orderId}`

Kiểm tra trạng thái thanh toán của đơn hàng.

#### Response:
```json
{
  "success": true,
  "message": "Lấy trạng thái thanh toán thành công",
  "data": {
    "orderId": "ORDER_123456",
    "status": "Pending",
    "amount": 100000,
    "paidAt": null,
    "transactionId": null,
    "message": "Đang chờ thanh toán"
  }
}
```

## 2. API Webhook SePay

### Endpoint: `POST /api/sepay/webhook`

Nhận webhook từ SePay khi có giao dịch thanh toán.

#### Headers:
- `Authorization`: Bearer token từ SePay
- `X-Sepay-Signature`: Chữ ký xác thực webhook
- `X-Sepay-Timestamp`: Timestamp của webhook
- `X-Sepay-Nonce`: Nonce để tránh replay attack

#### Request Body (từ SePay):
```json
{
  "transactionId": 123456789,
  "transactionDate": 1705123456,
  "accountNumber": "1234567890",
  "subAccount": "TEST",
  "amountIn": 100000,
  "amountOut": 0,
  "accumulated": 100000,
  "code": "TRANS_CODE_123",
  "transactionContent": "Thanh toán đơn hàng ORDER_123456",
  "referenceNumber": "REF_123456789",
  "status": "completed"
}
```

#### Response:
```json
{
  "success": true,
  "message": "Webhook processed successfully",
  "data": {
    "id": "guid-here",
    "transactionId": 123456789,
    "gateway": 0,
    "transactionDate": 1705123456,
    "accountNumber": "1234567890",
    "subAccount": "TEST",
    "amountIn": 100000,
    "amountOut": 0,
    "accumulated": 100000,
    "code": "TRANS_CODE_123",
    "transactionContent": "Thanh toán đơn hàng ORDER_123456",
    "referenceNumber": "REF_123456789",
    "body": "{\"transactionId\":123456789,...}",
    "status": 2,
    "errorMessage": null,
    "retryCount": 0,
    "processedAt": "2025-01-13T10:30:00Z",
    "createdAt": "2025-01-13T10:30:00Z",
    "updatedAt": "2025-01-13T10:30:00Z"
  }
}
```

## 3. Các API khác

### Endpoint: `GET /api/sepay/transactions/{id}`

Lấy thông tin giao dịch theo ID.

### Endpoint: `GET /api/sepay/transactions`

Lấy danh sách giao dịch theo khoảng thời gian.

#### Query Parameters:
- `startDate`: Ngày bắt đầu (ISO 8601)
- `endDate`: Ngày kết thúc (ISO 8601)
- `pageIndex`: Trang (mặc định: 1)
- `pageSize`: Số lượng mỗi trang (mặc định: 10)

### Endpoint: `PUT /api/sepay/transactions/{id}/status`

Cập nhật trạng thái giao dịch.

#### Request Body:
```json
{
  "status": 2,
  "errorMessage": "Giao dịch thành công"
}
```

## 4. Cấu hình

### appsettings.json:
```json
{
  "Sepay": {
    "ApiKey": "your-sepay-api-key-here",
    "SecretKey": "your-sepay-secret-key-here",
    "BaseUrl": "https://api.sepay.vn",
    "WebhookUrl": "/api/sepay/webhook",
    "AccountNumber": "your-bank-account-number",
    "AccountName": "CareNest",
    "MerchantGUID": "A000000775",
    "MerchantName": "CareNest",
    "Environment": "Sandbox"
  }
}
```

## 5. Trạng thái giao dịch

- `0`: Pending - Đang chờ thanh toán
- `1`: Processing - Đang xử lý
- `2`: Completed - Hoàn thành
- `3`: Failed - Thất bại
- `4`: Cancelled - Đã hủy
- `5`: Refunded - Đã hoàn tiền

## 6. Lưu ý quan trọng

1. **Bảo mật**: Luôn validate signature của webhook từ SePay
2. **Idempotency**: Xử lý webhook có thể được gọi nhiều lần, cần đảm bảo idempotency
3. **Timeout**: QR code có thời hạn 15 phút
4. **Logging**: Tất cả giao dịch đều được log để debug
5. **Error Handling**: Có retry mechanism cho webhook thất bại

## 7. Testing

### Sandbox Mode:
- Sử dụng `Environment: "Sandbox"` trong config
- API sẽ trả về dữ liệu test
- Webhook có thể được test bằng endpoint `/api/sepay/test/webhook`

### Test Webhook:
```bash
curl -X POST "https://your-domain.com/api/sepay/test/webhook" \
  -H "Content-Type: application/json" \
  -d '{
    "transactionId": 123456,
    "amount": 100000,
    "accountNumber": "TEST_ACCOUNT",
    "description": "Test Webhook Transaction"
  }'
```
