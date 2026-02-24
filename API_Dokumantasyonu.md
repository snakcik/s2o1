# S2O1 API Dokümantasyonu

Bu belge, S2O1 uygulamasının Docker üzerinde ayağa kaldırıldığında dış sistemlerden (mobil uygulamalar, entegrasyonlar, başka web siteleri vs.) erişilebilecek temel API uç noktalarını (endpoints) açıklamaktadır.

Docker yapılandırmasında port **5267** dışarıya açılmıştır ve güvenlik açısından `Cors` yapılandırması dışarıdan gelen isteklere yanıt verecek şekilde ayarlanmıştır. Sunucunuzun IP adresi üzerinden `http://<sunucu_ip>:5267/api/...` şeklinde çağrılar yapabilirsiniz.

## 🔑 Kimlik Doğrulama (Auth)

API'yi kullanabilmek için öncelikle sisteme giriş yapmalısınız (token veya oturum bilgisi almak için).

- `POST /api/Auth/login` : Kullanıcı girişi yapar. (Gövde: `username`, `password`)
- `POST /api/Auth/register` : Yeni kullanıcı kaydeder.
- `POST /api/Auth/role` : Kullanıcıya rol atar.

## 👤 Kullanıcı ve Kullanıcı Yönetimi (Users)

- `GET /api/Users` : Sistemdeki kullanıcıların listesini getirir.
- `GET /api/Users/{userId}` : Belirli bir kullanıcının bilgilerini getirir.
- `PUT /api/Users/{userId}` : Kullanıcı bilgilerini günceller.
- `DELETE /api/Users/{userId}` : Kullanıcıyı siler.
- `POST /api/Users/{userId}/permissions` : Kullanıcı izinlerini günceller / tanımlar.
- `POST /api/Users/{userId}/change-password` : Şifre değiştirme işlemi.

## 📦 Ürün ve Stok Yönetimi (Product & Stock)

- `GET /api/Product` : Tüm ürünleri listeler.
- `GET /api/Product/{id}` : Tek bir ürünün detayını getirir.
- `POST /api/Product` : Yeni bir ürün oluşturur.
- `PUT /api/Product` : Mevcut bir ürünü günceller.
- `DELETE /api/Product/{id}` : Seçili ürünü sistemden siler.
- `GET /api/Product/categories` : Ürün kategorilerini listeler.
- `GET /api/Product/brands` : Ürün markalarını listeler.

### Stok Hareketleri
- `POST /api/Stock/movement` : Yeni bir stok hareketi (girdi/çıktı) kaydeder.
- `GET /api/Stock/report` : Stok raporlarını getirir.
- `GET /api/Stock/product/{productId}/warehouse/{warehouseId}` : Belirli depodaki ürünün stok bilgisini getirir.

## 🏭 Depo Yönetimi (Warehouse)

- `GET /api/Warehouse` : Depoların listesini alır.
- `POST /api/Warehouse` : Yeni depo ekler.
- `GET /api/Warehouse/{id}/shelves` : Depodaki rafları listeler.
- `POST /api/Warehouse/shelves` : Depoya yeni bir raf ekler.

## 🤝 Müşteri ve Tedarikçiler (Customer & Supplier)

- `GET /api/Customer` : Müşterileri listeler.
- `POST /api/Customer` : Yeni müşteri ekler.
- `GET /api/Supplier` : Tedarikçileri listeler.
- `POST /api/Supplier` : Yeni tedarikçi ekler.

## 💰 Teklifler & Faturalar (Offers & Invoices)

- `GET /api/Offer` / `OfferController`: Teklifleri listeler, oluşturur ve yönetir.
- `GET /api/Invoice` / `InvoicesController`: Faturaları listeler, oluşturur ve yönetir.
- `GET /api/PriceList` : Fiyat listelerini yönetir.

---

### Genel Kullanım Notları:
1. **İstek Yapısı:** Genelde veri yollarken `Content-Type: application/json` başlığıyla `JSON` gövdeler kullanılır.
2. **Kimlik Doğrulama:** Auth üzerinden dönülecek olan yetki bilgisinin, kimlik gerektiren uç noktalara istek yaparken Headers (Başlık) içerisinde gönderilmesi gerekmektedir.
3. **Swagger UI:** API'nin daha detaylı ve etkileşimli denemeleri için sistem `Development` modunda çalışırken `http://<sunucu_ip>:5267/swagger` adresine girebilir ve tüm endpoint'leri canlı döküman üstünde deneyebilirsiniz. 

---

## 🛠️ CLI (Komut Satırı Arayüzü) Kullanımı ve Development (Geliştirici) Modu Seçimi

Sistem Docker üzerinde çalışırken, konfigürasyonları yönetmek ve ayarları değiştirmek için CLI aracına container içerisinden erişebilirsiniz. Hiç bilmeyen bir geliştirici için test ortamını ve Swagger'ı aktif etme adımları aşağıda sırasıyla anlatılmıştır.

### 1. CLI'ye Giriş Yapmak
Sisteminizde `docker-compose up -d --build` komutuyla uygulamayı ayağa kaldırdıktan sonra, aynı klasörde veya komut satırında aşağıdaki komutu yazarak CLI arayüzüne bağlanın:

```bash
docker attach s2o1_cli
```

### 2. Development Modunu Nasıl Açarım? (Swagger'ı Aktif Etmek)
CLI menüsü açıldığında (menüyü görmek için önceden giriş yapmanız veya ayar yapmanız istenirse adımları izleyin), **Main Menu** (Ana Menü) karşınıza gelecektir.

Menüden şu adımları izleyin:
1. Klavyedeki **Aşağı/Yukarı ok tuşlarını** kullanarak menüde gezinin.
2. **`Deployment Environment (Dev/Prod)`** seçeneğinin üzerine gelip **Enter**'a basın.
3. Açılan alt menüden **`Set Environment: Development (Enables Swagger API Docs)`** seçeneğini seçin.
4. "Environment set to Development" şeklinde başarılı olduğuna dair yeşil bir mesaj göreceksiniz. Ardından **Enter**'a basıp ana menüye dönebilirsiniz.

### 3. Değişikliklerin Geçerli Olması (API'yi Yeniden Başlatmak)
Değiştirdiğiniz bu modun algılanması ve Swagger'ın aktif olması için API konteynerini yeniden başlatmalısınız. 
Açık olan CLI konsolundan çıkmak için **`Ctrl+P`** ve hemen ardından **`Ctrl+Q`** tuşlarına basın (Bu, CLI'yi kapatmadan CLI'den çıkış yapmanızı/detach etmenizi sağlar).

Sonrasında terminalinize / komut satırınıza şu komutu yazarak sadece API'yi yeniden başlatın:
```bash
docker restart s2o1_api
```

### 4. Swagger ile Test Etmek
API yeniden başladıktan sonra, kullandığınız makinenin IP adresi (veya localhost) üzerinden tarayıcınızda şu adrese gidin:
```
http://<sunucu_ip>:5267/swagger
```
Artık Swagger arayüzünü görebilir, tüm servisleri ve JSON gövdelerini döküman üstünde deneyebilirsiniz!
