# QR Code Generation Demo - Sử dụng Sepay URL

## Cách hoạt động mới

Thay vì sử dụng API phức tạp, giờ đây hệ thống sử dụng URL trực tiếp từ Sepay để tạo QR code:

```
https://qr.sepay.vn/img?acc=SO_TAI_KHOAN&bank=NGAN_HANG&amount=SO_TIEN&des=NOI_DUNG&template=TEMPLATE&download=DOWNLOAD
```

## Ví dụ sử dụng

### 1. Tạo QR Code qua API

**Request:**
```http
POST /api/payment/create-qr
Content-Type: application/json

{
  "amount": 100000,
  "description": "Thanh toán đơn hàng #12345",
  "orderId": "ORDER_12345",
  "bankCode": "VPBANK",
  "accountNumber": "1234567890",
  "template": "compact",
  "download": false
}
```

**Response:**
```json
{
  "success": true,
  "message": "Tạo mã QR thành công",
  "data": {
    "qrCode": "https://qr.sepay.vn/img?acc=1234567890&bank=VPBANK&amount=100000&des=Thanh%20to%C3%A1n%20%C4%91%C6%A1n%20h%C3%A0ng%20%2312345&template=compact",
    "qrImageUrl": "https://qr.sepay.vn/img?acc=1234567890&bank=VPBANK&amount=100000&des=Thanh%20to%C3%A1n%20%C4%91%C6%A1n%20h%C3%A0ng%20%2312345&template=compact",
    "bankName": "Ngân hàng TMCP Việt Nam Thịnh Vượng",
    "bankCode": "VPBANK",
    "accountNumber": "1234567890",
    "accountName": "CareNest",
    "amount": 100000,
    "description": "Thanh toán đơn hàng #12345",
    "orderId": "ORDER_12345",
    "template": "compact",
    "download": false,
    "createdAt": "2025-01-17T10:30:00Z",
    "expiresAt": "2025-01-17T10:45:00Z"
  }
}
```

### 2. Sử dụng QR URL trong HTML

```html
<!-- Hiển thị QR code trực tiếp -->
<img src="https://qr.sepay.vn/img?acc=1234567890&bank=VPBANK&amount=100000&des=Thanh%20to%C3%A1n%20%C4%91%C6%A1n%20h%C3%A0ng%20%2312345" 
     alt="QR Code thanh toán" 
     width="200" 
     height="200" />

<!-- QR code với template compact -->
<img src="https://qr.sepay.vn/img?acc=1234567890&bank=VPBANK&amount=100000&des=Thanh%20to%C3%A1n%20%C4%91%C6%A1n%20h%C3%A0ng%20%2312345&template=compact" 
     alt="QR Code compact" 
     width="200" 
     height="200" />

<!-- QR code chỉ có QR (không có text) -->
<img src="https://qr.sepay.vn/img?acc=1234567890&bank=VPBANK&amount=100000&des=Thanh%20to%C3%A1n%20%C4%91%C6%A1n%20h%C3%A0ng%20%2312345&template=qronly" 
     alt="QR Code only" 
     width="200" 
     height="200" />
```

### 3. Các tham số URL

| Tham số | Bắt buộc | Mô tả | Ví dụ |
|---------|----------|-------|-------|
| `acc` | ✅ | Số tài khoản ngân hàng | `1234567890` |
| `bank` | ✅ | Code ngân hàng | `VPBANK`, `BIDV`, `VIETINBANK` |
| `amount` | ❌ | Số tiền cần chuyển | `100000` |
| `des` | ❌ | Nội dung chuyển khoản | `Thanh%20to%C3%A1n%20%C4%91%C6%A1n%20h%C3%A0ng` |
| `template` | ❌ | Template QR | `""`, `compact`, `qronly` |
| `download` | ❌ | Tải QR về máy | `true`, `false` |

### 4. Danh sách Bank Codes hỗ trợ

| Bank Code | Tên ngân hàng |
|-----------|---------------|
| `VPBANK` | Ngân hàng TMCP Việt Nam Thịnh Vượng |
| `BIDV` | Ngân hàng TMCP Đầu tư và Phát triển Việt Nam |
| `VIETINBANK` | Ngân hàng TMCP Công Thương Việt Nam |
| `ACB` | Ngân hàng TMCP Á Châu |
| `OCB` | Ngân hàng TMCP Phương Đông |
| `KIENLONGBANK` | Ngân hàng TMCP Kiên Long |
| `MSB` | Ngân hàng TMCP Hàng Hải |
| `TECHCOMBANK` | Ngân hàng TMCP Kỹ Thương Việt Nam |
| `AGRIBANK` | Ngân hàng Nông nghiệp và Phát triển Nông thôn Việt Nam |
| `MBBANK` | Ngân hàng TMCP Quân đội |
| `SACOMBANK` | Ngân hàng TMCP Sài Gòn Thương Tín |
| `VIETCOMBANK` | Ngân hàng TMCP Ngoại Thương Việt Nam |

## Ưu điểm của cách mới

1. **Đơn giản hơn**: Không cần gọi API phức tạp
2. **Nhanh hơn**: Tạo URL trực tiếp, không cần xử lý JSON
3. **Ít lỗi hơn**: Không phụ thuộc vào API external
4. **Dễ debug**: URL có thể test trực tiếp trên browser
5. **Linh hoạt**: Có thể customize template và download option

## Cấu hình

Cập nhật `appsettings.json`:

```json
{
  "Sepay": {
    "AccountNumber": "1234567890",
    "AccountName": "CareNest",
    "BankCode": "VPBANK"
  }
}
```

## Test QR Code

Bạn có thể test QR code bằng cách:

1. Gọi API `/api/payment/create-qr`
2. Copy URL từ response
3. Paste vào browser để xem QR code
4. Hoặc sử dụng URL trực tiếp trong thẻ `<img>`
