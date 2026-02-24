using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using S2O1.DataAccess.Contexts;
using S2O1.Domain.Common;
using S2O1.Core.Interfaces;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting; // IWebHostEnvironment

namespace S2O1.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemController : ControllerBase
    {
        private readonly S2O1DbContext _context;
        private readonly IWebHostEnvironment _env;

        public SystemController(S2O1DbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet("whoami")]
        public IActionResult WhoAmI([FromServices] S2O1.Core.Interfaces.ICurrentUserService userService)
        {
            return Ok(new
            {
                userId = userService.UserId,
                userName = userService.UserName,
                userRole = userService.UserRole,
                isRoot = userService.IsRoot
            });
        }

        [HttpGet("info")]
        [Filters.Permission(new[] { "System", "Reports" }, "Read")]
        public async Task<IActionResult> GetSystemInfo()
        {
            bool dbStatus = false;
            string dbError = null;
            try
            {
                dbStatus = await _context.Database.CanConnectAsync();
            }
            catch (Exception ex)
            {
                dbError = ex.Message;
            }

            string version = "Unknown";
            if (dbStatus)
            {
                var setting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == "AppVersion"); // SettingKey check? Usually unique keys. but 'AppVersion' column exists in SystemSetting too.
                // Wait, SystemSetting entity has AppVersion column AND SettingKey/Value. Which one stores version? Or both?
                // Default seed: SettingKey="CLI_Welcome_Message", AppVersion="v1.0.0".
                // So let's check any record's AppVersion column or specific version key.
                version = setting?.AppVersion ?? "v1.0.0";
            }

            return Ok(new 
            {
                databaseStatus = dbStatus ? "Connected" : "Disconnected",
                databaseError = dbError,
                appVersion = version,
                serverTime = DateTime.Now,
                runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
                environment = _env.EnvironmentName,
                os = System.Runtime.InteropServices.RuntimeInformation.OSDescription
            });
        }

        [HttpGet("exchange-rates")]
        [Filters.Permission("System", "Read")]
        public async Task<IActionResult> GetExchangeRates()
        {
            try
            {
                using var client = new System.Net.Http.HttpClient();
                var response = await client.GetStringAsync("https://www.tcmb.gov.tr/kurlar/today.xml");
                
                var xml = new System.Xml.XmlDocument();
                xml.LoadXml(response);

                decimal GetRate(string code)
                {
                    var node = xml.SelectSingleNode($"//Currency[@CurrencyCode='{code}']");
                    if (node == null) return 0;

                    var unitStr = node.SelectSingleNode("Unit")?.InnerText;
                    var rateStr = node.SelectSingleNode("BanknoteSelling")?.InnerText;
                    
                    // Eğer efektif satış (BanknoteSelling) boşsa normal satışı (ForexSelling) dene
                    if (string.IsNullOrEmpty(rateStr))
                        rateStr = node.SelectSingleNode("ForexSelling")?.InnerText;

                    if (string.IsNullOrEmpty(unitStr) || string.IsNullOrEmpty(rateStr)) return 0;

                    decimal unit = decimal.Parse(unitStr, System.Globalization.CultureInfo.InvariantCulture);
                    decimal rate = decimal.Parse(rateStr, System.Globalization.CultureInfo.InvariantCulture);

                    return rate / unit;
                }

                return Ok(new
                {
                    USD = GetRate("USD"),
                    EUR = GetRate("EUR"),
                    Date = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Kurlar alınamadı: " + ex.Message });
            }
        }
        [HttpGet("db-config")]
        public ActionResult GetDbConfig([FromServices] Microsoft.Extensions.Configuration.IConfiguration config, [FromServices] S2O1.Core.Interfaces.ICurrentUserService userService)
        {
            if (userService.UserId != 1) return Forbid();

            var isContainer = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux) || Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
            var connStrName = isContainer ? "DockerConnection" : "DefaultConnection";
            var connStr = config.GetConnectionString(connStrName) ?? config.GetConnectionString("DefaultConnection");
            return Ok(new { connectionString = connStr });
        }

        [HttpPost("db-config")]
        public async Task<IActionResult> UpdateDbConfig([FromBody] DbConfigDto dto, [FromServices] S2O1.Core.Interfaces.ICurrentUserService userService)
        {
            if (userService.UserId != 1) return Forbid();

            try
            {
                // Verify validity first?
                // Construct connection string
                var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder();
                builder.DataSource = dto.Server;
                builder.InitialCatalog = dto.Database;
                
                if (dto.IntegratedSecurity)
                {
                    builder.IntegratedSecurity = true;
                }
                else
                {
                    builder.UserID = dto.User;
                    builder.Password = dto.Password;
                    builder.IntegratedSecurity = false;
                }
                
                builder.TrustServerCertificate = true; // Force trust for local dev usually

                var newConnStr = builder.ConnectionString;

                var isContainer = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux) || Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
                var connStrName = isContainer ? "DockerConnection" : "DefaultConnection";

                // Update appsettings.json
                var filePath = System.IO.Path.Combine(_env.ContentRootPath, "appsettings.json");
                var json = await System.IO.File.ReadAllTextAsync(filePath);
                var jsonObj = System.Text.Json.Nodes.JsonNode.Parse(json);
                
                if (jsonObj["ConnectionStrings"] != null)
                {
                    jsonObj["ConnectionStrings"][connStrName] = newConnStr;
                }
                
                await System.IO.File.WriteAllTextAsync(filePath, jsonObj.ToString());

                return Ok(new { message = "Veritabanı ayarları güncellendi. Uygulama yeniden başlatılmalıdır." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("restore/{type}/{id}")]
        public async Task<IActionResult> Restore(string type, int id, [FromServices] S2O1.Core.Interfaces.ICurrentUserService userService)
        {
            // Permission check: Only Root or users with "ShowDeletedItems" (Read+Write/Full) can restore
            if (!userService.IsRoot)
            {
                var hasRestorePerm = await _context.UserPermissions
                    .Include(p => p.Module)
                    .AnyAsync(p => p.UserId == userService.UserId && 
                                   p.Module.ModuleName == "ShowDeletedItems" && 
                                   (p.CanWrite || p.IsFull));
                
                if (!hasRestorePerm) return Forbid();
            }

            BaseEntity entity = type.ToLower() switch
            {
                "product" => await _context.Products.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id),
                "brand" => await _context.Brands.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id),
                "category" => await _context.Categories.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id),
                "unit" => await _context.ProductUnits.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id),
                "warehouse" => await _context.Warehouses.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id),
                "shelf" => await _context.WarehouseShelves.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id),
                "supplier" => await _context.Suppliers.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id),
                "customercompany" => await _context.CustomerCompanies.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id),
                "customer" => await _context.Customers.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id),
                "offer" => await _context.Offers.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id),
                "invoice" => await _context.Invoices.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id),
                "user" => await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id),
                "company" => await _context.Companies.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id),
                _ => null
            };

            if (entity == null) return NotFound("Öğe bulunamadı veya bu tür için geri yükleme desteklenmiyor.");

            entity.IsDeleted = false;
            entity.IsActive = true;
            entity.UpdatedByUserId = userService.UserId;

            _context.Update(entity);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Öğe başarıyla geri yüklendi ve aktif edildi." });
        }

        public class DbConfigDto
        {
            public string Server { get; set; }
            public string Database { get; set; }
            public string User { get; set; }
            public string Password { get; set; }
            public bool IntegratedSecurity { get; set; }
        }

        [HttpGet("settings")]
        public async Task<IActionResult> GetSettings([FromServices] S2O1.Core.Interfaces.ICurrentUserService userService)
        {
            if (userService.UserId != 1) return Forbid();

            var strongPwd = await _context.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == "ForceStrongPassword");
            var barcodeType = await _context.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == "BarcodeType");

            return Ok(new 
            { 
                forceStrongPassword = strongPwd?.SettingValue == "true",
                barcodeType = barcodeType?.SettingValue ?? "QR",
                restartTime = (await _context.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == "AutoRestartTime"))?.SettingValue ?? "03:00"
            });
        }

        [HttpPost("settings")]
        public async Task<IActionResult> UpdateSettings([FromBody] SystemSettingsDto dto, [FromServices] S2O1.Core.Interfaces.ICurrentUserService userService)
        {
            if (userService.UserId != 1) return Forbid();

            async Task Upsert(string key, string value)
            {
                var s = await _context.SystemSettings.FirstOrDefaultAsync(x => x.SettingKey == key);
                if (s == null)
                {
                    _context.SystemSettings.Add(new S2O1.Domain.Entities.SystemSetting 
                    { 
                        SettingKey = key, 
                        SettingValue = value,
                        AppVersion = "v1.0.0",
                        LogoAscii = "",
                        CreateDate = DateTime.Now,
                        IsActive = true
                    });
                }
                else
                {
                    s.SettingValue = value;
                    _context.SystemSettings.Update(s);
                }
            }

            await Upsert("ForceStrongPassword", dto.ForceStrongPassword ? "true" : "false");
            await Upsert("BarcodeType", dto.BarcodeType ?? "QR");
            await Upsert("AutoRestartTime", dto.RestartTime ?? "03:00");

            await _context.SaveChangesAsync();
            return Ok(new { message = "Sistem ayarları güncellendi." });
        }

        [HttpGet("mail-settings")]
        [Filters.Permission("System", "Read")]
        public async Task<IActionResult> GetMailSettings([FromServices] S2O1.Core.Interfaces.ICurrentUserService userService)
        {
            if (userService.UserId != 1) return Forbid();

            var settings = await _context.SystemSettings
                .Where(s => s.SettingKey.StartsWith("Mail_"))
                .ToListAsync();

            var dto = new MailSettingsDto
            {
                SmtpHost = settings.FirstOrDefault(s => s.SettingKey == "Mail_SmtpHost")?.SettingValue ?? "",
                SmtpPort = int.TryParse(settings.FirstOrDefault(s => s.SettingKey == "Mail_SmtpPort")?.SettingValue, out var p) ? p : 587,
                Username = settings.FirstOrDefault(s => s.SettingKey == "Mail_Username")?.SettingValue ?? "",
                Password = settings.FirstOrDefault(s => s.SettingKey == "Mail_Password")?.SettingValue ?? "",
                EnableSsl = settings.FirstOrDefault(s => s.SettingKey == "Mail_EnableSsl")?.SettingValue == "true",
                FromEmail = settings.FirstOrDefault(s => s.SettingKey == "Mail_FromEmail")?.SettingValue ?? "",
                FromName = settings.FirstOrDefault(s => s.SettingKey == "Mail_FromName")?.SettingValue ?? "S2O1 System"
            };

            return Ok(dto);
        }

        [HttpPost("mail-settings")]
        [Filters.Permission("System", "Write")]
        public async Task<IActionResult> UpdateMailSettings([FromBody] MailSettingsDto dto, [FromServices] S2O1.Core.Interfaces.ICurrentUserService userService)
        {
            if (userService.UserId != 1) return Forbid();

            async Task Upsert(string key, string value)
            {
                var s = await _context.SystemSettings.FirstOrDefaultAsync(x => x.SettingKey == key);
                if (s == null)
                {
                    _context.SystemSettings.Add(new S2O1.Domain.Entities.SystemSetting 
                    { 
                        SettingKey = key, 
                        SettingValue = value,
                        AppVersion = "v1.0.0",
                        LogoAscii = "",
                        CreateDate = DateTime.Now,
                        IsActive = true
                    });
                }
                else
                {
                    s.SettingValue = value;
                    _context.SystemSettings.Update(s);
                }
            }

            await Upsert("Mail_SmtpHost", dto.SmtpHost);
            await Upsert("Mail_SmtpPort", dto.SmtpPort.ToString());
            await Upsert("Mail_Username", dto.Username);
            await Upsert("Mail_Password", dto.Password);
            await Upsert("Mail_EnableSsl", dto.EnableSsl ? "true" : "false");
            await Upsert("Mail_FromEmail", dto.FromEmail);
            await Upsert("Mail_FromName", dto.FromName);

            await _context.SaveChangesAsync();
            return Ok(new { message = "Mail ayarları güncellendi." });
        }

        [HttpPost("test-mail")]
        public async Task<IActionResult> TestMail([FromBody] MailSettingsDto dto, [FromServices] S2O1.Core.Interfaces.ICurrentUserService userService)
        {
            if (userService.UserId != 1) return Forbid();

            if (string.IsNullOrEmpty(dto.SmtpHost) || string.IsNullOrEmpty(dto.FromEmail))
                return BadRequest(new { message = "Mail ayarları (Host ve Gönderen) eksik." });

            try
            {
                // Ensure TLS 1.2+ is supported (SmtpClient can be picky)
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Tls13;

                using var smtp = new System.Net.Mail.SmtpClient(dto.SmtpHost, dto.SmtpPort);
                smtp.EnableSsl = dto.EnableSsl;
                smtp.UseDefaultCredentials = false;
                smtp.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                smtp.Timeout = 20000; // 20 seconds
                
                if (!string.IsNullOrEmpty(dto.Username))
                {
                    smtp.Credentials = new System.Net.NetworkCredential(dto.Username, dto.Password);
                }

                var mail = new System.Net.Mail.MailMessage();
                mail.From = new System.Net.Mail.MailAddress(dto.FromEmail, dto.FromName ?? "S2O1 Test");
                mail.To.Add(dto.FromEmail); 
                mail.Subject = "S2O1 Sistem - Sınama Maili";
                mail.Body = $"Bu bir sınama mailidir.\n\n" +
                            $"Durum: Altyapı Bağlantısı Deneniyor\n" +
                            $"Zaman: {DateTime.Now}\n" +
                            $"Sunucu: {dto.SmtpHost}:{dto.SmtpPort}\n" +
                            $"SSL: {dto.EnableSsl}\n\n";

                await smtp.SendMailAsync(mail);
                return Ok(new { message = "Sınama maili başarıyla gönderildi. Lütfen gelen kutunuzu kontrol edin." });
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException != null ? " | İç Hata: " + ex.InnerException.Message : "";
                return BadRequest(new { message = $"Mail gönderimi başarısız ({dto.SmtpHost}:{dto.SmtpPort}, SSL:{dto.EnableSsl}): {ex.Message}{inner}" });
            }
        }

        public class MailSettingsDto
        {
            public string SmtpHost { get; set; }
            public int SmtpPort { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public bool EnableSsl { get; set; }
            public string FromEmail { get; set; }
            public string FromName { get; set; }
        }

        public class SystemSettingsDto
        {
            public bool ForceStrongPassword { get; set; }
            public string BarcodeType { get; set; }
            public string RestartTime { get; set; }
        }

        [HttpPost("restart")]
        public IActionResult RequestRestart([FromServices] Microsoft.Extensions.Hosting.IHostApplicationLifetime lifetime, [FromServices] ICurrentUserService userService)
        {
            if (userService.UserId != 1) return Forbid();

            // Simple restart trigger
            Task.Run(async () => {
                await Task.Delay(2000); // Give time for response to reach client
                lifetime.StopApplication();
            });

            return Ok(new { message = "Uygulama yeniden başlatılıyor..." });
        }

        [HttpGet("health")]
        public IActionResult Health() => Ok(new { status = "UP", time = DateTime.Now });
    }
}
