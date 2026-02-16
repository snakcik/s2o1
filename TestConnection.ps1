# S2O1 API Bağlantı Test Script'i
# Diğer bilgisayardan bu script'i çalıştırarak bağlantıyı test edebilirsiniz

param(
    [string]$ServerIP = "192.168.1.164",
    [int]$Port = 5267
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "S2O1 API Bağlantı Testi" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 1. Ping Testi
Write-Host "1. Ping Testi..." -ForegroundColor Yellow
try {
    $ping = Test-Connection -ComputerName $ServerIP -Count 2 -Quiet
    if ($ping) {
        Write-Host "   ✓ Sunucuya erişilebiliyor" -ForegroundColor Green
    }
    else {
        Write-Host "   ✗ Sunucuya erişilemiyor!" -ForegroundColor Red
        Write-Host "   → Ağ bağlantınızı kontrol edin" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "   ✗ Ping başarısız: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# 2. Port Testi
Write-Host "2. Port $Port Testi..." -ForegroundColor Yellow
try {
    $tcpClient = New-Object System.Net.Sockets.TcpClient
    $connect = $tcpClient.BeginConnect($ServerIP, $Port, $null, $null)
    $wait = $connect.AsyncWaitHandle.WaitOne(3000, $false)
    
    if ($wait) {
        try {
            $tcpClient.EndConnect($connect)
            Write-Host "   ✓ Port $Port açık ve erişilebilir" -ForegroundColor Green
            $tcpClient.Close()
        }
        catch {
            Write-Host "   ✗ Port bağlantısı reddedildi" -ForegroundColor Red
        }
    }
    else {
        Write-Host "   ✗ Port $Port kapalı veya erişilemiyor" -ForegroundColor Red
        Write-Host "   → Firewall kurallarını kontrol edin" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "   ✗ Port testi başarısız: $($_.Exception.Message)" -ForegroundColor Red
}
finally {
    if ($tcpClient) { $tcpClient.Close() }
}

Write-Host ""

# 3. HTTP İstek Testi
Write-Host "3. HTTP İstek Testi..." -ForegroundColor Yellow
$apiUrl = "http://${ServerIP}:${Port}/api/health"
try {
    $response = Invoke-WebRequest -Uri $apiUrl -Method GET -TimeoutSec 5 -ErrorAction Stop
    Write-Host "   ✓ API'ye başarıyla bağlanıldı!" -ForegroundColor Green
    Write-Host "   → Status Code: $($response.StatusCode)" -ForegroundColor White
}
catch {
    if ($_.Exception.Message -like "*404*") {
        Write-Host "   ⚠ API'ye bağlanıldı ama /api/health endpoint'i bulunamadı (404)" -ForegroundColor Yellow
        Write-Host "   → Bu normal olabilir, API çalışıyor" -ForegroundColor White
    }
    else {
        Write-Host "   ✗ HTTP isteği başarısız: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Test Tamamlandı" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "API Adresi: http://${ServerIP}:${Port}" -ForegroundColor White
Write-Host ""
Write-Host "Devam etmek için bir tuşa basın..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
