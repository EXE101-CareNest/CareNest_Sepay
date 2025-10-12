# 🌐 Hướng dẫn sử dụng localtunnel cho SePay Webhook

## 📥 Cài đặt localtunnel

### Cài đặt Node.js trước:

```bash
# Tải Node.js từ https://nodejs.org
# Hoặc sử dụng chocolatey (Windows)
choco install nodejs

# Hoặc sử dụng winget (Windows)
winget install OpenJS.NodeJS
```

### Cài đặt localtunnel:

```bash
npm install -g localtunnel
```

## 🚀 Sử dụng localtunnel

### Bước 1: Khởi động ứng dụng

```bash
cd CareNest_SePay
dotnet run --environment Development
```

### Bước 2: Tạo tunnel

```bash
# Tạo tunnel cho port 7000
lt --port 7000

# Hoặc với custom subdomain
lt --port 7000 --subdomain your-custom-name
```

### Bước 3: Lấy public URL

Localtunnel sẽ hiển thị URL như:

```
your url is: https://your-custom-name.loca.lt
```

### Bước 4: Cấu hình webhook URL trong SePay

Sử dụng URL: `https://your-custom-name.loca.lt/api/sepay/webhook`

## 📝 Script tự động

Tạo file `start-with-localtunnel.ps1`:

```powershell
# Start application
Start-Process -FilePath "dotnet" -ArgumentList "run --environment Development" -WorkingDirectory "CareNest_SePay"

# Wait for application to start
Start-Sleep -Seconds 10

# Start localtunnel
Start-Process -FilePath "lt" -ArgumentList "--port 7000 --subdomain carenest-sepay"
```

## ⚠️ Lưu ý

1. **Subdomain**: Có thể sử dụng custom subdomain
2. **HTTPS**: Localtunnel tự động cung cấp HTTPS
3. **Stability**: Có thể ít ổn định hơn ngrok
4. **Free**: Hoàn toàn miễn phí
