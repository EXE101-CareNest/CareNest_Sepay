# PowerShell script để setup webhook tunnel cho SePay
param(
    [string]$TunnelType = "ngrok",  # "ngrok" hoặc "localtunnel"
    [string]$Port = "7000",
    [string]$Subdomain = "carenest-sepay"
)

Write-Host "🌐 Setting up Webhook Tunnel for SePay" -ForegroundColor Green
Write-Host "Tunnel Type: $TunnelType" -ForegroundColor Yellow
Write-Host "Port: $Port" -ForegroundColor Yellow
Write-Host ""

# Function để kiểm tra port có đang được sử dụng không
function Test-Port {
    param([int]$Port)
    try {
        $connection = New-Object System.Net.Sockets.TcpClient
        $connection.Connect("localhost", $Port)
        $connection.Close()
        return $true
    }
    catch {
        return $false
    }
}

# Function để đợi ứng dụng khởi động
function Wait-ForApplication {
    param([int]$Port, [int]$TimeoutSeconds = 30)
    
    Write-Host "⏳ Waiting for application to start on port $Port..." -ForegroundColor Yellow
    $elapsed = 0
    
    while ($elapsed -lt $TimeoutSeconds) {
        if (Test-Port -Port $Port) {
            Write-Host "✅ Application is running on port $Port" -ForegroundColor Green
            return $true
        }
        Start-Sleep -Seconds 2
        $elapsed += 2
        Write-Host "." -NoNewline -ForegroundColor Yellow
    }
    
    Write-Host ""
    Write-Host "❌ Application failed to start within $TimeoutSeconds seconds" -ForegroundColor Red
    return $false
}

# Function để lấy public URL từ ngrok
function Get-NgrokUrl {
    try {
        $response = Invoke-RestMethod -Uri "http://localhost:4040/api/tunnels" -Method Get
        $httpsTunnel = $response.tunnels | Where-Object { $_.proto -eq "https" }
        if ($httpsTunnel) {
            return $httpsTunnel.public_url
        }
        return $null
    }
    catch {
        return $null
    }
}

# Function để cập nhật cấu hình
function Update-WebhookConfig {
    param([string]$PublicUrl)
    
    $configFile = "CareNest_SePay/appsettings.Development.json"
    if (Test-Path $configFile) {
        $config = Get-Content $configFile | ConvertFrom-Json
        $config.Sepay.PublicWebhookUrl = "$PublicUrl/api/sepay/webhook"
        $config | ConvertTo-Json -Depth 10 | Set-Content $configFile
        Write-Host "✅ Updated webhook URL in configuration: $($config.Sepay.PublicWebhookUrl)" -ForegroundColor Green
    }
}

# Bước 1: Khởi động ứng dụng
Write-Host "1️⃣ Starting CareNest SePay Application..." -ForegroundColor Magenta
$appProcess = Start-Process -FilePath "dotnet" -ArgumentList "run --environment Development" -WorkingDirectory "CareNest_SePay" -PassThru -WindowStyle Hidden

# Đợi ứng dụng khởi động
if (-not (Wait-ForApplication -Port $Port)) {
    Write-Host "❌ Failed to start application" -ForegroundColor Red
    exit 1
}

# Bước 2: Khởi động tunnel
Write-Host ""
Write-Host "2️⃣ Starting $TunnelType tunnel..." -ForegroundColor Magenta

if ($TunnelType -eq "ngrok") {
    # Kiểm tra ngrok có được cài đặt không
    try {
        $ngrokVersion = & ngrok version 2>$null
        Write-Host "✅ ngrok is installed: $ngrokVersion" -ForegroundColor Green
    }
    catch {
        Write-Host "❌ ngrok is not installed. Please install ngrok first." -ForegroundColor Red
        Write-Host "Download from: https://ngrok.com/download" -ForegroundColor Yellow
        exit 1
    }
    
    # Khởi động ngrok
    $tunnelProcess = Start-Process -FilePath "ngrok" -ArgumentList "http $Port" -PassThru -WindowStyle Hidden
    
    # Đợi ngrok khởi động
    Start-Sleep -Seconds 5
    
    # Lấy public URL
    $publicUrl = Get-NgrokUrl
    if ($publicUrl) {
        Write-Host "✅ ngrok tunnel created: $publicUrl" -ForegroundColor Green
        Update-WebhookConfig -PublicUrl $publicUrl
    } else {
        Write-Host "❌ Failed to get ngrok URL" -ForegroundColor Red
    }
    
} elseif ($TunnelType -eq "localtunnel") {
    # Kiểm tra localtunnel có được cài đặt không
    try {
        $ltVersion = & lt --version 2>$null
        Write-Host "✅ localtunnel is installed: $ltVersion" -ForegroundColor Green
    }
    catch {
        Write-Host "❌ localtunnel is not installed. Please install localtunnel first." -ForegroundColor Red
        Write-Host "Run: npm install -g localtunnel" -ForegroundColor Yellow
        exit 1
    }
    
    # Khởi động localtunnel
    $tunnelProcess = Start-Process -FilePath "lt" -ArgumentList "--port $Port --subdomain $Subdomain" -PassThru -WindowStyle Hidden
    
    # Đợi localtunnel khởi động
    Start-Sleep -Seconds 5
    
    $publicUrl = "https://$Subdomain.loca.lt"
    Write-Host "✅ localtunnel created: $publicUrl" -ForegroundColor Green
    Update-WebhookConfig -PublicUrl $publicUrl
}

# Bước 3: Hiển thị thông tin
Write-Host ""
Write-Host "🎉 Setup Complete!" -ForegroundColor Green
Write-Host "==================" -ForegroundColor Green
Write-Host "Application URL: http://localhost:$Port" -ForegroundColor White
Write-Host "Public Webhook URL: $publicUrl/api/sepay/webhook" -ForegroundColor White
Write-Host ""
Write-Host "📋 Next Steps:" -ForegroundColor Cyan
Write-Host "1. Copy the Public Webhook URL above" -ForegroundColor White
Write-Host "2. Go to SePay dashboard" -ForegroundColor White
Write-Host "3. Configure webhook with the Public URL" -ForegroundColor White
Write-Host "4. Test webhook from SePay" -ForegroundColor White
Write-Host ""
Write-Host "🔧 Test Commands:" -ForegroundColor Cyan
Write-Host "Test webhook: curl -X POST $publicUrl/api/sepay/webhook -H 'Content-Type: application/json' -d '{\"test\": true}'" -ForegroundColor White
Write-Host ""
Write-Host "⚠️  Press Ctrl+C to stop both application and tunnel" -ForegroundColor Yellow

# Đợi user nhấn Ctrl+C
try {
    while ($true) {
        Start-Sleep -Seconds 1
    }
}
finally {
    Write-Host ""
    Write-Host "🛑 Stopping processes..." -ForegroundColor Yellow
    
    if ($appProcess -and !$appProcess.HasExited) {
        $appProcess.Kill()
        Write-Host "✅ Application stopped" -ForegroundColor Green
    }
    
    if ($tunnelProcess -and !$tunnelProcess.HasExited) {
        $tunnelProcess.Kill()
        Write-Host "✅ Tunnel stopped" -ForegroundColor Green
    }
    
    Write-Host "👋 Goodbye!" -ForegroundColor Green
}
