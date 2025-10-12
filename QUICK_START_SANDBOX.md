# 🚀 Quick Start - SePay Sandbox Testing

## ⚡ Bắt đầu nhanh trong 3 bước

### 1️⃣ Setup Webhook Tunnel (Cho SePay)

**Option A: Sử dụng ngrok (Khuyến nghị)**

```bash
# Cài đặt ngrok từ https://ngrok.com/download
# Sau đó chạy:
.\setup-webhook-tunnel.ps1 -TunnelType ngrok
```

**Option B: Sử dụng localtunnel**

```bash
# Cài đặt: npm install -g localtunnel
# Sau đó chạy:
.\setup-webhook-tunnel.ps1 -TunnelType localtunnel
```

### 2️⃣ Cấu hình Webhook trong SePay

1. Copy Public Webhook URL từ script trên
2. Vào SePay dashboard → Webhook Properties
3. Paste URL vào "Gọi đến URL"
4. Chọn "Không" cho "Là WebHooks xác thực thanh toán?"
5. Tick "Gọi lại Webhooks khi HTTP Status Code không nằm trong phạm vi từ 200 đến 299"

### 3️⃣ Test Webhook

**Test bằng HTTP file:**

- Mở file `test-sandbox.http` trong VS Code
- Click "Send Request" cho từng test case

**Test bằng PowerShell:**

```powershell
.\test-sandbox.ps1
```

## 🎯 Test Cases chính

| Test                 | Endpoint                           | Mục đích                   |
| -------------------- | ---------------------------------- | -------------------------- |
| **Sandbox Test**     | `POST /api/sepay/test/sandbox`     | Tạo transaction test       |
| **Webhook Test**     | `POST /api/sepay/test/webhook`     | Test xử lý webhook         |
| **Real Webhook**     | `POST /api/sepay/webhook`          | Simulate SePay webhook     |
| **Get Transaction**  | `GET /api/sepay/transactions/{id}` | Lấy transaction theo ID    |
| **Get Transactions** | `GET /api/sepay/transactions`      | Lấy danh sách transactions |

## 📋 Cấu hình cần thiết

File `appsettings.Development.json`:

```json
{
  "Sepay": {
    "ApiKey": "sandbox-api-key-here",
    "SecretKey": "sandbox-secret-key-here",
    "BaseUrl": "https://sandbox.sepay.vn/api",
    "Environment": "Sandbox"
  }
}
```

## 🔍 Kiểm tra kết quả

- **Logs**: `logs/sepay-YYYYMMDD.txt`
- **Database**: PostgreSQL `carenest-sepay-dev`
- **Console**: Application logs khi chạy

## 📚 Tài liệu chi tiết

Xem file `SANDBOX_TESTING_GUIDE.md` để có hướng dẫn đầy đủ.

---

**Lưu ý**: Đảm bảo PostgreSQL đang chạy và database đã được migrate trước khi test.
