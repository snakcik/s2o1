# S2O1 API Firewall Rule Ekleme Script'i
# Bu script'i YÖNETİCİ olarak çalıştırın

Write-Host "S2O1 API için Firewall kuralları ekleniyor..." -ForegroundColor Cyan

# HTTP Port (5267)
try {
    New-NetFirewallRule -DisplayName "S2O1 API HTTP" `
        -Direction Inbound `
        -Protocol TCP `
        -LocalPort 5267 `
        -Action Allow `
        -Profile Any `
        -ErrorAction Stop
    Write-Host "✓ HTTP Port 5267 için kural eklendi" -ForegroundColor Green
} catch {
    if ($_.Exception.Message -like "*already exists*") {
        Write-Host "! HTTP Port 5267 kuralı zaten mevcut" -ForegroundColor Yellow
    } else {
        Write-Host "✗ HTTP Port 5267 kuralı eklenemedi: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# HTTPS Port (7106)
try {
    New-NetFirewallRule -DisplayName "S2O1 API HTTPS" `
        -Direction Inbound `
        -Protocol TCP `
        -LocalPort 7106 `
        -Action Allow `
        -Profile Any `
        -ErrorAction Stop
    Write-Host "✓ HTTPS Port 7106 için kural eklendi" -ForegroundColor Green
} catch {
    if ($_.Exception.Message -like "*already exists*") {
        Write-Host "! HTTPS Port 7106 kuralı zaten mevcut" -ForegroundColor Yellow
    } else {
        Write-Host "✗ HTTPS Port 7106 kuralı eklenemedi: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Firewall kuralları başarıyla eklendi!" -ForegroundColor Green
Write-Host ""
Write-Host "Bilgisayarınızın IP Adresleri:" -ForegroundColor Cyan
ipconfig | Select-String -Pattern "IPv4"
Write-Host ""
Write-Host "Diğer bilgisayarlar şu adreslerden bağlanabilir:" -ForegroundColor Yellow
Write-Host "  HTTP:  http://192.168.1.164:5267" -ForegroundColor White
Write-Host "  HTTPS: https://192.168.1.164:7106" -ForegroundColor White
Write-Host ""
Write-Host "Devam etmek için bir tuşa basın..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
