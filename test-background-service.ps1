# Test Background Service
Write-Host "🧪 Testing Background Service" -ForegroundColor Green

# Test webhook endpoint
$webhookData = @{
    gateway = "BIDV"
    transactionDate = "2025-10-20 11:50:00"
    accountNumber = "3149978378"
    subAccount = "9624717052004"
    code = $null
    content = "Test background processing"
    transferType = "in"
    description = "Background service test"
    transferAmount = 75000
    referenceCode = "bg-test-$(Get-Date -Format 'yyyyMMddHHmmss')"
    accumulated = 0
    id = 999888
}

$headers = @{
    "Authorization" = "Apikey B5BUXOXHITVUY51ISJWMKAB1EOFNWVT7VMQGKG34L4NLO8VSMRGA3ZC9QEKDBFNR"
    "Content-Type" = "application/json"
}

$body = $webhookData | ConvertTo-Json -Depth 3

Write-Host "📤 Sending webhook request..." -ForegroundColor Yellow
$startTime = Get-Date

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5291/api/sepay/webhook" -Method POST -Headers $headers -Body $body -TimeoutSec 10
    
    $endTime = Get-Date
    $duration = ($endTime - $startTime).TotalMilliseconds
    
    Write-Host "✅ Webhook Response:" -ForegroundColor Green
    Write-Host "   Status: $($response.status)" -ForegroundColor White
    Write-Host "   Message: $($response.message)" -ForegroundColor White
    Write-Host "   Transaction ID: $($response.data.transactionId)" -ForegroundColor White
    Write-Host "   Response Time: ${duration}ms" -ForegroundColor Cyan
    
    Write-Host ""
    Write-Host "⏳ Waiting for background processing..." -ForegroundColor Yellow
    Start-Sleep -Seconds 3
    
    Write-Host "📋 Check logs for background processing messages:" -ForegroundColor Blue
    Write-Host "   - 'Webhook Background Service started'" -ForegroundColor White
    Write-Host "   - 'Processing webhook in background'" -ForegroundColor White
    Write-Host "   - 'Background webhook processed successfully'" -ForegroundColor White
    
} catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "🔍 To check logs, run:" -ForegroundColor Blue
Write-Host "   Get-Content logs/sepay-*.txt | Select-String 'background'" -ForegroundColor White

