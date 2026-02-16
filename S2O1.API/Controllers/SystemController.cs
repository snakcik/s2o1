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
    }
}
