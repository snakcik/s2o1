# İlerleme Kaydı ve Konuşma Geçmişi

### Kullanıcı Tercihleri
- **Dil:** Türkçe (Her zaman Türkçe cevap verilecek).
- **Not:** Bu dosya proje durumunu ve önemli kararları içerir.

## 2026-02-13 - Oturum Özeti

### Yapılan İşler
1.  **CLI Temizliği:** 
    - `UserManagementFlow.cs` içindeki gereksiz CRUD (Ekle/Sil/Düzenle) operasyonları kaldırıldı.
    - CLI artık sadece salt okunur listeleme ve sistem yönetimi modunda, güncel `UserDto` yapısıyla uyumlu çalışıyor.

2.  **Stok Modülü Geliştirmeleri (`StockService`):**
    - **Negatif Stok Kontrolü:** Stok çıkışı ve transfer işlemlerine `Company.AllowNegativeStock` kontrolü eklendi. İzin yoksa ve bakiye yetersizse işlem engelleniyor.
    - **Kritik Stok Uyarısı:** Her stok düşüşünden sonra ürünün `MinStockLevel` altına inip inmediği kontrol ediliyor ve `StockAlert.IsNotificationSent` güncelleniyor.

3.  **Teklif ve Fatura Sistemi (`OfferService` & `InvoiceService`):**
    - `InvoiceService` (Fatura Servisi) oluşturuldu ve DI container'a kaydedildi.
    - `OfferService` (Teklif Servisi) içindeki stok düşüm mantığı kaldırıldı (Teklif sadece statü değiştirir).
    - **Akış Kuruldu:** Teklif -> Fatura Oluştur (`CreateInvoiceFromOffer`) -> Fatura Onayla (`ApproveInvoice`).
    - Stok düşüm işlemi (Exit) artık **Fatura Onayı** aşamasında gerçekleşiyor.
    - DTO'lar (`InvoiceDto`) ve AutoMapper profilleri eksiksiz tanımlandı.
    - `InvoiceStatus` enum'ına `Approved` eklendi.

### Mevcut Durum
- **Backend:** Stok, Teklif ve Fatura sistemleri entegre çalışıyor. Proje hatasız derleniyor.
- **CLI:** Çalışır durumda.

### Sıradaki Adımlar (Potansiyel)
1.  **Bildirim Sistemi:** Kritik stok uyarılarının (`StockAlert`) gerçek bir bildirim servisine (E-posta/SMS/UI Bildirimi) bağlanması.
2.  **Raporlama:** Fatura ve stok hareket raporlarının API uç noktalarının (Controller) yazılması.
3.  **Test:** Teklif -> Fatura -> Stok Düşüm akışının API üzerinden test edilmesi.
