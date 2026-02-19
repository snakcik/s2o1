# S2O1 Geliştirme Günlüğü (Progress Log)

**Son Güncelleme:** 2026-02-18
**Durum:** Yetkilendirme Sistemi Tamamlandı & İyileştirildi

## Yapılan İşlemler

### 1. Yetkilendirme Altyapısı (Backend)
- `UserPermission` tablosuna `IsFull` kolonu eklendi.
- `PermissionAttribute` filtresi, `IsFull` yetkisine sahip kullanıcıların her işlemi yapabilmesini sağlayacak şekilde güncellendi.
- `AuthService` üzerinde `SaveUserPermissionsAsync` ve `GetUserPermissionsAsync` metodları `IsFull` özelliğini destekleyecek şekilde güncellendi.
- **Admin Otomatik Yetkilendirme:** Yeni bir "Admin" kullanıcısı oluşturulduğunda, tüm modüller için varsayılan olarak "Tam Yetki" (`IsFull=true`) verilmesi sağlandı.

### 2. Kullanıcı Arayüzü (Frontend)
- **Depo Yönetimi Eksikliği:** Sidebar menüsüne eksik olan "Depolar" seçeneği eklendi (`data-module="Warehouse"`). "ozan" kullanıcısının buton görememe sorunu bu eksiklikten kaynaklanıyordu.
- **Yetki Yönetim Paneli:** Kullanıcı yetkileri tablosuna **"Tam Yetki"** kolonu eklendi.
- **Checkbox Mantığı:** "Tam Yetki" işaretlendiğinde o satırdaki Okuma/Yazma/Silme kutucukları otomatik olarak işaretlenmektedir.
- **Görünürlük Kontrolü:** `app.js` içerisindeki `applyPermissions` ve `switchView` fonksiyonları `IsFull` yetkisini de kontrol edecek şekilde güncellendi.
- **Buton Seviyesi Yetkilendirme:** Sayfa içerisindeki "Yeni Ekle", "Düzenle" ve "Sil" gibi aksiyon butonları için `data-permission="Module:Type"` altyapısı kuruldu. `applyPermissions` fonksiyonu bu butonları yetkiye göre otomatik gizliyor.
- **Hata Yönetimi ve Kısıtlamalar:**
  - Tüm major veri yükleme fonksiyonlarına (`loadProducts`, `loadSuppliers`, `loadCompanies`, `loadCustomers`, `loadPriceLists`, `loadOffers`, `loadInvoices`) 403 (Yetki Yok) hatası yakalama eklendi. Yetkisi olmayan kullanıcılar için tabloda "🚫 Yetkisiz Erişim" mesajı gösterilmektedir.
  - Tablo içerisindeki "Düzenle", "Sil", "Onayla" gibi işlem butonlarının görünürlüğü, kullanıcının ilgili modüldeki ("Write", "Delete") yetkisine göre kontrol edilmektedir.
  - **Stok Girişi Menüsü:** `data-permission="Stock:Write"` attribute'u eklenerek sadece yazma yetkisi olanların görmesi sağlandı.
  - **Kullanıcı Yönetimi:** Kullanıcı listeleme ve düzenleme işlemlerine "Write" ve "Delete" yetki kontrolü eklendi. "User" rolündeki kullanıcıların bu sayfayı görmesi client-side engellendi.
  - **Basit Varlıklar (Marka/Kategori/Birim):** `loadSimple` fonksiyonu ile yönetilen bu alanlara 403 kontrolü ve buton gizleme eklendi.
  - **Raporlar ve Stok Girişi:** `loadStockReport`, `loadStockEntry` ve `submitStockEntry` fonksiyonlarına yetki kontrolleri eklendi.

### 3. Veritabanı Değişiklikleri
- `AddIsFullToPermissions` migrasyonu oluşturuldu ve uygulandı.

## Mevcut Durum & Kalan İşler
- [x] Tüm Controller'lara `PermissionAttribute` eklendi.
- [x] "Full" yetki seçeneği eklendi.
- [x] Admin kullanıcıları varsayılan olarak tam yetkili geliyor.
- [x] Belirli sayfalarda (Ürün Düzenleme gibi) buton bazlı ("Düzenle", "Sil" butonlarının gizlenmesi) yetki kontrolü yapılıyor.
- [ ] Yeni eklenecek modüllerde aynı yapının (403 kontrolü ve buton gizleme) uygulanması gerekecektir.

## Notlar
- `root` kullanıcısı (ID: 1) tüm kontrollerden muaftır.
- `Admin` rolü silinemez veya `root` tarafından değiştirilemez kuralları korunmaktadır.
