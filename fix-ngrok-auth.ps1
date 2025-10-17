# Script to fix ngrok authtoken configuration
# Run with Administrator privileges

Write-Host "ngrok Authtoken Fix" -ForegroundColor Green
Write-Host "===================" -ForegroundColor Green
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

Write-Host ""
Write-Host "Current authtoken issue:" -ForegroundColor Yellow
Write-Host "The authtoken 'cr_33y2ObZ2iCD211bT1W3VVRROo53' is not valid" -ForegroundColor Red
Write-Host ""

# Clear existing configuration
Write-Host "Step 1: Clearing existing configuration..." -ForegroundColor Magenta

try {
    # Remove existing config file
    $configFile = "C:\Users\GIA BAO\AppData\Local\ngrok\ngrok.yml"
    if (Test-Path $configFile) {
        Remove-Item $configFile -Force
        Write-Host "Removed existing config file" -ForegroundColor Green
    }
    
    # Clear authtoken
    & ngrok config add-authtoken "" 2>$null
    Write-Host "Cleared existing authtoken" -ForegroundColor Green
} catch {
    Write-Host "Failed to clear configuration: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "Step 2: Get correct authtoken" -ForegroundColor Magenta
Write-Host "Please follow these steps:" -ForegroundColor Cyan
Write-Host "1. Go to: https://dashboard.ngrok.com/get-started/your-authtoken" -ForegroundColor White
Write-Host "2. Login to your ngrok account" -ForegroundColor White
Write-Host "3. Copy the authtoken from the dashboard" -ForegroundColor White
Write-Host "4. The authtoken should look like: 2abc123def456ghi789jkl012mno345pqr678stu901vwx234yz" -ForegroundColor White
Write-Host ""

# Get authtoken from user
$AuthToken = Read-Host "Enter your correct ngrok authtoken"

if ($AuthToken -eq "") {
    Write-Host "No authtoken provided. Exiting." -ForegroundColor Red
    exit 1
}

# Validate authtoken format
if ($AuthToken.Length -lt 30 -or $AuthToken.Length -gt 60) {
    Write-Host "Warning: Authtoken length seems incorrect (should be 30-60 characters)" -ForegroundColor Yellow
}

if ($AuthToken -match "[^a-zA-Z0-9]") {
    Write-Host "Warning: Authtoken contains special characters" -ForegroundColor Yellow
}

# Configure authtoken
Write-Host ""
Write-Host "Step 3: Configuring new authtoken..." -ForegroundColor Magenta

try {
    & ngrok config add-authtoken $AuthToken
    Write-Host "Authtoken configured successfully!" -ForegroundColor Green
} catch {
    Write-Host "Failed to configure authtoken: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Verify configuration
Write-Host ""
Write-Host "Step 4: Verifying configuration..." -ForegroundColor Magenta

try {
    $configCheck = & ngrok config check 2>$null
    Write-Host "Configuration verified successfully!" -ForegroundColor Green
} catch {
    Write-Host "Configuration verification failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test ngrok
Write-Host ""
Write-Host "Step 5: Testing ngrok..." -ForegroundColor Magenta

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
