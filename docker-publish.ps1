$DOCKER_USER = Read-Host "Docker Hub Kullanıcı Adınızı Girin"

Write-Host "Uygulama Docker imajı oluşturuluyor..." -ForegroundColor Cyan
docker build -t "$DOCKER_USER/s2o1:latest" .

if ($LASTEXITCODE -eq 0) {
    Write-Host "Docker Hub'a giriş yapmanız gerekebilir..." -ForegroundColor Yellow
    docker login
    
    Write-Host "İmaj gönderiliyor (Push)..." -ForegroundColor Cyan
    docker push "$DOCKER_USER/s2o1:latest"
    
    Write-Host "İşlem tamamlandı! Docker Hub üzerinden imajınızı kontrol edebilirsiniz." -ForegroundColor Success
}
else {
    Write-Host "Build işlemi başarısız oldu!" -ForegroundColor Red
}
