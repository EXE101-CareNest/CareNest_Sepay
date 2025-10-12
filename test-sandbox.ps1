# PowerShell script để test SePay Sandbox
# Chạy script này sau khi khởi động ứng dụng

param(
    [string]$BaseUrl = "https://localhost:7000",
    [int]$TestAmount = 100000,
    [string]$TestDescription = "PowerShell Test Transaction"
)

Write-Host "🧪 Testing SePay Sandbox Environment" -ForegroundColor Green
Write-Host "Base URL: $BaseUrl" -ForegroundColor Yellow
Write-Host ""

# Function để gửi HTTP request
function Send-TestRequest {
    param(
        [string]$Method,
        [string]$Endpoint,
        [object]$Body = $null,
        [hashtable]$Headers = @{}
    )
    
    $uri = "$BaseUrl$Endpoint"
    $headers["Content-Type"] = "application/json"
    
    try {
        if ($Body) {
            $jsonBody = $Body | ConvertTo-Json -Depth 10
            Write-Host "Request Body: $jsonBody" -ForegroundColor Cyan
            $response = Invoke-RestMethod -Uri $uri -Method $Method -Body $jsonBody -Headers $headers
        } else {
            $response = Invoke-RestMethod -Uri $uri -Method $Method -Headers $headers
        }
        
        Write-Host "✅ Success: $Method $Endpoint" -ForegroundColor Green
        $response | ConvertTo-Json -Depth 10 | Write-Host -ForegroundColor White
        return $response
    }
    catch {
        Write-Host "❌ Error: $Method $Endpoint" -ForegroundColor Red
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $responseBody = $reader.ReadToEnd()
            Write-Host "Response: $responseBody" -ForegroundColor Red
        }
        return $null
    }
    Write-Host ""
}

# Test 1: Test Sandbox Transaction Creation
Write-Host "1️⃣ Testing Sandbox Transaction Creation" -ForegroundColor Magenta
$testRequest = @{
    amount = $TestAmount
    description = $TestDescription
}
$sandboxResult = Send-TestRequest -Method "POST" -Endpoint "/api/sepay/test/sandbox" -Body $testRequest

# Test 2: Test Webhook Processing
Write-Host "2️⃣ Testing Webhook Processing" -ForegroundColor Magenta
$webhookRequest = @{
    transactionId = 123456
    amount = 50000
    accountNumber = "TEST_ACCOUNT_001"
    description = "PowerShell Webhook Test"
}
$webhookResult = Send-TestRequest -Method "POST" -Endpoint "/api/sepay/test/webhook" -Body $webhookRequest

# Test 3: Test Real Webhook Simulation
Write-Host "3️⃣ Testing Real Webhook Simulation" -ForegroundColor Magenta
$realWebhookRequest = @{
    transactionId = 789012
    transactionDate = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
    accountNumber = "1234567890"
    subAccount = "SUB001"
    amountIn = 75000
    amountOut = 0
    accumulated = 75000
    code = "PAYMENT_CODE"
    transactionContent = "Payment for order #12345"
    referenceNumber = "REF_$(Get-Date -Format 'yyyyMMddHHmmss')"
    status = "Completed"
}
$webhookHeaders = @{
    "Authorization" = "Bearer sandbox-api-key-here"
    "X-Sepay-Signature" = "test-signature-here"
}
$realWebhookResult = Send-TestRequest -Method "POST" -Endpoint "/api/sepay/webhook" -Body $realWebhookRequest -Headers $webhookHeaders

# Test 4: Get Transaction by ID (nếu có transaction ID từ test trước)
if ($sandboxResult -and $sandboxResult.data -and $sandboxResult.data.transaction -and $sandboxResult.data.transaction.transactionId) {
    Write-Host "4️⃣ Testing Get Transaction by ID" -ForegroundColor Magenta
    $transactionId = $sandboxResult.data.transaction.transactionId
    $getResult = Send-TestRequest -Method "GET" -Endpoint "/api/sepay/transactions/$transactionId"
}

# Test 5: Get Transactions by Date Range
Write-Host "5️⃣ Testing Get Transactions by Date Range" -ForegroundColor Magenta
$startDate = (Get-Date).AddDays(-7).ToString("yyyy-MM-dd")
$endDate = (Get-Date).ToString("yyyy-MM-dd")
$dateRangeResult = Send-TestRequest -Method "GET" -Endpoint "/api/sepay/transactions?startDate=$startDate&endDate=$endDate&pageIndex=1&pageSize=10"

# Test 6: Test Error Scenarios
Write-Host "6️⃣ Testing Error Scenarios" -ForegroundColor Magenta

# Test với invalid transaction ID
Write-Host "Testing invalid transaction ID..." -ForegroundColor Yellow
$invalidIdResult = Send-TestRequest -Method "GET" -Endpoint "/api/sepay/transactions/invalid-id"

# Test với negative amount
Write-Host "Testing negative amount..." -ForegroundColor Yellow
$negativeAmountRequest = @{
    amount = -1000
    description = "Negative amount test"
}
$negativeAmountResult = Send-TestRequest -Method "POST" -Endpoint "/api/sepay/test/sandbox" -Body $negativeAmountRequest

# Test với empty request body
Write-Host "Testing empty request body..." -ForegroundColor Yellow
$emptyBodyResult = Send-TestRequest -Method "POST" -Endpoint "/api/sepay/test/sandbox" -Body @{}

# Test 7: Performance Test
Write-Host "7️⃣ Testing Performance (Multiple Requests)" -ForegroundColor Magenta
$performanceResults = @()
for ($i = 1; $i -le 5; $i++) {
    Write-Host "Sending request $i/5..." -ForegroundColor Yellow
    $perfRequest = @{
        amount = Get-Random -Minimum 10000 -Maximum 100000
        description = "Performance test request $i"
    }
    $perfResult = Send-TestRequest -Method "POST" -Endpoint "/api/sepay/test/sandbox" -Body $perfRequest
    $performanceResults += $perfResult
}

# Summary
Write-Host ""
Write-Host "📊 Test Summary" -ForegroundColor Green
Write-Host "===============" -ForegroundColor Green

$totalTests = 0
$passedTests = 0

# Count results
if ($sandboxResult) { $passedTests++ }
$totalTests++

if ($webhookResult) { $passedTests++ }
$totalTests++

if ($realWebhookResult) { $passedTests++ }
$totalTests++

if ($getResult) { $passedTests++ }
$totalTests++

if ($dateRangeResult) { $passedTests++ }
$totalTests++

# Error tests (should fail gracefully)
if ($invalidIdResult -eq $null) { $passedTests++ }
$totalTests++

if ($negativeAmountResult -eq $null) { $passedTests++ }
$totalTests++

if ($emptyBodyResult -eq $null) { $passedTests++ }
$totalTests++

Write-Host "Total Tests: $totalTests" -ForegroundColor White
Write-Host "Passed Tests: $passedTests" -ForegroundColor Green
Write-Host "Failed Tests: $($totalTests - $passedTests)" -ForegroundColor Red

if ($passedTests -eq $totalTests) {
    Write-Host "🎉 All tests passed! Sandbox environment is working correctly." -ForegroundColor Green
} else {
    Write-Host "⚠️ Some tests failed. Please check the logs and configuration." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "📝 Next Steps:" -ForegroundColor Cyan
Write-Host "1. Check application logs in logs/ folder" -ForegroundColor White
Write-Host "2. Verify database transactions" -ForegroundColor White
Write-Host "3. Review any error messages above" -ForegroundColor White
Write-Host "4. Test with different amounts and scenarios" -ForegroundColor White

Write-Host ""
Write-Host "🔗 Useful Commands:" -ForegroundColor Cyan
Write-Host "View logs: Get-Content logs/sepay-$(Get-Date -Format 'yyyyMMdd').txt -Tail 50" -ForegroundColor White
Write-Host "Check database: psql -h localhost -U postgres -d carenest-sepay-dev -c 'SELECT * FROM \"SepayTransactions\" ORDER BY \"CreatedAt\" DESC LIMIT 10;'" -ForegroundColor White
