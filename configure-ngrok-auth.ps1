# Script to configure ngrok authtoken
# Run with Administrator privileges

param(
    [string]$AuthToken = ""
)

Write-Host "ngrok Authtoken Configuration" -ForegroundColor Green
Write-Host "==============================" -ForegroundColor Green
Write-Host ""

# Check if ngrok is installed
try {
    $ngrokVersion = & ngrok version 2>$null
    Write-Host "ngrok is installed: $ngrokVersion" -ForegroundColor Green
} catch {
    Write-Host "ngrok is not installed or not in PATH" -ForegroundColor Red
    Write-Host "Please install ngrok first: choco install ngrok -y" -ForegroundColor Yellow
    exit 1
}

# Get authtoken from user if not provided
if ($AuthToken -eq "") {
    Write-Host "Please get your authtoken from: https://dashboard.ngrok.com/get-started/your-cr_33y2ObZ2iCD211bT1W3VVRROo53" -ForegroundColor Cyan
    Write-Host ""
    $AuthToken = Read-Host "Enter your ngrok authtoken"
}

if ($AuthToken -eq "") {
    Write-Host "No authtoken provided. Exiting." -ForegroundColor Red
    exit 1
}

# Configure authtoken
Write-Host ""
Write-Host "Configuring authtoken..." -ForegroundColor Yellow

try {
    & ngrok config add-authtoken $AuthToken
    Write-Host "Authtoken configured successfully!" -ForegroundColor Green
} catch {
    Write-Host "Failed to configure authtoken: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Verify configuration
Write-Host ""
Write-Host "Verifying configuration..." -ForegroundColor Yellow

try {
    $configCheck = & ngrok config check 2>$null
    Write-Host "Configuration verified successfully!" -ForegroundColor Green
} catch {
    Write-Host "Configuration verification failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test ngrok
Write-Host ""
Write-Host "Testing ngrok..." -ForegroundColor Yellow

try {
    $ngrokVersion = & ngrok version 2>$null
    Write-Host "ngrok is working: $ngrokVersion" -ForegroundColor Green
} catch {
    Write-Host "ngrok test failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "Configuration Complete!" -ForegroundColor Green
Write-Host "=======================" -ForegroundColor Green
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "1. Create tunnel: ngrok http 7048" -ForegroundColor White
Write-Host "2. Copy the public URL from ngrok output" -ForegroundColor White
Write-Host "3. Configure webhook in SePay dashboard" -ForegroundColor White
Write-Host "4. Test webhook endpoints" -ForegroundColor White
Write-Host ""

# Ask if user wants to create tunnel now
$createTunnel = Read-Host "Do you want to create tunnel for port 7048 now? (y/n)"
if ($createTunnel -eq "y" -or $createTunnel -eq "Y") {
    Write-Host ""
    Write-Host "Creating tunnel for port 7048..." -ForegroundColor Yellow
    Write-Host "Press Ctrl+C to stop the tunnel" -ForegroundColor Cyan
    Write-Host ""
    
    try {
        & ngrok http 7048
    } catch {
        Write-Host "Failed to create tunnel: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Script completed!" -ForegroundColor Green
