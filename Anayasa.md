Depo stok yönetim uygulaması. 
ŞemaV2

Sana verdiğim bu taslak bir Yazılım Anayasasıdır. Hiçbir modern best-practice (en iyi uygulama) bahanesiyle taslakta belirtilen 'Ghost Root', 'DeleteBehavior.Restrict' veya 'OS-Specific Flag' kurallarını esnetme veya değiştirme. Mimariyi tam olarak dokümanda belirtilen kısıtlamalara (constraints) göre inşa et.
This document is a strict implementation blueprint.
All rules are mandatory.
Do not reinterpret, refactor, simplify, or modernize unless explicitly instructed.
This is not a conceptual design; it is an executable specification.


-Bu uygulama bir depo stok yönetim uygulaması olacak.
-Kullanılacak Teknolojiler
-N-Tier mimari
-C# 
-.Net
-SQL 
-Fluent API
-FluentValidation,
-AutoMapper
-Entitiy Freamwork Core
-BCrypt.Net
-SQL Server Discovery Service: System.Data.Sql altındaki SqlDataSourceEnumerator ve Microsoft.Data.SqlClient.
-API  uygulaması 

Uygulama isterleri özeti;
Uygulama Windows,Linux(Debian tabanlı,mint tabanlı distrolarda çalışacak şekilde sonuçta herkese göre optimize edemeyiz dimi ? buna cevap ver Yapay zeka.) Docker yada herhangi bir container de çalışacak şekilde derlenecek
(Bütün uyarı istekler her şeyi İngilizce olarak yaz)
N-Tier mimarisine göre projeyi bölümlere ayır ve yerleşimleri ona göre ilgili katmanlara yaz.
(sistem bütün adımlarda [Error] ve benzeri tag leri kullanarak mesajları CLI da gösterir)
CLI menüsünde seçimler ok tuşları yönerkeleri ile yapılacak bu sayede menüye yeni bir seçenek eklendiğinde araya sokulabilecek. ayrıca yanlış tuşlama için her seferinde kontrol mekanizması kurmaya gerek kalmayacak. Her menünün altına Escape yada exit seçeneği koyup bir üst menüye dönme seçeneği sunulsun
Dockerfile veya Compose dosyasında installation.flag dosya yolu için ilgili volume'un zorunlu olduğu dökümana eklenmeli.


LogoAsci default value:
██████╗ ███████╗ ██╗ ██████╗ 
╚════██╗██╔════╝███║██╔═══██╗
 █████╔╝███████╗╚██║██║   ██║
██╔═══╝ ╚════██║ ██║██║   ██║
███████╗███████║ ██║╚██████╔╝
╚══════╝╚══════╝ ╚═╝ ╚═════╝
CLI Markalama ve Görsel Kimlik 
CLI Logosu Yazdırma: Logoyu komut satırı arayüzünde (CLI) yazdırırken, logo için Console.ForegroundColor = ConsoleColor.Cyan; , logonun hemen altındaki metinler için ise ConsoleColor.White; rengini kullanın. 
Logo Tasarımı: Logoyu oluştururken aşağıda belirtilen "Block Style" (Blok Stil) ASCII sanatını kullanın. 
Renklendirme Kuralları: Terminal ekranında logonun görünürlüğü için Console.ForegroundColor özelliği kullanılarak logo Cyan (Turkuaz) veya Turuncu renklerinde bastırılmalıdır.
Welcome Text: Under the logo, print: 2S1O - Warehouse Management System [v1.0.0] in White.


Program Başlar.

Uygulama açıldığında:
aynı uygulama açık mı açıksa: uygulama zaten çalışıyor uyarı mesajını göster ve kapan.
OS detection yapar,
işletim sistemi özelinde Windows ise Run as admin yada unix ise run as sudo benzeri(litaratüre uygun bir uyarı ) mesajı ver ve uygulamayı kapat.
Uygula
Uygulamanın Admin/sudo yetkisi ile açılıp açılmadığınıma halihazırda açık değilse adımlara devam et. kontrol et
Network bilgileri işletim sistemine göre uygun komutlar le kontrol edilir, uygulamaya erişilebilecek IP tespit edilir.
installation.flag yada registay i kontrol eder ve daha önce kurulmuş mu kurulmamış mı kontrol eder

Eğer uygulama sistemde hali hazırda kurulmuş ise,

Veri tabanı kontrolü:
sistem ilk önce bir SQL sunucusuna bağlı olup olmadığını ve bağlantı mevcutsa şemaların yerinde olup olmadığını kontrol eder, eğer bir SQL sunucusuna bağlıysa ve şemalarda sorun yok ise. konsol log göster:"Database Connection Active, 
Lisans Kontrolü:
Valid:Veri tabanında geçerli lisans bilgileri var ise uygulama artık API isteklerine hazır bir şekilde ayağa kalkar. Kalan istekler için 403 döner( veritaban için gerekli entitiyler aşağıda Entitiyler başlığı ile verilecektir. migration işlemlerini oradaki notlara ve komutlara göre tamamla)
invalid: Uygulama yine çalışır ancak sadece lisans ile ilgili API isteklerine cevap verecek servisler çalışır.
Sett

Uygulama kurulu ise CLI açtığımda ve S2O1 yazdığımda benden kullanıcı adı ve şifre isteyecek ve kesinlikle hiçbir yere uygulama bilgileri hariç Uygulama logosu Welcome ekranı çıkacak
S2O1 info: komutunda Veri tabanı bağlantı durumu, aktif Master/Slave durumu 
S2O1 info -all : login ister ve sadece root kullanıcısı giriş yaptıktan sonda görülür(root kullanıcı bilgileri aşağıdaki satırlarda verilecektir.) komutu Veritabanı bilgileri (şifreler hariç),Veri tabanı bağlantı durumu, aktif Master/Slave durumu , sunucunun çalıştığı IP: uygulamaya erişilecek port bilgisi. Uygulama konteynerde çalışıyorsa konteyner bilgileri.

Eğer uygulama ilk kez çalışıyor ise: Aşağıdaki menü gelecek

 ().Program Installation
 ().CreateDatabse
 ().ConnectDatabse
 
Program Installation:
windows veya Linux ise program dosyalarını ilgili klasöre taşı örn: PrgramFiles(x64)/s2o1 gibi ama containnerde ise bu menünün anlamı yok zaten menüyü gösterme.
program dosyaları yükleme sırasın kullanılan komutlar [Success(yeşil)] [Warning(sarı)] [Error(kırmızı)] gibi tag ler ile CLI da gözüksün.
dosya kopyalama işleri tamamen bittikten sonra 
işletim sistemine bağlı olarak
Environment.SetEnvironmentVariable ile uygulamanın yolunu System PATH'e ekle 
yada /usr/local/bin/ altında uygulamanın çalıştırılabilir dosyasına bir Symbolic Link (ln -s) oluşturmalısın.
uygulama windows ise Registery nin içine aşağıdaki bilgileri yaz
HKEY_LOCAL_MACHINE\Software\2S1O\Installed = true
HKEY_LOCAL_MACHINE\Software\2S1O\InstallDate = 2026-02-05 
hata alırsan hatayı bildir kuruluma devam et

Linux ise
/etc/2s1o/installed.flag dosyasını oluştur ve içine sistemin kurulu olduğunu ve kurulduğu tarihi teyid edecek bilgileri yaz,hata alırsan hatayı bildir kuruluma devam et
Containner de ise 
Host’tan /var/lib/2s1o/installed.flag gibi bir dosya mount edilir. 
Bu sayede uygulama ilk kurulum mu yoksa daha önce kurulmuş mu olduğunu işletim sistemi bazında anlayabilir.

Create Database:
Veritabanını locale kuracak şekilde ayarla
UserId:sa
DatabaseName:2S1O
DatabasePass:WSS2s1O_4-root
bilgilerini kullanarak connection stringi oluştur ve veritabanına bağlan konsole log göster "bağlantı kontrol ediliyor". bağlantı oluşturduktan sonra konsolda log göster "bağlantı başarılı veritabanı oluşturuluyor" migration başlat ve veritabanı işlemlerini tamamla hata vermez ise "Database Created Succes, Application is Starting"
Hata vermesi durumunda konsolda hata mesajını göster ve uygulama başlangıç menüsüne dönsün
CLI loglarında,uayrılarında Databasepass kesinlikle görünmeyecek **** şekilde olacak.

Veritabanı ilk oluşturma sırasında oluşturulacak her seferinde kontrol edilmeyecek bilgiler,
RoleName: "root","Admin","User"
Yeni User oluştur; RoleId:RoleName root olan kullanıcının ID si kullanılacak, UserName:root,şifre Q1w2e3r4-, UserName:root UserLastName:Root UserMail:root@root.com UserRegNo:123456789
Root kullanıcısı oluşturulurken sadece root kullanıcısı için bütün bilgiler (,UserName,LastName,Mail,RegNo BCrypt.Net) ile hashlenerek kaydedilmelidir.
Not:Root girişi sırasında veritabanından ID:1 olan kaydı çek. Kullanıcının girdiği düz metin 'UserName' bilgisini, BCrypt.Verify metodu ile veritabanındaki hashlenmiş UserName alanı üzerinden doğrula. Sakın UserName üzerinden SQL sorgusu atma.

Veritabanı tabloları oluştuktan hemen sonra Reflection ile `Modules` tablosunu doldur ve, **Root kullanıcısına bu modüller için tam yetki (Full Permission) tanımlayan** bir döngüyü de algoritmana ekle.
Modül tarama ve yetki atama adımları tek bir Transaction bloğunda yapılmalıdır. Eğer yetki atama aşamasında bir hata oluşursa veya elektrik kesilirse, Modules tablosuna yapılan ekleme de geri alınmalıdır.
Upsert kontrolü sadece Modules tablosuna bakmamalı; eğer modül varsa bile Root kullanıcısının (ID:1) bu modül için yetki satırı olup olmadığını kontrol etmeli, eksikse tamamlamalıdır.
"Reflection ile sınıfları tara ve Modules tablosunda olmayanları içeri aktar
Not: Yetki bağımlılık kontrolleri, yetki atama ve güncelleme işlemleri sırasında uygulanır.
Modül senkronizasyonu sırasında ModuleName bazlı kontrol yapılmalı; eğer modül tabloda varsa işlem pas geçilmeli, yoksa yeni kayıt oluşturulmalıdır. Root yetkileri tanımlanırken Update-or-Insert (Upsert) mantığı kullanılmalı, böylece mevcut Root yetkileri bozulmadan sadece yeni eklenen modüller için yetki satırları eklenmelidir.

2.ConnectDatabse;
Start Discovery: SqlDataSourceEnumerator.Instance.GetDataSources() metodunu çağır.
Filter & Map: Gelen verideki ServerName, InstanceName, IsLocal ve Version bilgilerini al.
Display: * Eğer InstanceName boşsa (Default instance), sadece ServerName göster.
Değilse ServerName\InstanceName formatında listele.
CLI Seçim Ekranı: Kullanıcıya veritabanı isimlerini yazması için boş bir satır bırakmak yerine, Ok Tuşları (Up/Down) ile gezinebileceği bir menü sunulmalıdır.
Instance için kullanıcı adı ve şifreyi istesin
User:(kullanıcı adı istenir)
Password:(Kullanıcı şifre istenir)
özet:
Sunucu Seçimi: Önce sunucu/instance seçilir.
Kimlik Doğrulama (Credentials): Sunucu seçildikten hemen sonra sistem sorar: "Bu sunucuya hangi kullanıcı ile bağlanacaksınız?"
User: (Kullanıcı adı istenir)
Password: (Şifre istenir - Maskelenmiş/Gizli şekilde)
Veritabanı Listeleme: Girilen bilgilerle sunucuya bir "Master" bağlantısı kurulur ve içindeki veritabanları çekilir.
Veritabanı Seçimi: Kullanıcı listelenen DB'lerden birini seçer ve Enter'a basar.
Teknik Not:Bağlantı Havuzu ve Bellek Yönetimi (Technical Note): "Master veritabanına bağlanıp veritabanı listesini çektikten sonra, bu geçici bağlantı (Connection) mutlaka using blokları içinde kapatılmalı ve bellekten temizlenmelidir. Kullanıcı nihai veritabanını seçtiğinde, yeni ve kalıcı bağlantı dizesi ile temiz bir oturum açılmalıdır."

Veritabanı seçildi kontrol mekanizması:
	Kullanıcı seçilen instance için kullanıcı adı şifre doğru ise ; konsolda mesaj göster "Database is being checked" :
veritabanınada tablolar var mı 
Yoksa: konsolda log göster "Tables not found, tables are being created."
tablolar yoksa yeni bir migration oluşturulacak bu sayede tablolar oluşturulmuş veri tabanı hazır olacak konsolda log göster (The database was created with new tables.);
Varsa:
	__EFMigrationsHistory kontrol et ve MigrationId yi kontrol et senin kodundaki son Migration ID'si ile SQL'deki uyuşmuyorsa "Güncelleme gerekli" uyarısını tetikle ve "uyuşmuyorsa" adımına git. Eğer mevcuttaki veri tabanından daha eski bir MigrationId var ise Uyarı ver: Daha eski bir versiyon veri tabanı kullanılıyor devam edilsin mi ? navigasyon tuşları kuralını burada uygula ()Evet ()Hayır
evet ise işlemlere devam et 
Hayır ise işlem yapma veri tabanı olduğu gibi kalsın bir üst menüye dön. 
Not: Migration karşılaştırması, EF Core MigrationId sıralamasına göre yapılır.
uyuşuyorsa: sistem artık bu veri tabanını kullanacak (konsolda log göster Bağlantı başarılı.)
uyuşmuyorsa: uyarı çıkacak: veri tabanı güncellenmesi sonrası veri kaybı oluşabilir devam etmek istiyor musunuz ? navigasyon tuşları kuralını burada uygula ()Evet ()Hayır
"evet"`context.Database.Migrate()` komutunu çalıştır(kosolda log göster: veritabanı güncelleniyor). Hata verirse veri tabanını temizle ve tabloları vs oluştur.(Veritabanı yeniden oluşturuluyor.)
"hayır"seçeneği seçildiğinde en başa dön.
Database Integrity & Versioning Rules:
Tablo Kontrolü: sys.tables sorgusuyla tabloların varlığı kontrol edilir. 
Eğer __EFMigrationsHistory tablosu bile yoksa, sistem "Sıfır Kurulum" (Initial Migration) moduna geçer.

Versiyon Karşılaştırma Algoritması:
LocalMigrationID (Kodun içindeki son ID)
RemoteMigrationID (Veritabanındaki son ID)
Durum 1: Local == Remote -> Log: "System is up to date. Connection successful."
Durum 2: Local > Remote -> Uyarı: "Database update required. Risk of data loss. Continue? (e/h)". Navigasyon tuşları kuralını burada uygula ()Evet ()Hayır

(Evet) Seçilirse:
İşlem Başlatılır: context.Database.Migrate() komutu tetiklenir.
Konsol Log: [INFO] Migration process started. Applying missing schemas...
Başarı Durumu: İşlem hatasız biterse: [SUCCESS] Database updated to the latest version. Proceeding to application startup.
Sonraki Adım: Uygulama, dökümanındaki "Reflection ile Modül ve Root Kullanıcı Kontrolü" adımına geçer.

(Hayır) Seçilirse:
İşlem Durdurulur: Veritabanı üzerinde hiçbir yapısal (DDL) değişiklik yapılmaz.
Konsol Log: [WARNING] Migration cancelled by the user. Application cannot start with an outdated database schema.
Geri Dönüş: Sistem otomatik olarak en başa, yani "1. Create Database / 2. Connect Database" seçim menüsüne geri döner. (Bu sayede kullanıcı yanlış veritabanını seçtiyse başka bir tanesine bağlanma şansı bulur).

Durum 3: Local < Remote -> Kritik Uyarı: "Your application version is outdated for this database. Please update the application."
Kullanıcıya seçenek sun:
()Exit Application
() Return to Main Menu (Create/Connect/Edit Database)
Seçim 'Exit Application' ise: Uygulama durdurulur.
Seçim 'Return to Main Menu' ise: Sistem otomatik olarak başlangıç menüsüne geri döner.
Migration Fail-Safe: Database.Migrate() işlemi sırasında bir Exception alınırsa:Hata loglanır.
Kullanıcıya "Update failed. Would you like to RECREATE the database? (Warning: All data will be lost!)" sorusu sorulur. Sadece e cevabı gelirse veritabanı temizlenip baştan kurulur.


Edit Database:
Bu menü sistemde bir veritabanına bağlanmış ancak daha sonrasında herhangi bir bağlantı sorunu yaşanıyor.
Seçili olan veritabanı bilgilerini göster
seçtiğim veriyi editle menüsü çıksın
Ip:
UserName:
Pass:
veritabanına bağlanmak için gereklibaşka bir ayar varsa buradan düzeltebilsin 
ayarları girdikten sonra en sonra test seçeneği çıksın
test başarılı ise "Connection Successfull" mesajı versin ve
test başarılı değil ise " ConnectionFailure { varsa geri dönen hata mesajı}


Not: Root kullanıcı, lisans doğrulama mekanizmasından muaftır.Lisans sınırı kontrol edilirken root kullanıcısına bakılmaz yani root lisans sınırına dahil değildir  Gerekli durumlarda Root yetkisi ile CLI üzerinden lisans kontrolü geçici olarak devre dışı bırakılabilir. Bu durum istisnai ve kontrollü bir yönetimsel işlem olarak kabul edilir.
root isterse CLI kullanarak lisans kontrolünü bypass edebilir(çünki uygulama slave modundaysa zaten master bir uygulamaya bağlı ve o uygulamanın kendi lisans paketleri olacak.)

Entitiyler,

Bu entitiyleri oluştururken OOP den faydalan olabildiğince (gerekli gördüğün yerleri kısaltmak için miras alma kullanabilirsin)
Tüm forein keys, gerekli yerlerde Fluent API kullanılarak DeleteBehavior.Restrict ile yapılandırılmalıdır.
Navigation property leri eklemeyi unutma
DataAccess katmanının Business (Logic) katmanından kesinlikle izole olması gerek
Tüm veritabanı işlemleri IRepository desenini takip etmeli, Business katmanı SQL veya EF Core kütüphanelerini referans almamalıdır.
Stok hareketleri, faturalar ve teklifler arasındaki tüm işlemler Unit-of-Work tasarımıyla yönetilmelidir. Eğer bir fatura kaydedilirken stok hareketi tablosuna yazma hatası alınırsa, faturayı da iptal et (Rollback). Bağımsız INSERT komutları kullanma.


(Abstract)
BaseEntitiy;
	
	Id;(int)	
	CreateDate = DateTimeNow;(DateTime)	
	IsDeleted { get; set; } = false;(Bool)	
	IsActive = True;(bool)	
	UpdatedByUserId(int?)

SystemSetting : BaseEntity
	SettingKey: (string) -> "CLI_Welcome_Message"
	SettingValue: (string) -> "2S1O Depo Stok Yönetim Uygulamasıdır. Tüm Hakları Saklıdır."
	LogoAscii: (string) -> Logonun ASCII karakter hali.
	AppVersion: (string) -> "v1.0.0"

LicenseInfo : BaseEntity
	LicenseKey: (string)
	LicenseType: (Enum) Demo, Basic, Expansion (10/50/100).
	UserLimit: (int) Bu lisansın izin verdiği maksimum aktif kullanıcı sayısı.
	ExpirationDate: (DateTime?) Demo lisanslar için bitiş tarihi.
	IsBypassed: (bool) Root tarafından lisansın pasif edilip edilmediği bilgisi (Default: false).
	LastCheckDate: (DateTime) Sistem saati manipülasyonunu engellemek için kaydedilen son çalışma zamanı.


Role:BaseEntitiy;
	RoleName


Module : BaseEntity 
    public string ModuleName 

UserPermission : BaseEntity 
    public int UserId 
    public int ModuleId
    public bool CanRead
    public bool CanWrite 
    public bool CanDelete


User:BaseEntitiy;
	RoleId(Role den tablosundan çekecek)	
	CreatedByUserId,
	UserName,(Zorunlu,string, max value 255 min value 2)
	UserPassword,(Zorunlu,string,max value 255 min value 6)
	UserMail,(Zorunlu,string,max value 255 min value 6) 
	UserLastName,(zorunlu)
	UserRegNo,(zorunlu)
	CompanyId,(zorunlu değil)
	TitleId,(zorunlu değil)
	UserPicture,
Not: Kullanıcı şifrelerini veri tabanına kayıt ederken BCrypt.Net kütüphanesi kullanarak şifreler Hasch lenecek.
Brute-force saldırılarını önlemek için User entity’sine AccessFailedCount ve LockoutEnd alanlarını ekle.
String girişlerde HTML tag lerini temizlemek veya engellemek için global bir doğrulama filtresi uygula.

UserApiKey:BaseEntitiy;
	UserId: (int) Hangi kullanıcıya ait?
	KeyName: (string) Bu key ne için?
	ApiKey: (string, Unique) Rastgele üretilmiş güvenli karakter dizisi (GUID veya 64 karakterli token).
	SecretKey: (string)
	ExpiresDate: (DateTime?) varsayılan olarak 1 yıl sonrası

Company:BaseEntitiy;
	CompanyName


Title:BaseEntitiy;
	TitleName
	CompanyId
	
CustomerCompany:BaseEntitiy;
	CustomerCompanyName,
	CustomerCompanyAddress,
	CustoemrCompanyMail,


Customer:BaseEntitiy;
	CustomerCompanyId
	CustomerContactPersonName
	CustomerContactPersonLastName,
	CustomerContactPersonMobilPhone,
	CustomerContactPersonMail,


Brand:BaseEntitiy;
	BrandName: (string, Zorunlu, max 50-100)
	BrandDescription: (string, Opsiyonel)
	BrandLogo: (string/byte[], Opsiyonel)


Category:BaseEntitiy;
	CategoryName
	CategoryDescription:
	ParentCategoryId
One-to-Many: Bir kategorinin altında birden fazla Product (Ürün) olabilir.
Self-Join: Bir kategorinin altında birden fazla alt kategori olabilir (SubCategories collection).

ProductUnit:BaseEntitiy;
	Bu entitiy yi oluştururken Adet i oluştur ve Adet birimini varsayılan olarak ayarla
	UnitName; Defoult Unit
	UnitShortName;
	IsDecimal;(bool)

Warehouse:BaseEntitiy;
	WarehouseName:(string, Zorunlu)
	Location: (string, Opsiyonel)
	CompanyId: (int, Zorunlu)

ProductLocation:BaseEntitiy;
	WarehouseId: (int, Zorunlu)
	LocationCode: (string, Zorunlu)
	LocationDescription: (string, Opsiyonel)


Supplier:BaseEntitiy;
	SupplierCompanyName: (string, Zorunlu)
	SupplierContactName: (string, Zorunlu)
	SupplierContactMail;
	SupplierAdress:

StockMovement:BaseEntitiy;
	ProductId: (int, Zorunlu)
	WarehouseId: (int, Zorunlu)
	MovementType: (Enum/byte, Zorunlu) Hareketin yönü: 1: Entry (Giriş), 2: Exit (Çıkış), 3: Return (İade), 4: Transfer.
	Quantity: (decimal, Zorunlu)
	MovementDate: (DateTime) İşlemin yapıldığı an (Genelde CreateDate ile aynıdır ama geriye dönük işlem yapılabilmesi için ayrı tut)
	DocumentNo: (string, Opsiyonel)
	Description: (string, Opsiyonel)
	UserId: (int)
	SupplierId:(int) Supplier tablosuna bağlı.
	CustomerId: (int, Opsiyonel/Nullable) -> Customer tablosuna bağlı.

Invoice : BaseEntity
	InvoiceNumber: (string)
	OfferId: (int) Hangi tekliften dönüştü?
	SellerCompanyId: (int) Faturayı kesen bizim şirketimiz.
	BuyerCompanyId: (int) Müşteri şirketi.
	PreparedByUserId: (int) Teklifi hazırlayan personel.
	ApprovedByUserId: (int) Faturayı onaylayan/kesen personel.
	IssueDate: (DateTime) Fatura kesim tarihi.
	DueDate: (DateTime) Son ödeme tarihi,Varsaılan olarak fatura tarihinden sonra 15 gün.
	axTotal: (decimal) Toplam KDV.
	GrandTotal: (decimal) KDV dahil genel toplam.
	EInvoiceUuid: (Guid?) E-fatura entegrasyonu için benzersiz ID.
	Status: (Enum) Draft, Sent, Paid, Cancelled.

InvoiceItem : BaseEntity (Tekliften kopyalanır ama fatura anındaki kesinleşmiş fiyatları tutar)
	ProductId, Quantity, UnitPrice, VatRate, TotalPrice (Qty * UnitPrice).

Stok Hareketi Mantığı: Eğer MovementType 1 (Entry) ise SupplierId zorunludur. Eğer MovementType 2 (Exit) ise CustomerId zorunludur. Her iki alan da veritabanında nullable olmalıdır ancak hareket tipine göre FluentValidation ile kontrol edilmelidir.
MovementType Transfer olduğunda sistem TargetWarehouseId alanını zorunlu tutmalıdır. Bu durumda iki kayıt oluşturulmalıdır: kaynak depodan bir çıtı ve hedef depoya bir girdi.

PriceList:BaseEntitiy;
	ProductId:(int, Zorunlu)	
	PurchasePrice: (decimal, Zorunlu)
	SalePrice: (decimal, Zorunlu)
	DiscountRate: (decimal, Default: 0)
	VatRate: (int)
	Currency: (string)
	IsActivePrice: (bool)

StockAlert:BaseEntitiy;
	ProductId: (int, Zorunlu)
	MinStockLevel: (decimal, Zorunlu)
	MaxStockLevel: (decimal, Opsiyonel)
	IsNotificationSent: (bool, Default: false)
Check on Movement: Her StockMovement (Stok Hareketi) gerçekleştiğinde, sistem ilgili ürünün CurrentStock miktarını StockAlert tablosundaki MinStockLevel ile karşılaştırır.
Alert Trigger: Eğer stok kritik seviyenin altındaysa, loglarda veya UI'da "Warning: Low stock for [ProductName]" uyarısını basar.

Product : BaseEntity
	ProductName: (string, Zorunlu)
	ProductCode: (string, Zorunlu, Unique)
	CategoryId: (int, Zorunlu) -> Category tablosuna bağlı.
	BrandId: (int, Zorunlu) -> Brand tablosuna bağlı.
	UnitId: (int, Zorunlu) -> ProductUnit tablosuna bağlı.
	WarehouseId: (int, Zorunlu) -> Warehouse tablosuna bağlı.
	LocationId: (int, Opsiyonel) -> ProductLocation (Raf/Göz) tablosuna bağlı.
	CurrentStock: (decimal, Default: 0) Ürünün anlık toplam stoğu.
Price Integration: Ürün ilk oluşturulduğunda girilen fiyat, otomatik olarak PriceList tablosuna "Active" flag'i ile kaydedilmeli.
Stock Movement: CurrentStock alanı asla manuel elle güncellenmemeli; sadece StockMovement tablosuna kayıt girildiğinde tetiklenerek değişmeli.
Alert Link: Ürün bazlı kritik stok seviyeleri StockAlert tablosu üzerinden kontrol edilmeli.

Offer:BaseEntity
	OfferNumber: (string) Otomatik artan teklif numarası
	CustomerId: (int) Teklifin hangi müşteriye verildiği.
	OfferDate: (DateTime) Teklif tarihi. varsayılan hazırlanan günün tarihi.
	ValidUntil: (DateTime) varsayılan olarak hazırlanan günün 15 gün sonrası.
	TotalAmount: (decimal) Teklifin toplam tutarı.
	Status: (Enum) 1: Pending (Beklemede), 2: Approved (Onaylandı), 3: Rejected (Reddedildi).

OfferItem : BaseEntity
	OfferId: (int) Hangi teklife ait?
	ProductId: (int) Hangi ürün?
	Quantity: (decimal) Miktar.
	UnitPrice: (decimal) Teklif anındaki birim fiyat (PriceList'ten çekilir ama değiştirilebilir olmalı).
	DiscountRate: (decimal) Satır bazlı indirim.

Teklif Onay Mantığı: Bir teklifin durumu “Onaylandı” olarak güncellendiğinde, sistem teklif içindeki her bir kalem için otomatik olarak bir Stok Hareketi (Çıkış) kaydı oluşturmalı ve böylece CurrentStock (mevcut stok) azaltılmalıdır.

Teklif Sistemi Kuralları:
Esneklik: Teklifler, stok seviyelerinden bağımsız olarak oluşturulabilir.
Stok Uyarısı: Teklif oluşturma sırasında, eğer miktar (Quantity) mevcut stoktan (CurrentStock) büyükse uyarı gösterilmeli ancak işlem engellenmemelidir.
Dönüşüm Mantığı:
Bir teklif “Onaylandı” durumuna geçtiğinde stok uygunluğu kontrol edilmelidir.
Eğer stok yeterliyse: Sistem otomatik olarak Stok Hareketi (Çıkış) oluşturmalıdır.
İçinde Müşteri bilgisi Teklifi hazırlayan kişi bilgisi ürünlerin bilgisi olan bir faturalama çıkarılması gereklidir.
Eğer stok yetersizse: Kullanıcıya “Negatif Stoka İzin Ver” veya “Tedarik Bekle” seçenekleri sunulmalıdır.
Negatif Stok Bayrağı: Bu davranışı küresel olarak kontrol etmek için Company veya Warehouse tablosuna AllowNegativeStock adında bir boolean alan eklenmelidir.

UYGULAMA İÇİ KURALLAR:

Kullanıcı Rolleri:
Root: 
Programın içinde veri tabanı adı kullanıcı adı şifresi değiştirme yetkisi olsun
Yeni kullanıcı oluşturabilir ve bu kullanıcıya rol atayabilir. Admin kullanıcısı, yeni User kullanıcısı yetkisi oluşturma yetkisi olsun ve bu yetkiler için rolleri değiştirebilsin,
kullanıcıların hangi verilerde değişiklik yapabilip hangi veride yazma hangi veride okuma yetkisi olup olmadığı yetki değişikliğini yapabilsin.
etki: Tam erişim.
Görünürlük: Root, sistemdeki tüm Admin ve User kayıtlarını görür.
Kod Tarafı: Root için hiçbir Where filtresi uygulanmaz. Sadece RoleName == 'root' kontrolü yeterlidir.
programa tam erişim ve diğer kullanıcılar için rol atama özelliği. Sistemin hiçbir yerinde root kullanıcısı görünmeyecek sadece bilenin girebileceği bir kullanıcı.
Sistemin hiçbir yerinde root kullanıcısı görünmeyecek" kuralı çok önemli. Bu, User tablosunda yapılacak sorgularda (GetallUsers gibi) her zaman WHERE RoleName != 'root' filtresinin ekle
Veri seviyesinde güvenlik: Kullanıcıların kayıtlar üzerindeki düzenleme yetkilerini, sahip oldukları rollere, hiyerarşideki konumlarına veya kayıt sahipliğine göre sınırlandırın.

Admin: Programa yeni kullanıcı tanımlama girilen verilerde değişiklik hakkı ve diğer user kullanıcılarında değişiklik yapabilip yapamama hakkı, kendine verilen hakları userlere verebilme hakkı.(Admin kullanıcısnın Company ekleme çıkarma hakkı var ise o da oluşturduğı usere aynı yetkiyi isterse verebilir.)

User : Admin yada Root kullanıcısı tarafından oluşturulabilir izin verilen bölümler için sadece erişim hakkı vardır yada değişiklik yapma hakkı vardır.

Güvenlik Alanında En İyi Uygulamalar (Security Best Practices)
Ham SQL Kullanımı Yasaktır: SQL Enjeksiyonu (SQL Injection) riskini bertaraf etmek amacıyla yalnızca EF Core LINQ sağlayıcıları kullanılmalıdır. Ham SQL (Raw SQL) kullanımı kesinlikle yasaktır.
Giriş Doğrulama (Input Validation): Katı kuralları (örneğin; e-posta formatı, karakter uzunluğu ve HTML betiği içermeme durumu) uygulamak için FluentValidation kütüphanesi kullanılmalıdır.
BCrypt Tuzlama (Salting): Parolalar, yüksek maliyet faktörü (high cost factor) kullanılarak hash'lenmelidir.
Sorgu Filtreleme (Query Filtering): Yetkisiz veri erişimini engellemek için IsDeleted = false ve IsActive = true koşullarını içeren Global Sorgu Filtreleri uygulanmalıdır.
Hız Sınırlama (Rate Limiting): Kimlik doğrulama uç noktalarına (endpoints) yönelik DDoS saldırılarını önlemek amacıyla API katmanında hız sınırlama mekanizması bulunmalıdır.
API üzerinden gelen isteklerde, kullanıcının sadece UserApiKey tablosundaki aktif anahtarlarla yetkilendirilmesini sağlayan bir "API Key Authentication Filter" mekanizmasının API katmanına eklenmesi gereklidir.

İşlem (Transaction) ve Veri Tutarlılığı Kuralları
Birden fazla tabloyu etkileyen tüm iş süreçleri, tek bir atomik veritabanı işlemi (transaction) dahilinde yürütülmelidir.
Bir işlem (transaction) şunları garanti etmelidir:
Tüm işlemlerin başarıyla tamamlanması,
Veya işlemlerin hiçbirinin veritabanına kaydedilmemesi (Rollback).
Kısmi veri kalıcılığına (partial data persistence) izin verilmesi kesinlikle yasaktır.

Zorunlu İşlem (Transaction) Senaryoları
Aşağıdaki operasyonların yürütülmesi esnasında transaction kullanımı zorunludur:
Stok Hareket Operasyonları
Stok Hareket (StockMovement) kaydının oluşturulması,
Mevcut Stok (Product.CurrentStock) bilgisinin güncellenmesi.
Bu iki eylem, aynı işlem (transaction) dahilinde icra edilmelidir. Eylemlerden birinin başarısız olması durumunda, diğeri mutlaka geri alınmalıdır (rolled back).

Depo Transfer Operasyonları
Bir transfer operasyonu aşağıdaki süreçlerden oluşmaktadır:
Kaynak depodan bir adet "Çıkış" (Exit) hareketi,
Hedef depoya bir adet "Giriş" (Entry) hareketi.
Her iki hareketin de aynı işlem (transaction) dahilinde oluşturulması zorunludur.
Herhangi bir hareketin başarısız olması durumunda, tüm transfer operasyonu geri alınmalıdır (rolled back). Kısmi transfer işlemlerine (partial transfers) kesinlikle izin verilmez.

Teklif Onay Mantığı (Offer Approval Logic)
Bir teklif durumu "Onaylandı" (Approved) olarak güncellendiğinde, sistem aşağıdaki işlemleri gerçekleştirmek zorundadır:
Teklif durumunun güncellenmesi,
Her bir teklif kalemi (OfferItem) için bir Stok Hareketi (Exit) oluşturulması,
Mevcut Stok (Product.CurrentStock) miktarının ilgili hareketlere göre güncellenmesi.
Tüm bu adımlar tek bir işlem (transaction) dahilinde icra edilmelidir. Herhangi bir adımın başarısız olması durumunda; teklif durumu önceki haline döndürülmeli ve hiçbir stok verisi veritabanına kaydedilmemelidir.

İşlem Yönetimi Kuralları (Transaction Handling Rules)
İşlemler (Transactions) mutlaka Uygulama / Servis Katmanında (Application / Service Layer) yönetilmelidir.
Aşağıdaki alanlarda işlem yönetimi yapılmamalıdır:
Controller (Denetleyici) içerisinde,
Repository (Depo) katmanı içerisinde.
Ayrıca, tek bir iş süreci (business operation) esnasında SaveChanges metodu birden fazla kez çağrılmamalıdır.

İşlem Öncesi Doğrulama Sırası (Validation Order Before Transaction)
Bir veritabanı işlemi (transaction) başlatılmadan önce aşağıdaki kontrollerin yapılması zorunludur:
Giriş Doğrulaması (FluentValidation): Veri formatı ve zorunlu alan kontrolleri.
İş Kuralları Doğrulaması (Business Rule Validation): Yetkilendirme kontrolleri ve stok kuralları.
Eğer doğrulama aşamalarından herhangi biri başarısız olursa, veritabanı işlemi (transaction) başlatılmamalıdır.

Hata Yönetimi (Failure Handling)
Bir işlem (transaction) hatası durumunda aşağıdaki prosedürler uygulanmalıdır:
Tüm veritabanı değişiklikleri geri alınmalıdır (Rollback).
Meydana gelen hata, kaynağı (CLI veya API) belirtilerek günlüklenmelidir (Log).
Sistem, hiçbir koşulda yetim (orphan) veya kısmi kayıt bırakmamalıdır.

Denetim İzi (Audit Trail) ve Sistem Günlükleme Kuralları
Denetim İzinin Amacı
Sistem, tüm kritik operasyonlar için eksiksiz ve değiştirilemez (immutable) bir denetim izi tutmak zorundadır. Denetim günlükleri (audit logs) şu amaçlarla gereklidir:
Sistem Suistimalinin Takibi: Olası kötüye kullanım faaliyetlerini izlemek.
Sorumluluğun Sağlanması: İşlemlerin hangi kullanıcı tarafından gerçekleştirildiğini (accountability) belgelemek.
Hata Ayıklama ve Adli Analiz Desteği: Sistem hatalarının çözümünü ve olay sonrası teknik incelemeleri (forensic analysis) desteklemek.
Root Görünmezliği ve Güvenlik Kurallarının Uygulanması: Üst düzey yetkili (Root) hesapların gizliliğini ve güvenlik protokollerini denetlemek.

System vs Root Execution Rules
The system MUST clearly distinguish between System-initiated operations and Root user actions.
Definitions:
Root User:
A privileged human user created intentionally by the system owner.
Requires authentication (login).
Performs administrative and operational actions consciously.
System:
Represents automatic, internal application processes.
Does NOT authenticate and does NOT represent a human user.
System-Initiated Operations:
The following operations MUST be considered System actions:
Database migrations
Initial database setup
Reflection-based module synchronization
Automatic license validation
Background or startup-triggered maintenance tasks
Audit Logging Rules for System Actions:
System actions MUST be audited with:
ActorRole = "System"
ActorUserId = NULL (or a predefined non-human constant, e.g. -1)
Source = "System"
System actions MUST NEVER be attributed to the Root user.
Root User Actions:
Root actions MUST always require explicit authentication.
Root actions MUST be audited with:
ActorUserId = Root User ID
ActorRole = "Root"
Source = "CLI" or "API"
Security & Visibility Rules:
System actions MUST NOT appear as Root actions in audit logs.
Root user invisibility rules MUST remain intact and MUST NOT be bypassed by System operations.
This separation ensures accurate audit trails, preserves Root user confidentiality, and prevents misleading or forged administrative activity records.

AuditLog Varlık (Entity) Gereksinimleri
Sistem, aşağıda belirtilen sorumluluklara sahip özel bir AuditLog varlığı içermek zorundadır. Her bir denetim kaydı aşağıdaki verileri kapsamak zorundadır:
ActorUserId (İşlemi Yapan Kullanıcı): İşlemi gerçekleştiren kullanıcının kimliği (sistem tarafından yapılan otomatik işlemler için boş bırakılabilir).
ActorRole (Kullanıcı Rolü): İşlemi yapanın yetki seviyesi (Root / Admin / User / System).
ActionType (İşlem Türü): Gerçekleştirilen eylemin tipi (Oluşturma / Güncelleme / Silme / Onaylama / Reddetme / Giriş / Çıkış).
EntityName (Varlık Adı): İşlemden etkilenen varlığın ismi (Örneğin; Ürün, Kullanıcı, Teklif).
EntityId (Varlık Kimliği): Etkilenen kaydın ID numarası (uygulanamaz durumlarda boş bırakılabilir).
ActionDescription (İşlem Açıklaması): İşlemin insan tarafından okunabilir kısa bir özeti.
Source (Kaynak): İşlemin tetiklendiği platform (CLI / API).
CreatedAt (Oluşturulma Tarihi): UTC zaman diliminde işlemin gerçekleşme tarihi ve saati.
IPAddress (IP Adresi): İşlemin yapıldığı cihazın IP adresi (erişilebilir olduğu durumlarda).

Mandatory Audited Operations
The following operations MUST ALWAYS generate an audit log entry:
Authentication & Security
Login attempts (success & failure)
Account lockout and unlock
Password change
API Key creation, revocation, expiration
User & Permission Management
User creation, update, delete
Role assignment or change
UserPermission changes (Read / Write / Delete)
System & Configuration
Database connection changes
Migration execution
SystemSetting updates
Company / Warehouse configuration changes
Business Operations
StockMovement creation (all types)
Offer approval or rejection
PriceList activation/deactivation
StockAlert trigger events

Audit Logging Rules
Audit logs MUST be written within the same transaction as the business operation
If audit logging fails, the entire transaction MUST fail
Audit logs MUST NOT be soft-deleted
Audit logs MUST NOT be editable or updatable
No system component is allowed to bypass audit logging

Source Identification Rule
Each audit record MUST explicitly identify its source:
CLI operations MUST be logged with Source = "CLI"
API operations MUST be logged with Source = "API"
Automatic system actions (migrations, background jobs) MUST use Source = "System"


Root User Audit Policy
Root user actions MUST be audited like all other users
Root user data MUST NOT be exposed through standard User queries
Audit logs MAY contain Root actions but MUST NOT expose Root credentials


Logging Levels
The system MUST support structured logging with the following levels:
Information: Normal operations
Warning: Suspicious or abnormal behavior (low stock, failed login)
Error: Transaction failure, validation failure, migration error
Critical: Security breach attempts, data corruption risk

Completion Criteria
This requirement is considered complete if:
Every critical operation produces an audit record
Audit logs are immutable and queryable
CLI and API actions are clearly distinguishable
No business operation can succeed without audit logging


Authorization Model Overview
The system MUST enforce authorization using a Module-based permission model.
Authorization decisions MUST be made based on:
User role (Root / Admin / User)
Assigned module permissions (Read / Write / Delete)
Role-based access alone is NOT sufficient.

Module Definition Rules
Each functional area of the system MUST be represented as a Module
Modules MUST be unique by ModuleName
Modules MUST be discoverable via Reflection at application startup
New modules MUST be automatically inserted into the Modules table if missing
Modules MUST NOT be hardcoded in the database manually.


UserPermission Rules
Permissions MUST be evaluated using the UserPermission table.
Each permission record defines:
Which user
Which module
Allowed actions (Read / Write / Delete)
If no permission record exists:
Access MUST be denied by default


Root Authorization Rules
Root user MUST have implicit full permissions on all modules
Root permissions MUST NOT be stored explicitly in UserPermission table
Root authorization MUST bypass module permission checks
Root user MUST NOT be selectable, assignable, or visible in any permission management UI.



Admin Authorization Rules
Admin users MUST have full Read / Write / Delete permissions by default
Admin permissions MAY be overridden explicitly if required
Admin users MAY manage:
Users (except Root)
Roles and permissions
Modules and assignments



User Authorization Rules
User permissions MUST be explicitly defined per module
Users MUST NOT have access to modules without permission records
Users MAY have:
Read-only access
Write access
Delete access
Access decisions MUST be evaluated at runtime.

Authorization Enforcement Points
Authorization MUST be enforced at the following levels:
API Layer (before controller action execution)
CLI Command execution
Service layer (business logic protection)
UI-level authorization alone is strictly forbidden.

Authorization Failure Handling
If a user attempts an unauthorized action:
The operation MUST be blocked
An audit log MUST be generated
For API: return a proper forbidden response (e.g. HTTP 403)
For CLI: display "Access denied for this operation"


Permission Evaluation Order
When evaluating access:
Check if user is Root → allow
Check if user is Admin → allow (unless explicitly restricted)
Check UserPermission for the target module
Deny by default

Completion Criteria
This requirement is considered complete if:
No unauthorized operation can reach the service layer
All permission checks are centralized
Root user access is implicit and hidden
User permissions are enforced consistently in API and CLI

Error Handling, Exception Strategy & User-Safe Messages

General Error Handling Rules
The system MUST use a centralized exception handling mechanism.
Exceptions MUST NOT be handled individually inside:
Controllers
CLI command handlers
Repositories
All unhandled exceptions MUST propagate to a global handler.

Error Categorization
Errors MUST be categorized as:
Validation Errors
Authorization Errors
Business Rule Violations
Concurrency Conflicts
System / Infrastructure Errors
Each category MUST have a deterministic handling strategy.

Validation Errors
Triggered by FluentValidation
MUST be returned without stack trace
MUST include field-level messages
MUST NOT expose internal model names
API Response:
HTTP 400 Bad Request
CLI Output:
Display validation message in a readable format
Do NOT terminate the application

Authorization Errors
Triggered when permission checks fail
API Response:
HTTP 403 Forbidden
CLI Output:
"Access denied for this operation."
Authorization errors MUST be logged as security events.

Business Rule Violations
Examples:
Insufficient stock during Offer approval
Invalid state transitions
Logical constraints violations
Handling Rules:
MUST NOT throw raw system exceptions
MUST return controlled domain errors
MUST include a human-readable explanation
API Response:
HTTP 409 Conflict or HTTP 422 Unprocessable Entity
CLI Output:
Display business error message clearly

Concurrency Conflicts
Triggered when:
RowVersion mismatch occurs
Concurrent updates detected
Handling Rules:
MUST NOT auto-retry
MUST NOT overwrite data
MUST explicitly fail
API Response:
HTTP 409 Conflict
Message: "The record has been modified by another operation. Please retry."
CLI Output:
Display a concurrency warning
Abort the operation safely

System / Infrastructure Errors
Examples:
Database connection failure
Migration failure
IO errors
Configuration errors
Handling Rules:
MUST log full technical details internally
MUST return generic messages to users
API Response:
HTTP 500 Internal Server Error
Message: "An unexpected error occurred. Please contact support."
CLI Output:
Display short error message
Suggest retry or system check

Logging Rules
All errors MUST be logged with:
Timestamp
Source (API / CLI)
UserId (if available)
Operation name
Error category
Sensitive data MUST NOT be logged:
Passwords
Tokens
Connection strings
Secret keys


Stack Trace Exposure Rules
Stack traces MUST NEVER be returned to:
API clients
CLI users
Stack traces MAY be stored internally for diagnostics.

Exception-to-Response Mapping
Exception handling MUST include a deterministic mapping between:
Exception type
HTTP status code
CLI output format
This mapping MUST be centralized and consistent.

Completion Criteria
This requirement is considered complete if:
No raw exception messages are exposed to users
Errors are categorized and handled consistently
Logs contain sufficient diagnostic information
Business logic errors are distinguishable from system failures


Mandatory Concurrency-Controlled Entities
The following entities MUST include a concurrency control mechanism:
Product (CurrentStock)
StockMovement (logical consistency)
Offer (Status updates)
PriceList (Active price switching)

Concurrency Strategy
The system MUST use Optimistic Concurrency Control
Each concurrency-controlled entity MUST include:
A RowVersion / ConcurrencyToken field
The database MUST reject updates when the record version has changed since it was read

Stock Update Concurrency Rules
When updating Product.CurrentStock:
The system MUST verify that the record version has not changed
If the version has changed:
The operation MUST fail
The transaction MUST be rolled back
A concurrency conflict message MUST be returned
Silent overwrites are strictly forbidden.

Conflict Handling Behavior
In case of a concurrency conflict:
The system MUST NOT retry automatically
The system MUST notify the caller that:
"The record has been modified by another operation. Please retry."

Non-Negotiable Rules
Pessimistic locking (database locks) MUST NOT be used
Forced overwrite behavior is forbidden
Concurrency conflicts MUST be explicit and visible

Completion Criteria
This requirement is considered complete if:
Concurrent stock updates never result in incorrect values
Stock movements always reflect the correct final stock state
Conflicting operations are detected and rejected explicitly

For CLI:
Display a clear warning message
For API:
Return a proper conflict response (e.g. HTTP 409)

Offer & Stock Concurrency Rule
When an Offer is approved:
Stock availability MUST be validated again at approval time
Even if stock was sufficient during offer creation
Approval MUST fail if a concurrent stock change makes it invalid (unless negative stock is explicitly allowed)

___________________________________________________________________________________________________________________________________



Uygulama ayağa kalktığı zaman ilk olarak lisans kontrolü yapacak her seferinde. Eğer geçerli lisans yok ise dencrypt adımına geçecek sistem. Lisans sayfası açılacak 
Not:Sisteminin "donanım sabiti" bulurken şu iki noktaya dikkat etmesi gerekir:
	Windows/Linux için: Anakart Seri Numarası (UUID) ve İşlemci ID (CPU ID) en stabil ikilidir.
	Docker/Container için: Konteyner her yeniden başladığında ID değişebilir. Bu yüzden Docker ortamında MAC Adresi veya ana makineden (host) map 	edilmiş bir Volume ID kullanmak daha garantidir.
her kullanıcı eklendiği zaman da lisansa bakacak ve kullanıcıyı lisans müsait ise ekleyecek eğer müsait değilse kullanıcı eklenmesine izin vermeyecek. kullanıcıya hata döndürülecek (Uygulama hali hazırda açıksa ve kullanıcı ekleme sayfasındaysa Ekrana küçük bir hata fırlat Kullanıcı eklenemez Lisans limitiniz dolu.
Kullanıcı sayısı lisans sınırına ulaştı. yeni kullanıcı ekleme sayfasına girilemeyecek.

encrypt dosya oluşturma
Lisans kontrolü kurulu olduğu ortamda (windows,Docker containner,Linux bir ortam) Ortak değişmeyecek bir sabit bulacak(bu sabit donanıma bakmak zorunda varsa 2 sabit daha iyi olur amaç sunucunun yada ortamın clone edilip uyuglamanın başka bir ortamda çalışmasını engellemek) ve bunu Lisans sayfasında Download Licence key altında şifreli dosya ile front end in açıldığı bilgisayara indirebilecek.

dencrypt uygulama oluşturma
benim yazmış olduğum lisans uygulamasında oluşturulan şifreli dosyayı yükleyeceğim ve içinde değişmeyen sabit ve lisans paketi olan bir dosya oluşturacağım ve bunu S2O1 uygulamasında oluşturulacak olan licence sayfasına upload edeceğim. Eğer değişmeyen donanım sabiti ile dosyadaki değerler eşleşiyorsa istenen paket sisteme eklenecek ve sistem yeni lisansları ile çalışacak.

Basic Lisans: Ugulama 5 kullanıcı için çalışır
Demo Lisans Uygulama 5 kullanıcı için kurulduktan 1 ay sonra kendini kapatacak ve lisans girme ekranı gelecek sadece şekilde kapanacak API leri pasife çekecek kesinilkle lisans giriş sayfasından başka bir sayfaya yada API ye erişim sağlanamayacak.
Genişleme Lisansı: 1-10-50-100 kişilik paketler halinde olacak.



Gerekli olan katmana yaz bu uygulamayı;
2S1O CLI Modülü: Fonksiyonel Akış ve Komut Yapısı


CLI Execution Context Rules
The CLI application does NOT rely on DbContext to determine the current user, role, or execution source.
DbContext is responsible only for database access and entity tracking.
User identity, role, permissions, and request source MUST be provided explicitly to the application layer.
Execution Context Definition:
Each CLI command execution MUST carry an Execution Context.
The Execution Context MUST include:
	CurrentUserId
	CurrentUserRole (Root / Admin / User)
	Source = "CLI"

Context Initialization:
The Execution Context MUST be created immediately after a successful 2S1O login command.
The authenticated user information MUST be stored in memory for the duration of the CLI session.
The CLI is session-less between executions, but stateful during a single runtime session.

Context Propagation Rules:
All Application / Service layer operations MUST receive the Execution Context explicitly.
Authorization checks, audit logging, and business rules MUST rely on the Execution Context and NOT on DbContext.
DbContext MUST remain user-agnostic and MUST NOT contain user identity or permission logic.

Audit & Authorization Integration:
All CLI-triggered operations MUST be audited with:
ActorUserId = CurrentUserId
ActorRole = CurrentUserRole
Source = "CLI"
Authorization decisions MUST be evaluated using the Execution Context before any business operation is executed.
This rule ensures consistent behavior between CLI and API executions and prevents user identity leakage into the persistence layer.





1. Giriş ve Global Erişim
Global Command: Uygulama Windows PATH değişkenine eklenmelidir. Terminale herhangi bir dizinde 2S1O yazıldığında uygulama başlamalıdır.
Landing Page: Argümansız çalıştırıldığında (> 2S1O) veritabanından SystemSetting tablosunu okur.
Console.ForegroundColor = ConsoleColor.Cyan ile logoyu basar.
Console.ForegroundColor = ConsoleColor.White ile hoş geldiniz mesajını ve veritabanı durumunu (Connected/Disconnected) gösterir.
System Work Mode: Master/Slave

2. Komut Argümanları (Arguments)
Uygulama parametrelerle çalışabilmelidir:

2S1O login: Kullanıcıyı kimlik doğrulamaya zorlar.

3. Root CLI Menüsü (Giriş Sonrası)
Kullanıcı 2S1O login yazıp root bilgileriyle giriş yaptıktan sonra terminal şu etkileşimli menüyü sunmalıdır:

Bütün menüde exit komutunu yada seçeneğini koymayı unutma. ayarların herhangi bir yerinde tamamlamadan çıkış yapabilmek istiyorum.

[MAIN MENU]
1.Db-setup: Doğrudan Veritabanı Sihirbazı (Wizard) ekranını açar.
	View Current Connection String.
	Update Server/User/Password.
	Test Connection.

2.API Key Management:Root kullanıcısı için front end ile uğraşmadan direk API ile haberleşmek için
	Create New API Key: Root kullanıcısı için yeni bir UserApiKey üretir ve ekrana basar (SecretKey dahil).
	List Active Keys: Aktif anahtarları ve son kullanma tarihlerini listeler.
	Revoke Key: Belirli bir API Key'i pasife çeker (IsActive = false).

3.System Work Mode:Active Mode:Master/Slave Burada anlık çalıştığı modu yaz (Bu sistemde Master / Slave ayrımı bir veri replikasyonu veya iş kuralı ayrımı değildir.)
    
	1.Master(Sistem Stand alone çalışacak şekilde ayarlanacak Client tarafından(front end olabilir windows form olabilir uzak bir client olabilir) cevap verecek.
	2.Slave (sisteme sadece belirlenen IP den veya Domainden gelen istekler cevap verecek şekilde ayarlanacak)(Slave is NOT read-only Bu sistem DB replication değildir”)
		1.Set MasterIP (istenen kadar IP girilebilir olmalı) exit komutu ile IP girmeden Menüden çıkılabilmeli
		2.Set Enter MasterDomain (istenen kadar Domain girilebilir olmalı) exit komutu ile IP girmeden Menüden çıkılabilmeli
Not:ayarlar SystemSetting tablosunda tutulacak. Program ayağa kalkarken (Startup/Program.cs):
    SystemSetting tablosundaki UsageType kontrol edilecek. Varsayılan Master
    Eğer Slave ise, .NET'in CORS Policy servisi dinamik olarak sadece tablodaki IP/Domain'i AllowAnyOrigin yerine listeye ekleyecek.
    Böylece başka bir IP'den gelen istekler daha uygulamanın kapısından (Middleware) girmeden reddedilecek.
	Slave modda sistem sadece belirli IP/Domainlere izin verecek. Antigravity bunu statik bir liste olarak değil, SystemSetting tablosundan her başlangıçta okunan dinamik bir CORS Policy olarak kurgula	

4.Licence Settings
	1.Disable licence check
	2.Enable Licence check

System Statistics:
Total Products / Stock Alerts Count.
Connected Clients (Last 24h).
Security Reset:
Unlock User (Hatalı girişten kilitlenen bir Admin/User'ın kilidini açar).

5.Log out: Exit CLI.


Teknik Gereksinim (CLI Logic)
Session-less: CLI üzerinden yapılan her kritik işlemde, root yetkisi o anki oturum (Session) boyunca bellekte tutulmalıdır.
Password Masking: Şifre istenirken karakterler ekrana basılmamalı (veya * olarak basılmalı).
Logging: CLI üzerinden yapılan tüm "Database Update" veya "API Key Creation" işlemleri, sistem loglarına Source: CLI etiketiyle kaydedilmelidir.



Depo Yönetimi

Diğer entitiye bağımlı yetkiler dışında Yetki modülleri altına depo yetkisi oluştur 
bu yetkiye sahip olan kullanıcılar artık sistemde depocu olacak. ancak istenirse elle farklı yetkiler yine verilebilir.

Depo altına Raf ekleme menüsü getir depo altına raf ekleyebileyim.
Depoya eklenen ürünler için otomatik bir kod oluşsun ve o ürünün uniq kodu artık o
ürün barkod veya qr da ürünün hangi şirket deposunda olduğu ürünün rafı. ürünün ne olduğu bilgisi olacak.

Ayarlar menüsünde sistem ayarları altında ayar eklemek istiyorum. 
Bu ayara bağlı olarak sistem ürün kodunu ya QR kod oluşturacak yada barkod oluşturacak.

Depocular ne yapacak;

Kullanıcı fatura kestiği zaman depo yetkisi olan kişilerin Dashboar'ına satış düşsün ve Depocu bu satışın ürünlerini depodan çıkarmak için işi üzerine alsın.
depocu işi üzerine aldığı zaman;
Satışı yapılan ürünlerin kodu, hangi depoda, hangi rafta olduğu liste olarak depocuda gözüksün.
Depocu için bir sepet oluşturulsun ve depocu ürünler için oluşan barkod/qr kod veya ürünleri elle seçerek ürünleri sepete eklesin sepet tamamlandıktan sonra  depodan çıkış işlemini onaylasın. onay ekranı sonrası teslim eden. ilgili depocu teslim alan Textbox ta isim soy isim alınacak ve sepete eklenen ürünler için irsaliye oluşacak irsaliyede satışın yapıldığı şirket bilgileri ürün markası kodu ve hangi üründen kaç birim olduğu yazacak. ürünler için irsaliyeye ekle/ekleme seçeneği olsun ürün ekleme sayfasında bu seçenek çıksın çünkü işçilik gibi soyut kavramlar depoda olmayacağı için irsaliyeye eklenmeyecek.

Giden irsaliyeler ekranında irsaliyelerin yanında PDF butonu olsun ve butona bastığımızda dosyanın pdf ini o anda oluştursun ve indirsin, teklif yönetimi altında faturalanmış teklifler için hala faturalarştır butonu gözüküyor teklifler ekranında filtre olsun teklifin 3 durumu içinde ayrı onaysız/onaylı/faturalandırmış şeklinde. Faturalandırılmış teklifler için düzenle ve sil faturalaştır butonu çıkmasın. faturalandırılmış teklifler için fatura sayfasında onaylanmamış faıra için reddet butonu olsun ve reddeilen fatura teklif ekranına onaysız olarak geri düşsün

