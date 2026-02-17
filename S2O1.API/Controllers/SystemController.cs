using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using S2O1.DataAccess.Contexts;
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

        [HttpGet("info")]
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
    }
}
