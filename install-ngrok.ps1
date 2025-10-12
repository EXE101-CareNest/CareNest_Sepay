# PowerShell script để cài đặt ngrok với Chocolatey
# Chạy script này với quyền Administrator

param(
    [string]$AuthToken = "",
    [switch]$SkipChocolatey = $false
)

Write-Host "🍫 Installing ngrok with Chocolatey" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""

# Function để kiểm tra quyền Administrator
function Test-Administrator {
    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

# Kiểm tra quyền Administrator
if (-not (Test-Administrator)) {
    Write-Host "❌ This script requires Administrator privileges!" -ForegroundColor Red
    Write-Host "Please run PowerShell as Administrator and try again." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Right-click on PowerShell and select 'Run as Administrator'" -ForegroundColor Cyan
    exit 1
}

Write-Host "✅ Running with Administrator privileges" -ForegroundColor Green
Write-Host ""

# Bước 1: Kiểm tra Chocolatey
Write-Host "1️⃣ Checking Chocolatey installation..." -ForegroundColor Magenta

try {
    $chocoVersion = & choco --version 2>$null
    Write-Host "✅ Chocolatey is already installed: v$chocoVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Chocolatey is not installed" -ForegroundColor Red
    
    if (-not $SkipChocolatey) {
        Write-Host "📥 Installing Chocolatey..." -ForegroundColor Yellow
        
        # Cài đặt Chocolatey
        Set-ExecutionPolicy Bypass -Scope Process -Force
        [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072
        iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))
        
        # Refresh environment variables
        $env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")
        
        Write-Host "✅ Chocolatey installed successfully" -ForegroundColor Green
    } else {
        Write-Host "❌ Cannot proceed without Chocolatey" -ForegroundColor Red
        exit 1
    }
}

Write-Host ""

# Bước 2: Kiểm tra ngrok
Write-Host "2️⃣ Checking ngrok installation..." -ForegroundColor Magenta

try {
    $ngrokVersion = & ngrok version 2>$null
    Write-Host "✅ ngrok is already installed: $ngrokVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ ngrok is not installed" -ForegroundColor Red
    Write-Host "📥 Installing ngrok..." -ForegroundColor Yellow
    
    # Cài đặt ngrok
    & choco install ngrok -y
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ ngrok installed successfully" -ForegroundColor Green
    } else {
        Write-Host "❌ Failed to install ngrok" -ForegroundColor Red
        exit 1
    }
}

Write-Host ""

# Bước 3: Cấu hình authtoken (nếu có)
if ($AuthToken -ne "") {
    Write-Host "3️⃣ Configuring ngrok authtoken..." -ForegroundColor Magenta
    
    try {
        & ngrok config add-authtoken $AuthToken
        Write-Host "✅ Authtoken configured successfully" -ForegroundColor Green
    } catch {
        Write-Host "❌ Failed to configure authtoken" -ForegroundColor Red
        Write-Host "You can configure it manually later with: ngrok config add-authtoken YOUR_TOKEN" -ForegroundColor Yellow
    }
} else {
    Write-Host "3️⃣ Skipping authtoken configuration" -ForegroundColor Yellow
    Write-Host "💡 To configure authtoken later, run: ngrok config add-authtoken YOUR_TOKEN" -ForegroundColor Cyan
}

Write-Host ""

# Bước 4: Test ngrok
Write-Host "4️⃣ Testing ngrok installation..." -ForegroundColor Magenta

try {
    $ngrokVersion = & ngrok version 2>$null
    Write-Host "✅ ngrok is working: $ngrokVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ ngrok test failed" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Bước 5: Hiển thị thông tin
Write-Host "🎉 Installation Complete!" -ForegroundColor Green
Write-Host "========================" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Next Steps:" -ForegroundColor Cyan
Write-Host "1. Get free authtoken from https://ngrok.com" -ForegroundColor White
Write-Host "2. Configure authtoken: ngrok config add-authtoken YOUR_TOKEN" -ForegroundColor White
Write-Host "3. Start your application: dotnet run --environment Development" -ForegroundColor White
Write-Host "4. Create tunnel: ngrok http 7000" -ForegroundColor White
Write-Host "5. Use the public URL for SePay webhook configuration" -ForegroundColor White
Write-Host ""
Write-Host "🔧 Quick Commands:" -ForegroundColor Cyan
Write-Host "ngrok http 7000                    # Create tunnel for port 7000" -ForegroundColor White
Write-Host "ngrok http 7000 --subdomain=name   # Custom subdomain (Pro plan)" -ForegroundColor White
Write-Host "ngrok config add-authtoken TOKEN   # Configure authtoken" -ForegroundColor White
Write-Host "ngrok help                         # Show help" -ForegroundColor White
Write-Host ""
Write-Host "📚 Documentation:" -ForegroundColor Cyan
Write-Host "https://ngrok.com/docs" -ForegroundColor White
Write-Host ""

# Bước 6: Tùy chọn chạy setup script
$runSetup = Read-Host "Do you want to run the webhook setup script now? (y/n)"
if ($runSetup -eq "y" -or $runSetup -eq "Y") {
    if (Test-Path "setup-webhook-tunnel.ps1") {
        Write-Host "🚀 Starting webhook setup..." -ForegroundColor Green
        & .\setup-webhook-tunnel.ps1 -TunnelType ngrok
    } else {
        Write-Host "❌ setup-webhook-tunnel.ps1 not found" -ForegroundColor Red
        Write-Host "Please run it manually: .\setup-webhook-tunnel.ps1 -TunnelType ngrok" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "👋 Installation completed successfully!" -ForegroundColor Green
