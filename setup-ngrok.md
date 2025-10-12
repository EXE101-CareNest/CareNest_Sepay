# 🌐 Hướng dẫn sử dụng ngrok cho SePay Webhook

## 📥 Cài đặt ngrok

### Windows:

```bash
# Tải ngrok từ https://ngrok.com/download
# Hoặc sử dụng chocolatey
choco install ngrok

# Hoặc sử dụng winget
winget install ngrok.ngrok
```

### Mac:

```bash
brew install ngrok
```

### Linux:

```bash
# Tải từ https://ngrok.com/download
wget https://bin.equinox.io/c/bNyj1mQVY4c/ngrok-v3-stable-linux-amd64.zip
unzip ngrok-v3-stable-linux-amd64.zip
sudo mv ngrok /usr/local/bin
```

## 🚀 Sử dụng ngrok

### Bước 1: Khởi động ứng dụng

```bash
cd CareNest_SePay
dotnet run --environment Development
```

### Bước 2: Tạo tunnel với ngrok

```bash
# Tạo tunnel cho port 7000 (HTTPS)
ngrok http 7000

# Hoặc tạo tunnel cho port 5000 (HTTP)
ngrok http 5000
```

### Bước 3: Lấy public URL

Ngrok sẽ hiển thị URL như:

```
Forwarding    https://abc123.ngrok.io -> http://localhost:7000
```

### Bước 4: Cấu hình webhook URL trong SePay

Sử dụng URL: `https://abc123.ngrok.io/api/sepay/webhook`

## 🔐 Cấu hình ngrok với authentication

### Tạo tài khoản ngrok (miễn phí):

1. Đăng ký tại https://ngrok.com
2. Lấy authtoken từ dashboard
3. Cấu hình:

```bash
ngrok config add-authtoken YOUR_AUTHTOKEN
```

### Sử dụng custom subdomain (Pro):

```bash
ngrok http 7000 --subdomain=your-custom-name
```

## 📝 Script tự động

Tạo file `start-with-ngrok.ps1`:

```powershell
# Start application
Start-Process -FilePath "dotnet" -ArgumentList "run --environment Development" -WorkingDirectory "CareNest_SePay"

# Wait for application to start
Start-Sleep -Seconds 10

# Start ngrok
Start-Process -FilePath "ngrok" -ArgumentList "http 7000"
```

## ⚠️ Lưu ý quan trọng

1. **URL thay đổi**: Mỗi lần restart ngrok, URL sẽ thay đổi
2. **HTTPS**: Ngrok tự động cung cấp HTTPS
3. **Rate limiting**: Free plan có giới hạn requests
4. **Security**: Chỉ sử dụng cho development/testing

## 🔄 Workflow hoàn chỉnh

1. Khởi động ứng dụng: `dotnet run`
2. Khởi động ngrok: `ngrok http 7000`
3. Copy URL từ ngrok (ví dụ: `https://abc123.ngrok.io`)
4. Cấu hình webhook URL trong SePay: `https://abc123.ngrok.io/api/sepay/webhook`
5. Test webhook từ SePay dashboard
