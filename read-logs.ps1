# PowerShell script để đọc và phân tích logs SePay
param(
    [string]$LogDate = (Get-Date -Format "yyyyMMdd"),
    [int]$TailLines = 50,
    [switch]$Follow = $false,
    [switch]$Errors = $false,
    [switch]$Warnings = $false,
    [switch]$Info = $false,
    [switch]$All = $false
)

Write-Host "📋 SePay Log Reader" -ForegroundColor Green
Write-Host "==================" -ForegroundColor Green
Write-Host ""

# Function để tìm file log
function Get-LogFile {
    param([string]$Date)
    
    $logFile = "CareNest_SePay/logs/sepay-$Date.txt"
    if (Test-Path $logFile) {
        return $logFile
    }
    
    # Tìm file log gần nhất
    $logDir = "CareNest_SePay/logs"
    if (Test-Path $logDir) {
        $latestLog = Get-ChildItem -Path $logDir -Filter "sepay-*.txt" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
        if ($latestLog) {
            Write-Host "⚠️  Log file for $Date not found. Using latest log: $($latestLog.Name)" -ForegroundColor Yellow
            return $latestLog.FullName
        }
    }
    
    return $null
}

# Function để phân tích log level
function Get-LogLevel {
    param([string]$Line)
    
    if ($Line -match '\[ERR\]') { return "ERROR" }
    if ($Line -match '\[WRN\]') { return "WARNING" }
    if ($Line -match '\[INF\]') { return "INFO" }
    if ($Line -match '\[DBG\]') { return "DEBUG" }
    return "UNKNOWN"
}

# Function để format log line
function Format-LogLine {
    param([string]$Line)
    
    $level = Get-LogLevel -Line $Line
    
    switch ($level) {
        "ERROR" { 
            $color = "Red"
            $icon = "❌"
        }
        "WARNING" { 
            $color = "Yellow"
            $icon = "⚠️"
        }
        "INFO" { 
            $color = "Green"
            $icon = "ℹ️"
        }
        "DEBUG" { 
            $color = "Cyan"
            $icon = "🔍"
        }
        default { 
            $color = "White"
            $icon = "📝"
        }
    }
    
    return @{
        Level = $level
        Color = $color
        Icon = $icon
        Line = $Line
    }
}

# Function để hiển thị log với màu sắc
function Show-LogLine {
    param([hashtable]$LogInfo)
    
    $timestamp = ""
    $message = $LogInfo.Line
    
    # Extract timestamp
    if ($LogInfo.Line -match '^(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3} \+\d{2}:\d{2})') {
        $timestamp = $matches[1]
        $message = $LogInfo.Line.Substring($matches[1].Length + 1)
    }
    
    Write-Host "$($LogInfo.Icon) " -NoNewline -ForegroundColor $LogInfo.Color
    Write-Host "[$($LogInfo.Level)] " -NoNewline -ForegroundColor $LogInfo.Color
    Write-Host "$timestamp " -NoNewline -ForegroundColor Gray
    Write-Host "$message" -ForegroundColor White
}

# Function để đếm log levels
function Count-LogLevels {
    param([string[]]$Lines)
    
    $counts = @{
        ERROR = 0
        WARNING = 0
        INFO = 0
        DEBUG = 0
        UNKNOWN = 0
    }
    
    foreach ($line in $Lines) {
        $level = Get-LogLevel -Line $line
        $counts[$level]++
    }
    
    return $counts
}

# Function để hiển thị summary
function Show-LogSummary {
    param([string[]]$Lines)
    
    $counts = Count-LogLevels -Lines $Lines
    
    Write-Host ""
    Write-Host "📊 Log Summary" -ForegroundColor Cyan
    Write-Host "==============" -ForegroundColor Cyan
    
    if ($counts.ERROR -gt 0) {
        Write-Host "❌ Errors: $($counts.ERROR)" -ForegroundColor Red
    }
    if ($counts.WARNING -gt 0) {
        Write-Host "⚠️  Warnings: $($counts.WARNING)" -ForegroundColor Yellow
    }
    if ($counts.INFO -gt 0) {
        Write-Host "ℹ️  Info: $($counts.INFO)" -ForegroundColor Green
    }
    if ($counts.DEBUG -gt 0) {
        Write-Host "🔍 Debug: $($counts.DEBUG)" -ForegroundColor Cyan
    }
    
    Write-Host "📝 Total: $($Lines.Count)" -ForegroundColor White
}

# Main execution
$logFile = Get-LogFile -Date $LogDate

if (-not $logFile) {
    Write-Host "❌ No log file found for date: $LogDate" -ForegroundColor Red
    Write-Host "Available log files:" -ForegroundColor Yellow
    
    $logDir = "CareNest_SePay/logs"
    if (Test-Path $logDir) {
        Get-ChildItem -Path $logDir -Filter "sepay-*.txt" | ForEach-Object {
            Write-Host "  - $($_.Name)" -ForegroundColor White
        }
    } else {
        Write-Host "  No logs directory found" -ForegroundColor Red
    }
    exit 1
}

Write-Host "📁 Reading log file: $logFile" -ForegroundColor Yellow
Write-Host "📅 Date: $LogDate" -ForegroundColor Yellow
Write-Host ""

# Đọc log file
try {
    if ($Follow) {
        Write-Host "🔄 Following log file (Press Ctrl+C to stop)..." -ForegroundColor Cyan
        Write-Host ""
        
        Get-Content -Path $logFile -Wait -Tail $TailLines | ForEach-Object {
            $logInfo = Format-LogLine -Line $_
            Show-LogLine -LogInfo $logInfo
        }
    } else {
        $lines = Get-Content -Path $logFile -Tail $TailLines
        
        if ($All) {
            # Hiển thị tất cả logs
            foreach ($line in $lines) {
                $logInfo = Format-LogLine -Line $line
                Show-LogLine -LogInfo $logInfo
            }
        } elseif ($Errors) {
            # Chỉ hiển thị errors
            $errorLines = $lines | Where-Object { $_ -match '\[ERR\]' }
            foreach ($line in $errorLines) {
                $logInfo = Format-LogLine -Line $line
                Show-LogLine -LogInfo $logInfo
            }
        } elseif ($Warnings) {
            # Chỉ hiển thị warnings
            $warningLines = $lines | Where-Object { $_ -match '\[WRN\]' }
            foreach ($line in $warningLines) {
                $logInfo = Format-LogLine -Line $line
                Show-LogLine -LogInfo $logInfo
            }
        } elseif ($Info) {
            # Chỉ hiển thị info
            $infoLines = $lines | Where-Object { $_ -match '\[INF\]' }
            foreach ($line in $infoLines) {
                $logInfo = Format-LogLine -Line $line
                Show-LogLine -LogInfo $logInfo
            }
        } else {
            # Hiển thị tất cả với summary
            foreach ($line in $lines) {
                $logInfo = Format-LogLine -Line $line
                Show-LogLine -LogInfo $logInfo
            }
        }
        
        Show-LogSummary -Lines $lines
    }
} catch {
    Write-Host "❌ Error reading log file: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "💡 Usage Examples:" -ForegroundColor Cyan
Write-Host ".\read-logs.ps1                    # Show last 50 lines" -ForegroundColor White
Write-Host ".\read-logs.ps1 -TailLines 100     # Show last 100 lines" -ForegroundColor White
Write-Host ".\read-logs.ps1 -Errors            # Show only errors" -ForegroundColor White
Write-Host ".\read-logs.ps1 -Warnings          # Show only warnings" -ForegroundColor White
Write-Host ".\read-logs.ps1 -Follow            # Follow log file" -ForegroundColor White
Write-Host ".\read-logs.ps1 -LogDate 20241010  # Show specific date" -ForegroundColor White
