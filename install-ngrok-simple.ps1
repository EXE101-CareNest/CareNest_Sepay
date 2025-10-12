# Simple ngrok installation script
# Run with Administrator privileges

param(
    [string]$AuthToken = ""
)

Write-Host "Simple ngrok Installation" -ForegroundColor Green
Write-Host "=========================" -ForegroundColor Green
Write-Host ""

# Check Administrator privileges
function Test-Administrator {
    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

if (-not (Test-Administrator)) {
    Write-Host "This script requires Administrator privileges!" -ForegroundColor Red
    Write-Host "Please run PowerShell as Administrator and try again." -ForegroundColor Yellow
    exit 1
}

Write-Host "Running with Administrator privileges" -ForegroundColor Green
Write-Host ""

# Method 1: Try Chocolatey
Write-Host "Method 1: Trying Chocolatey..." -ForegroundColor Magenta

# Add Chocolatey to PATH
$env:Path += ";C:\ProgramData\chocolatey\bin"

try {
    $chocoVersion = & choco --version 2>$null
    Write-Host "Chocolatey found: $chocoVersion" -ForegroundColor Green
    
    # Install ngrok via Chocolatey
    Write-Host "Installing ngrok via Chocolatey..." -ForegroundColor Yellow
    & choco install ngrok -y
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "ngrok installed successfully via Chocolatey" -ForegroundColor Green
        
        # Test ngrok
        $ngrokVersion = & ngrok version 2>$null
        if ($ngrokVersion) {
            Write-Host "ngrok is working: $ngrokVersion" -ForegroundColor Green
            $success = $true
        }
    }
} catch {
    Write-Host "Chocolatey method failed: $($_.Exception.Message)" -ForegroundColor Red
    $success = $false
}

# Method 2: Direct download if Chocolatey failed
if (-not $success) {
    Write-Host ""
    Write-Host "Method 2: Direct download..." -ForegroundColor Magenta
    
    try {
        # Download ngrok
        $url = "https://bin.equinox.io/c/bNyj1mQVY4c/ngrok-v3-stable-windows-amd64.zip"
        $output = "ngrok.zip"
        $installDir = "C:\ngrok"
        
        Write-Host "Downloading ngrok from $url..." -ForegroundColor Yellow
        Invoke-WebRequest -Uri $url -OutFile $output -UseBasicParsing
        
        Write-Host "Extracting to $installDir..." -ForegroundColor Yellow
        if (Test-Path $installDir) {
            Remove-Item $installDir -Recurse -Force
        }
        Expand-Archive -Path $output -DestinationPath $installDir -Force
        
        # Add to PATH
        $env:Path += ";$installDir"
        
        # Test ngrok
        $ngrokVersion = & ngrok version 2>$null
        if ($ngrokVersion) {
            Write-Host "ngrok installed successfully via direct download: $ngrokVersion" -ForegroundColor Green
            $success = $true
        }
        
        # Cleanup
        Remove-Item $output -Force
        
    } catch {
        Write-Host "Direct download failed: $($_.Exception.Message)" -ForegroundColor Red
        $success = $false
    }
}

# Configure authtoken if provided
if ($success -and $AuthToken -ne "") {
    Write-Host ""
    Write-Host "Configuring authtoken..." -ForegroundColor Magenta
    
    try {
        & ngrok config add-authtoken $AuthToken
        Write-Host "Authtoken configured successfully" -ForegroundColor Green
    } catch {
        Write-Host "Failed to configure authtoken: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Final status
Write-Host ""
if ($success) {
    Write-Host "Installation Complete!" -ForegroundColor Green
    Write-Host "=====================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next Steps:" -ForegroundColor Cyan
    Write-Host "1. Get free authtoken from https://ngrok.com" -ForegroundColor White
    Write-Host "2. Configure authtoken: ngrok config add-authtoken YOUR_TOKEN" -ForegroundColor White
    Write-Host "3. Start your application: dotnet run --environment Development" -ForegroundColor White
    Write-Host "4. Create tunnel: ngrok http 7000" -ForegroundColor White
    Write-Host "5. Use the public URL for SePay webhook configuration" -ForegroundColor White
    Write-Host ""
    Write-Host "Quick Commands:" -ForegroundColor Cyan
    Write-Host "ngrok http 7000                    # Create tunnel for port 7000" -ForegroundColor White
    Write-Host "ngrok http 7000 --subdomain=name   # Custom subdomain (Pro plan)" -ForegroundColor White
    Write-Host "ngrok config add-authtoken TOKEN   # Configure authtoken" -ForegroundColor White
    Write-Host "ngrok help                         # Show help" -ForegroundColor White
    
    # Test ngrok
    Write-Host ""
    $testNgrok = Read-Host "Do you want to test ngrok now? (y/n)"
    if ($testNgrok -eq "y" -or $testNgrok -eq "Y") {
        Write-Host "Testing ngrok..." -ForegroundColor Yellow
        & ngrok version
    }
} else {
    Write-Host "Installation Failed!" -ForegroundColor Red
    Write-Host "Please try manual installation:" -ForegroundColor Yellow
    Write-Host "1. Download ngrok from https://ngrok.com/download" -ForegroundColor White
    Write-Host "2. Extract to C:\ngrok" -ForegroundColor White
    Write-Host "3. Add C:\ngrok to PATH" -ForegroundColor White
}

Write-Host ""
Write-Host "Script completed!" -ForegroundColor Green
